using CodeDesignPlus.Net.Microservice.Payments.Application.Payment.Commands.InitiatePayment;
using CodeDesignPlus.Net.ValueObjects.User;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.Payment.Commands.InitiatePayment;

/// <summary>
/// The payer's email and phone are optional: a visitor paying at the front desk is known only by name and document
/// (pendings/665). When they come, they are validated as before.
/// </summary>
public class BuyerInfoDtoValidatorTest
{
    private readonly BuyerInfoDtoValidator validator = new();

    [Fact]
    public void Validate_VisitorWithoutEmailNorPhone_IsValid()
    {
        // Arrange
        var visitor = Buyer.CreateWithoutShipping(Guid.NewGuid(), "Carlos Gómez", "", "", TypeDocument.Create("CC", "Cédula de ciudadanía"), "79123456");

        // Act
        var result = validator.Validate(visitor);

        // Assert
        Assert.True(result.IsValid, string.Join("; ", result.Errors.Select(x => x.ErrorMessage)));
    }

    [Fact]
    public void Validate_PayerWithEmailAndPhone_IsValid()
    {
        // Arrange
        var payer = Buyer.CreateMinimal(Guid.NewGuid(), "Ana Restrepo", "3001112233", "ana@example.com");

        // Act
        var result = validator.Validate(payer);

        // Assert
        Assert.True(result.IsValid);
    }
}
