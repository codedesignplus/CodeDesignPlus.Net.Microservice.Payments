using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.InitiatePayment;
using CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.Commands.DeleteSavedCard;
using CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.Commands.SetDefaultCard;
using CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.Queries.GetSavedCardById;
using Xunit;
using AppErrors = CodeDesignPlus.Net.Microservice.Payments.Application.Errors;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.SavedCard;

/// <summary>
/// Una tarjeta guardada solo la ve, la borra, la marca y la cobra su dueño (pendings/180).
/// </summary>
public class SavedCardOwnershipTest
{
    private readonly Mock<ISavedCardRepository> repository = new();
    private readonly Mock<IUserContext> user = new();
    private readonly Mock<IPubSub> pubsub = new();
    private readonly Guid owner = Guid.NewGuid();
    private readonly Guid stranger = Guid.NewGuid();
    private readonly SavedCardAggregate card;

    public SavedCardOwnershipTest()
    {
        card = SavedCardAggregate.Create(Guid.NewGuid(), owner, "payu-token-123", "409744******0004", "VISA", "Ana Pérez", "2027/03", "0004", owner);
        repository.Setup(x => x.FindAsync<SavedCardAggregate>(card.Id, It.IsAny<CancellationToken>())).ReturnsAsync(card);
    }

    private static CodeDesignPlus.Net.ValueObjects.Payment.PaymentMethod PayWith(string token) => CodeDesignPlus.Net.ValueObjects.Payment.PaymentMethod.Create("VISA", null, CodeDesignPlus.Net.ValueObjects.Payment.CreditCard.Create(token, "0004", "2027/03", "Ana Pérez", "777", 1));

    [Fact]
    public async Task GetById_CardOfAnotherUser_ThrowsSavedCardNotFound()
    {
        // Arrange
        user.SetupGet(x => x.IdUser).Returns(stranger);
        var handler = new GetSavedCardByIdQueryHandler(repository.Object, new Mock<IMapper>().Object, user.Object);

        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => handler.Handle(new GetSavedCardByIdQuery(card.Id), CancellationToken.None));

        // Assert
        Assert.Equal(AppErrors.SavedCardNotFound.GetCode(), exception.Code);
    }

    [Fact]
    public async Task Delete_CardOfAnotherUser_ThrowsAndDeletesNothing()
    {
        // Arrange
        user.SetupGet(x => x.IdUser).Returns(stranger);
        var handler = new DeleteSavedCardCommandHandler(repository.Object, user.Object, pubsub.Object);

        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => handler.Handle(new DeleteSavedCardCommand(card.Id), CancellationToken.None));

        // Assert
        Assert.Equal(AppErrors.SavedCardNotFound.GetCode(), exception.Code);
        repository.Verify(x => x.DeleteAsync<SavedCardAggregate>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetDefault_CardOfAnotherUser_ThrowsAndChangesNothing()
    {
        // Arrange
        user.SetupGet(x => x.IdUser).Returns(stranger);
        var handler = new SetDefaultCardCommandHandler(repository.Object, user.Object);

        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => handler.Handle(new SetDefaultCardCommand(card.Id), CancellationToken.None));

        // Assert
        Assert.Equal(AppErrors.SavedCardNotFound.GetCode(), exception.Code);
        repository.Verify(x => x.UpdateAsync(It.IsAny<SavedCardAggregate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ResolveToken_OwnSavedCard_ReplacesTheIdWithTheProviderToken()
    {
        // Arrange
        user.SetupGet(x => x.IdUser).Returns(owner);

        // Act
        var resolved = await SavedCardToken.ResolveAsync(PayWith(card.Id.ToString()), repository.Object, user.Object, CancellationToken.None);

        // Assert
        Assert.Equal("payu-token-123", resolved.CreditCard!.Token);
        Assert.Equal("777", resolved.CreditCard.SecurityCode);
        Assert.Equal(1, resolved.CreditCard.InstallmentsNumber);
    }

    [Fact]
    public async Task ResolveToken_SavedCardOfAnotherUser_ThrowsSavedCardNotFound()
    {
        // Arrange
        user.SetupGet(x => x.IdUser).Returns(stranger);

        // Act
        var exception = await Assert.ThrowsAsync<CodeDesignPlusException>(() => SavedCardToken.ResolveAsync(PayWith(card.Id.ToString()), repository.Object, user.Object, CancellationToken.None));

        // Assert
        Assert.Equal(AppErrors.SavedCardNotFound.GetCode(), exception.Code);
    }

    [Fact]
    public async Task ResolveToken_FreshProviderToken_LeavesThePaymentMethodAsIs()
    {
        // Arrange: PayU tokens are GUIDs too, but they are not the id of a saved card.
        user.SetupGet(x => x.IdUser).Returns(owner);
        var freshToken = Guid.NewGuid().ToString();
        var method = PayWith(freshToken);

        // Act
        var resolved = await SavedCardToken.ResolveAsync(method, repository.Object, user.Object, CancellationToken.None);

        // Assert
        Assert.Same(method, resolved);
    }
}
