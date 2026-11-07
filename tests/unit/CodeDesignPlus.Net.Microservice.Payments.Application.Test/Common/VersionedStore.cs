using System.Reflection;
using CodeDesignPlus.Net.Core.Abstractions;
using CodeDesignPlus.Net.Mongo.Abstractions;
using CodeDesignPlus.Net.Mongo.Abstractions.Exceptions;
using CodeDesignPlus.Net.Mongo.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Moq;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.Common;

/// <summary>
/// A repository that behaves like Mongo for one versioned aggregate: every read is a copy, a plain update replaces the whole
/// document and a versioned write checks the version it read, retrying like <c>UpdateWithRetryAsync</c> in the SDK.
/// </summary>
/// <remarks>
/// That is what makes a late write erase another one (pendings/448 and 491 in ms-communitylife, where this store comes from): two processes each hold their own copy, and
/// with a plain <c>UpdateAsync</c> the last one to save wins. A mock that hands out the same instance to everyone would hide
/// the race. <see cref="BeforeNextSave"/> injects the other writer exactly between the read and the write of whoever is
/// saving, which is the window the race needs.
/// </remarks>
/// <typeparam name="TAggregate">The versioned aggregate.</typeparam>
/// <typeparam name="TRepository">The repository of the aggregate.</typeparam>
public class VersionedStore<TAggregate, TRepository>
    where TAggregate : AggregateRootBase, IVersionedAggregate
    where TRepository : class, IRepositoryBase
{
    private const int MaxAttempts = 5;

    private readonly Dictionary<Guid, BsonDocument> documents = [];

    private Func<Task>? otherWriter;

    static VersionedStore()
    {
        MongoSerializerRegistration.RegisterSerializers();

        try
        {
            BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        }
        catch (BsonSerializationException)
        {
            // Already registered by another test.
        }
    }

    public VersionedStore()
    {
        this.Repository
            .Setup(x => x.FindAsync<TAggregate>(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, Guid _, CancellationToken _) => this.Find(id)!);

        this.Repository
            .Setup(x => x.FindAsync<TAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => this.Find(id)!);

        this.Repository
            .Setup(x => x.UpdateAsync(It.IsAny<TAggregate>(), It.IsAny<CancellationToken>()))
            .Returns((TAggregate aggregate, CancellationToken _) => this.ReplaceAsync(aggregate));

        this.Repository
            .Setup(x => x.UpdateWithRetryAsync(It.IsAny<Func<Task<TAggregate>>>(), It.IsAny<Func<TAggregate, bool>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<Task<TAggregate>> load, Func<TAggregate, bool> mutate, CancellationToken _) => this.UpdateWithRetryAsync(load, mutate));
    }

    public Mock<TRepository> Repository { get; } = new();

    /// <summary>
    /// How many times a versioned write was refused because another one saved first.
    /// </summary>
    public int Conflicts { get; private set; }

    /// <summary>
    /// Stores <paramref name="aggregate"/> as it would be after its first save.
    /// </summary>
    public void Add(TAggregate aggregate)
    {
        aggregate.GetAndClearEvents();

        SetVersion(aggregate, 1);

        lock (this.documents)
        {
            this.documents[aggregate.Id] = aggregate.ToBsonDocument();
        }
    }

    /// <summary>
    /// Stores <paramref name="aggregate"/> as a document saved before the aggregate was versioned: without <c>Version</c>.
    /// </summary>
    public void AddWithoutVersion(TAggregate aggregate)
    {
        aggregate.GetAndClearEvents();

        var document = aggregate.ToBsonDocument();

        document.Remove(nameof(IVersionedAggregate.Version));

        lock (this.documents)
        {
            this.documents[aggregate.Id] = document;
        }
    }

    /// <summary>
    /// Runs <paramref name="writer"/> once, right before the next save, after whoever is saving has already read.
    /// </summary>
    public void BeforeNextSave(Func<Task> writer) => this.otherWriter = writer;

    public TAggregate? Find(Guid id)
    {
        lock (this.documents)
        {
            return this.documents.TryGetValue(id, out var document) ? BsonSerializer.Deserialize<TAggregate>(document) : null;
        }
    }

    /// <summary>
    /// Whether the stored document carries a version field.
    /// </summary>
    public bool HasVersion(Guid id)
    {
        lock (this.documents)
        {
            return this.documents[id].Contains(nameof(IVersionedAggregate.Version));
        }
    }

    private async Task<bool> UpdateWithRetryAsync(Func<Task<TAggregate>> load, Func<TAggregate, bool> mutate)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            var entity = await load();

            if (entity is null || !mutate(entity))
            {
                return false;
            }

            await this.RunOtherWriterAsync();

            if (this.TrySave(entity))
            {
                return true;
            }

            this.Conflicts++;
        }

        throw new ConcurrencyConflictException(typeof(TAggregate), Guid.Empty, 0);
    }

    private async Task ReplaceAsync(TAggregate aggregate)
    {
        await this.RunOtherWriterAsync();

        lock (this.documents)
        {
            this.documents[aggregate.Id] = aggregate.ToBsonDocument();
        }
    }

    private bool TrySave(TAggregate entity)
    {
        lock (this.documents)
        {
            var exists = this.documents.TryGetValue(entity.Id, out var document);
            var stored = exists && document!.Contains(nameof(IVersionedAggregate.Version)) ? document[nameof(IVersionedAggregate.Version)].ToInt64() : 0;

            // A new aggregate (version 0) is inserted; if the id already exists with a version, the insert is a conflict.
            if (stored != entity.Version)
            {
                return false;
            }

            SetVersion(entity, entity.Version + 1);

            this.documents[entity.Id] = entity.ToBsonDocument();

            return true;
        }
    }

    private async Task RunOtherWriterAsync()
    {
        var writer = this.otherWriter;

        this.otherWriter = null;

        if (writer is not null)
        {
            await writer();
        }
    }

    private static void SetVersion(TAggregate aggregate, long version)
    {
        // Looked up exactly as the SDK repository does, on the concrete type: a private setter declared on a base class is
        // not visible from there, and the SDK fails instead of saving (pendings/514). Finding it on the base class here hid
        // that the copies of the UVT and the minimum wage could not be saved.
        var setter = aggregate.GetType().GetProperty(nameof(IVersionedAggregate.Version), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetSetMethod(true)
            ?? throw new InvalidOperationException($"{aggregate.GetType().Name} implements {nameof(IVersionedAggregate)} but has no 'long Version {{ get; private set; }}' property.");

        setter.Invoke(aggregate, [version]);
    }
}

/// <summary>
/// The store of payments: the webhook, the reconciliation job and the daily sweep write the same one (pendings/665).
/// </summary>
internal sealed class VersionedPaymentStore : VersionedStore<PaymentAggregate, IPaymentRepository>;
