using CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Services.Payu;

namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Test.Services.Payu;

/// <summary>
/// Covers which PayU failures are worth retrying, and whether the reason survives.
/// </summary>
/// <remarks>
/// The evaluator used to treat <b>any</b> <c>code=ERROR</c> as retryable, ignoring the <c>error</c> field
/// that PayU sends alongside it. A card with an expired date — a permanent validation error that will never
/// succeed — was retried seven times over ~26 seconds before failing.
/// <para>
/// The rule is not a hardcoded catalogue of PayU codes, which would age badly and belongs to PayU rather
/// than to us: when PayU explains what is wrong, the request is wrong and retrying cannot fix it. Only an
/// opaque failure, with no explanation, is worth another attempt.
/// </para>
/// </remarks>
public class PayuResponseEvaluatorTest
{
    [Fact]
    public void AValidationErrorIsNotRetried()
    {
        // The real case: a card whose expiration date had already passed.
        var body = """{"code":"ERROR","error":"The credit card expiration date is not valid","transactionResponse":null}""";

        Assert.False(PayuResponseEvaluator.IsRetryableError(body));
    }

    [Fact]
    public void ABadApiKeyIsNotRetried()
    {
        var body = """{"code":"ERROR","error":"Invalid credentials","transactionResponse":null}""";

        Assert.False(PayuResponseEvaluator.IsRetryableError(body));
    }

    [Fact]
    public void AnOpaqueErrorIsStillRetried()
    {
        // PayU failed and did not say why: it may be transient, so it earns another attempt.
        var body = """{"code":"ERROR","transactionResponse":null}""";

        Assert.True(PayuResponseEvaluator.IsRetryableError(body));
    }

    [Fact]
    public void AnErrorWithAnEmptyExplanationIsStillRetried()
    {
        var body = """{"code":"ERROR","error":"","transactionResponse":null}""";

        Assert.True(PayuResponseEvaluator.IsRetryableError(body));
    }

    [Fact]
    public void ASuccessfulResponseIsNotAnError()
    {
        var body = """{"code":"SUCCESS","transactionResponse":{"state":"DECLINED"}}""";

        Assert.False(PayuResponseEvaluator.IsRetryableError(body));
    }

    [Fact]
    public void AResponseThatIsNotJsonIsNotRetried()
    {
        // A gateway HTML error page, for instance. Retrying a body we cannot even parse is guesswork.
        Assert.False(PayuResponseEvaluator.IsRetryableError("<html>502 Bad Gateway</html>"));
    }

    [Theory]
    [InlineData("""{"code":"ERROR","error":"The credit card expiration date is not valid"}""", "The credit card expiration date is not valid")]
    [InlineData("""{"code":"ERROR"}""", null)]
    [InlineData("no es json", null)]
    public void TheReasonIsExtractedSoItCanReachTheCaller(string body, string? expected)
    {
        // PayU does send the reason; it used to travel in the exception's second argument and never reach
        // the ProblemDetails, so the client only ever saw "PayU returned code=ERROR".
        Assert.Equal(expected, PayuResponseEvaluator.GetErrorMessage(body));
    }
}
