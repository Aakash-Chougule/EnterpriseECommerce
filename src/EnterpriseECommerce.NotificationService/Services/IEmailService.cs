namespace EnterpriseECommerce.NotificationService.Services;

public interface IEmailService
{
    Task SendOrderConfirmationAsync(
        OrderCreatedEvent orderEvent,
        CancellationToken cancellationToken = default);
}