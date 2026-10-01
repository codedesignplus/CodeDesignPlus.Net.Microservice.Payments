using CodeDesignPlus.Net.Microservice.Payments.Domain;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Payments.Domain.Test;

public class SoftDeleteTest
{
    [Fact]
    public void SavedCard_Delete_MarksTheCardAsDeleted()
    {
        // Arrange
        var card = SavedCardAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), "token-123", "409744******0004", "VISA", "Ana Pérez", "03/27", "0004", Guid.NewGuid());
        var deletedBy = Guid.NewGuid();

        // Act
        card.Delete(deletedBy);

        // Assert
        Assert.True(card.IsDeleted);
        Assert.False(card.IsActive);
        Assert.Equal(deletedBy, card.DeletedBy);
        Assert.NotNull(card.DeletedAt);
    }

    [Fact]
    public void Bank_Delete_MarksTheBankAsDeleted()
    {
        // Arrange
        var bank = BanksAggregate.Create(Guid.NewGuid(), "Bancolombia", "Commercial bank", "1007", true);

        // Act
        bank.Delete();

        // Assert
        Assert.True(bank.IsDeleted);
        Assert.False(bank.IsActive);
        Assert.NotNull(bank.DeletedAt);
    }
}
