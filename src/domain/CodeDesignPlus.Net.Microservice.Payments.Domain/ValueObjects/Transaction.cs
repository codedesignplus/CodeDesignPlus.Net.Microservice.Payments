namespace CodeDesignPlus.Net.Microservice.Payments.Domain.ValueObjects;

public partial class Transaction
{
    [GeneratedRegex(@"^(\d{1,3}\.){3}\d{1,3}$")]
    private static partial Regex IpAddressRegex();

    public Order Order { get; private set; } = null!;
    public Payer Payer { get; private set; } = null!;
    public string PaymentMethod { get; private set; } = null!;
    public CreditCard CreditCard { get; private set; } = null!;
    public string DeviceSessionId { get; private set; } = null!;
    public string IpAddress { get; private set; } = null!;
    public string Cookie { get; private set; } = null!;
    public string UserAgent { get; private set; } = null!;

    public Transaction(Order order, Payer payer, string paymentMethod, CreditCard creditCard, string deviceSessionId, string ipAddress, string cookie, string userAgent)
    {

        DomainGuard.IsNull(order, Errors.OrderCannotBeNull);
        DomainGuard.IsNull(payer, Errors.BuyerCannotBeNull);
        DomainGuard.IsNullOrEmpty(paymentMethod, Errors.PaymentMethodCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(paymentMethod.Length, 32, Errors.PaymentMethodCannotBeGreaterThan32Characters);
        DomainGuard.IsNull(creditCard, Errors.CreditCardCannotBeNull);

        DomainGuard.IsNullOrEmpty(deviceSessionId, Errors.DeviceSessionIdCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(deviceSessionId.Length, 255, Errors.DeviceSessionIdCannotBeGreaterThan255Characters);


        DomainGuard.IsNullOrEmpty(ipAddress, Errors.IpAddressCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(ipAddress.Length, 39, Errors.IpAddressCannotBeGreaterThan39Characters);
        DomainGuard.IsFalse(IpAddressRegex().IsMatch(ipAddress), Errors.IpAddressMustBeValidFormat);

        DomainGuard.IsNullOrEmpty(cookie, Errors.CookieCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(cookie.Length, 255, Errors.CookieCannotBeGreaterThan255Characters);

        DomainGuard.IsNullOrEmpty(userAgent, Errors.UserAgentCannotBeNullOrEmpty);
        DomainGuard.IsGreaterThan(userAgent.Length, 1024, Errors.UserAgentCannotBeGreaterThan1024Characters);


        Order = order;
        Payer = payer;
        PaymentMethod = paymentMethod;
        CreditCard = creditCard;
        DeviceSessionId = deviceSessionId;
        IpAddress = ipAddress;
        Cookie = cookie;
        UserAgent = userAgent;
    }

    public static Transaction Create(Order order, Payer payer, string paymentMethod, CreditCard creditCard, string deviceSessionId, string ipAddress, string cookie, string userAgent)
    {
        return new Transaction(order, payer, paymentMethod, creditCard, deviceSessionId, ipAddress, cookie, userAgent);
    }
}
