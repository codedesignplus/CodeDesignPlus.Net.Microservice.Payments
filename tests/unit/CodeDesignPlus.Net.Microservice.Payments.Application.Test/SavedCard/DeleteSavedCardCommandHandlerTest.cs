using CodeDesignPlus.Net.Microservice.Payments.Application.SavedCard.Commands.DeleteSavedCard;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.SavedCard;

public class DeleteSavedCardCommandHandlerTest
{
    private readonly Mock<ISavedCardRepository> repository = new();
    private readonly Mock<IUserContext> user = new();
    private readonly Mock<IPubSub> pubsub = new();

    [Fact]
    public async Task Handle_ExistingCard_DeletesTheDocument()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var card = SavedCardAggregate.Create(Guid.NewGuid(), userId, "token-123", "409744******0004", "VISA", "Ana Pérez", "03/27", "0004", userId);
        user.SetupGet(x => x.IdUser).Returns(userId);
        repository.Setup(x => x.FindAsync<SavedCardAggregate>(card.Id, It.IsAny<CancellationToken>())).ReturnsAsync(card);
        var handler = new DeleteSavedCardCommandHandler(repository.Object, user.Object, pubsub.Object);

        // Act
        await handler.Handle(new DeleteSavedCardCommand(card.Id), CancellationToken.None);

        // Assert
        repository.Verify(x => x.DeleteAsync<SavedCardAggregate>(card.Id, It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(x => x.UpdateAsync(It.IsAny<SavedCardAggregate>(), It.IsAny<CancellationToken>()), Times.Never);
        pubsub.Verify(x => x.PublishAsync(It.IsAny<IReadOnlyList<IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
