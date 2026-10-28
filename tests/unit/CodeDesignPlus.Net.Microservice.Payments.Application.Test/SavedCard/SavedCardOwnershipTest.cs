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
}
