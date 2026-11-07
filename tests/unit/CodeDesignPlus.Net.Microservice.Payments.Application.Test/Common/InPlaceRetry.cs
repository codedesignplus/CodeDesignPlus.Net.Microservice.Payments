namespace CodeDesignPlus.Net.Microservice.Payments.Application.Test.Common;

/// <summary>
/// Stands for <c>UpdateWithRetryAsync</c> of the SDK over the instances the mock already hands out.
/// </summary>
/// <remarks>
/// For the tests that check what a handler does with one payment and nobody else writes it. A race needs a store that copies
/// on read (<see cref="VersionedPaymentStore"/>): with the same instance for everyone it cannot happen (rule 59).
/// </remarks>
internal static class InPlaceRetry
{
    /// <summary>
    /// Loads, applies the change and, if it changed something, calls <paramref name="save"/> as the write.
    /// </summary>
    /// <param name="repository">The mocked repository.</param>
    /// <param name="save">What the write does; it may throw to stand for a failing store.</param>
    public static void ReappliesInPlace(this Mock<IPaymentRepository> repository, Action<PaymentAggregate>? save = null)
    {
        repository
            .Setup(x => x.UpdateWithRetryAsync(It.IsAny<Func<Task<PaymentAggregate>>>(), It.IsAny<Func<PaymentAggregate, bool>>(), It.IsAny<CancellationToken>()))
            .Returns(async (Func<Task<PaymentAggregate>> load, Func<PaymentAggregate, bool> mutate, CancellationToken _) =>
            {
                var current = await load();

                if (current is null || !mutate(current))
                {
                    return false;
                }

                save?.Invoke(current);

                return true;
            });
    }
}
