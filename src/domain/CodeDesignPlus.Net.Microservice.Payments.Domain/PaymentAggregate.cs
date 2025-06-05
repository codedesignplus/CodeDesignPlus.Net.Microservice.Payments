namespace CodeDesignPlus.Net.Microservice.Payments.Domain;

public class PaymentAggregate(Guid id) : AggregateRoot(id)
{
    public static PaymentAggregate Create(Guid id, Guid tenant, Guid createBy)
    {
       return default;
    }
}
