using CodeDesignPlus.Net.gRpc.Clients.Abstractions;
using CodeDesignPlus.Net.gRpc.Clients.Services.Notification;
using CodeDesignPlus.Net.Microservice.Payments.AsyncWorker.Consumers;
using CodeDesignPlus.Net.Microservice.Payments.AsyncWorker.Notifications;
using CodeDesignPlus.Net.Microservice.Payments.Domain.DomainEvents;
using Microsoft.Extensions.Options;

namespace CodeDesignPlus.Net.Microservice.Payments.AsyncWorker.Test.Consumers;

/// <summary>
/// An invalidated collection account reaches the inbox of the configured roles of its tenant, once per event.
/// </summary>
public class MerchantAccountInvalidatedHandlerTest
{
    private static readonly Guid Tenant = Guid.Parse("bb000000-0000-4000-8000-000000000002");
    private static readonly Guid AdministratorRole = Guid.Parse("20000000-0000-0000-0000-000000000001");

    [Fact]
    public async Task HandleAsync_AccountInvalidated_NotifiesTheConfiguredRolesOfTheTenant()
    {
        // Arrange
        var inbox = new Mock<IInboxGrpc>();
        NotificationRequest? sent = null;
        inbox.Setup(x => x.NotifyAsync(It.IsAny<NotificationRequest>(), It.IsAny<CancellationToken>())).Callback<NotificationRequest, CancellationToken>((request, _) => sent = request).Returns(Task.CompletedTask);

        var options = Options.Create(new MerchantAccountNoticesOptions { Roles = [AdministratorRole], Kind = "payments.merchant-account-invalid", Module = "payments", SentBy = Guid.NewGuid() });
        var @event = MerchantAccountInvalidatedDomainEvent.Create(Guid.NewGuid(), Tenant, "Wompi", "Wompi does not accept the private key.");

        // Act
        await new MerchantAccountInvalidatedHandler(inbox.Object, options).HandleAsync(@event, CancellationToken.None);

        // Assert
        Assert.Equal((Tenant.ToString(), "payments.merchant-account-invalid", AdministratorRole.ToString(), NotificationIds.For(@event.EventId, "payments.merchant-account-invalid").ToString()), (sent!.Tenant, sent.Kind, sent.Audience.Values.Single(), sent.Id));
    }
}
