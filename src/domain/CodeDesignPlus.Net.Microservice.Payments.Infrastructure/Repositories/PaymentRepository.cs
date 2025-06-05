namespace CodeDesignPlus.Net.Microservice.Payments.Infrastructure.Repositories;

public class PaymentRepository(IServiceProvider serviceProvider, IOptions<MongoOptions> mongoOptions, ILogger<PaymentRepository> logger) 
    : RepositoryBase(serviceProvider, mongoOptions, logger), IPaymentRepository
{
   
}