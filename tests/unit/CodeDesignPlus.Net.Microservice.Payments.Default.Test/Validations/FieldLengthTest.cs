using CodeDesignPlus.Net.xUnit.Microservice.Validations;
using Xunit;

namespace CodeDesignPlus.Net.Microservice.Payments.Default.Test.Validations;

/// <summary>
/// Los campos de texto libre usan las longitudes estandar de FieldLength (regla 48 de rules/, plan 094).
/// </summary>
public class FieldLengthTest
{
    [Fact]
    public void Validators_FreeTextFields_UseStandardLengths()
    {
        var violations = StandardFieldLengths.FindViolations(
            typeof(Application.Errors).Assembly,
            // Viajan a la pasarela de pagos: su longitud la fija la pasarela, no el estandar.
            "Buyer.Name",
            "CreateSavedCardCommand.CardHolderName",
            "InitiatePaymentCommand.Description",
            "Payer.FullName",
            "TokenizeCardCommand.Name");

        Assert.Empty(violations);
    }
}
