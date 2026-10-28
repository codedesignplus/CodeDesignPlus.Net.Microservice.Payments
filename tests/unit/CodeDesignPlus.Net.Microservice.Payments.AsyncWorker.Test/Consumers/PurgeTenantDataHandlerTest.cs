using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CodeDesignPlus.Net.Core.Abstractions;
using CodeDesignPlus.Net.Microservice.Payments.AsyncWorker.Consumers;
using CodeDesignPlus.Net.Microservice.Payments.AsyncWorker.DomainEvents;
using CodeDesignPlus.Net.Microservice.Payments.Domain;
using CodeDesignPlus.Net.Microservice.Payments.Domain.Repositories;
using CodeDesignPlus.Net.Mongo.Abstractions;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Payments.AsyncWorker.Test.Consumers;

/// <summary>
/// Al purgarse una copropiedad no puede quedar en este micro ningún documento suyo, salvo la venta de licencias
/// (regla 47).
/// </summary>
public class PurgeTenantDataHandlerTest
{
    /// <summary>
    /// Los tipos que se purgan por otra vía, con su razón.
    /// </summary>
    private static readonly Dictionary<Type, string> PurgedElsewhere = new()
    {
        [typeof(PaymentAggregate)] = "Se borra con DeleteByTenantKeepingLicensesAsync para conservar la venta de licencias.",
    };

    /// <summary>
    /// Los tipos persistidos del micro que guardan la copropiedad en una propiedad <c>Tenant</c>.
    /// </summary>
    /// <remarks>
    /// Sale del dominio por reflexión y no de una lista escrita a mano: así un agregado nuevo entra solo en la
    /// prueba, y si nadie lo añade al consumidor, la prueba falla.
    /// </remarks>
    private static HashSet<Type> TypesWithTenant() => typeof(PaymentAggregate).Assembly.GetTypes()
        .Where(type => type.IsClass && !type.IsAbstract && typeof(IEntityBase).IsAssignableFrom(type))
        .Where(type => type.GetProperty("Tenant", BindingFlags.Public | BindingFlags.Instance)?.PropertyType is { } property && (property == typeof(Guid) || property == typeof(Guid?)))
        .ToHashSet();

    private static async Task<(Mock<IPaymentRepository> Repository, Guid Tenant)> PurgeAsync()
    {
        var repository = new Mock<IPaymentRepository>();
        var tenant = Guid.NewGuid();
        var handler = new PurgeTenantDataHandler(repository.Object, Mock.Of<ILogger<PurgeTenantDataHandler>>());

        await handler.HandleAsync(TenantPurgedDomainEvent.Create(tenant, "Malpelo XXI"), CancellationToken.None);

        return (repository, tenant);
    }

    [Fact]
    public async Task HandleAsync_TenantPurged_DeletesEveryTypeWithTenant()
    {
        // Act
        var (repository, tenant) = await PurgeAsync();

        // Assert
        var purged = repository.Invocations
            .Where(invocation => invocation.Method.Name == nameof(IRepositoryBase.DeleteByTenantAsync) && (Guid)invocation.Arguments[0] == tenant)
            .Select(invocation => invocation.Method.GetGenericArguments()[0])
            .ToHashSet();

        Assert.Empty(TypesWithTenant().Except(purged).Except(PurgedElsewhere.Keys).Select(type => type.Name));
    }

    [Fact]
    public async Task HandleAsync_TenantPurged_DeletesPaymentsKeepingLicenses()
    {
        // Act
        var (repository, tenant) = await PurgeAsync();

        // Assert
        repository.Verify(r => r.DeleteByTenantKeepingLicensesAsync(tenant, It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(r => r.DeleteByTenantAsync<PaymentAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void TypesWithTenant_Domain_FindsTheAggregates()
    {
        // Act
        var types = TypesWithTenant();

        // Assert
        Assert.Contains(typeof(PaymentAggregate), types);
        Assert.Contains(typeof(DisbursementAggregate), types);
    }
}
