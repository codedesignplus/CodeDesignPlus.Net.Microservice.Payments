using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Test.Services.Payu;

/// <summary>
/// Covers the amount format PayU uses to sign a confirmation.
/// </summary>
/// <remarks>
/// Getting this wrong is expensive and quiet: the signature no longer matches, the webhook answers 403, and
/// the payment stays charged at PayU while the platform never records it. It also looks intermittent, because
/// whether it works depends on the last cent of the amount.
/// </remarks>
public class PayUSignatureValueTest
{
    [Theory]
    // PayU drops the second decimal only when it is a zero.
    [InlineData("150.00", "150.0")]
    [InlineData("150.20", "150.2")]
    [InlineData("3773988.10", "3773988.1")]
    // ...and keeps both otherwise. This is the case the old ToString("F1") got wrong, and it rounded on top.
    [InlineData("150.26", "150.26")]
    [InlineData("4935052.09", "4935052.09")]
    [InlineData("0.01", "0.01")]
    public void TheAmountIsFormattedTheWayPayUSignsIt(string amount, string expected)
    {
        var value = decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture);

        Assert.Equal(expected, PayUAdapter.FormatValueForSignature(value));
    }

    [Fact]
    public void TheSecondDecimalIsNeverRounded()
    {
        // 4935052.09 se convertia en "4935052.1" y la firma dejaba de coincidir. Es el caso real que dejo un
        // pago cobrado y sin registrar.
        var value = decimal.Parse("4935052.09", System.Globalization.CultureInfo.InvariantCulture);

        Assert.NotEqual("4935052.1", PayUAdapter.FormatValueForSignature(value));
    }

    [Fact]
    public void TheFormatDoesNotDependOnTheMachineCulture()
    {
        // Con una cultura que usa coma decimal, formatear sin InvariantCulture produciria "150,26" y ninguna
        // firma volveria a coincidir en esa maquina.
        var original = Thread.CurrentThread.CurrentCulture;

        try
        {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("es-CO");

            Assert.Equal("150.26", PayUAdapter.FormatValueForSignature(150.26m));
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = original;
        }
    }
}
