namespace EnterpriseECommerce.NotificationService.Services;

public interface IEmailService
{
    Task SendOrderConfirmationAsync(
        OrderCreatedEvent orderEvent,
        CancellationToken cancellationToken = default);

    Task SendPaymentConfirmationAsync(
        PaymentSucceededEvent paymentEvent,
        CancellationToken cancellationToken = default);

    Task SendOrderStatusChangedAsync(
        OrderStatusChangedEvent orderEvent,
        CancellationToken cancellationToken = default);
}