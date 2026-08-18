using MailKit.Net.Smtp;
using MailKit.Security;

using MimeKit;

namespace EnterpriseECommerce.NotificationService.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    // ============================================================
    // ORDER CONFIRMATION
    // ============================================================

    public async Task SendOrderConfirmationAsync(
        OrderCreatedEvent orderEvent,
        CancellationToken cancellationToken = default)
    {
        var subject =
            $"Order Confirmation - {orderEvent.OrderNumber}";

        var body =
            $"""
            <div style="font-family:Arial,sans-serif;max-width:650px;margin:auto;color:#1e293b;">

                <div style="background:#2563eb;padding:24px;color:#fff;border-radius:8px 8px 0 0;">
                    <h2 style="margin:0;">
                        Order Confirmed
                    </h2>
                </div>

                <div style="border:1px solid #e2e8f0;padding:25px;border-radius:0 0 8px 8px;">

                    <p>
                        Hello <strong>{orderEvent.CustomerName}</strong>,
                    </p>

                    <p>
                        Thank you for your order.
                        Your order has been received successfully.
                    </p>

                    <div style="background:#f8fafc;padding:18px;border-radius:8px;margin:20px 0;">

                        <p>
                            <strong>Order Number:</strong>
                            {orderEvent.OrderNumber}
                        </p>

                        <p>
                            <strong>Total Amount:</strong>
                            ₹{orderEvent.TotalAmount:N2}
                        </p>

                        <p>
                            <strong>Order Date:</strong>
                            {orderEvent.CreatedAt:dd MMM yyyy HH:mm}
                        </p>

                    </div>

                    <p>
                        We will notify you when your order moves to the next stage.
                    </p>

                    <p>
                        Regards,<br />
                        <strong>Enterprise E-Commerce</strong>
                    </p>

                </div>

            </div>
            """;

        await SendEmailAsync(
            orderEvent.CustomerEmail,
            orderEvent.CustomerName,
            subject,
            body,
            cancellationToken);

        _logger.LogInformation(
            "Order confirmation email sent for {OrderNumber}.",
            orderEvent.OrderNumber);
    }

    // ============================================================
    // PAYMENT CONFIRMATION
    // ============================================================

    public async Task SendPaymentConfirmationAsync(
        PaymentSucceededEvent paymentEvent,
        CancellationToken cancellationToken = default)
    {
        var subject =
            $"Payment Successful - {paymentEvent.OrderNumber}";

        var body =
            $"""
            <div style="font-family:Arial,sans-serif;max-width:650px;margin:auto;color:#1e293b;">

                <div style="background:#16a34a;padding:24px;color:#fff;border-radius:8px 8px 0 0;">
                    <h2 style="margin:0;">
                        Payment Successful
                    </h2>
                </div>

                <div style="border:1px solid #e2e8f0;padding:25px;border-radius:0 0 8px 8px;">

                    <p>
                        Hello <strong>{paymentEvent.CustomerName}</strong>,
                    </p>

                    <p>
                        Your payment has been received successfully.
                    </p>

                    <div style="background:#f0fdf4;border:1px solid #bbf7d0;padding:18px;border-radius:8px;margin:20px 0;">

                        <p>
                            <strong>Order Number:</strong>
                            {paymentEvent.OrderNumber}
                        </p>

                        <p>
                            <strong>Amount Paid:</strong>
                            ₹{paymentEvent.Amount:N2}
                        </p>

                        <p>
                            <strong>Payment Method:</strong>
                            {paymentEvent.PaymentMethod}
                        </p>

                        <p>
                            <strong>Transaction ID:</strong>
                            {paymentEvent.TransactionId}
                        </p>

                        <p>
                            <strong>Payment Date:</strong>
                            {paymentEvent.PaidAt:dd MMM yyyy HH:mm}
                        </p>

                    </div>

                    <p>
                        Your order will continue through the fulfillment process.
                    </p>

                    <p>
                        Regards,<br />
                        <strong>Enterprise E-Commerce</strong>
                    </p>

                </div>

            </div>
            """;

        await SendEmailAsync(
            paymentEvent.CustomerEmail,
            paymentEvent.CustomerName,
            subject,
            body,
            cancellationToken);

        _logger.LogInformation(
            "Payment confirmation email sent for order {OrderNumber}.",
            paymentEvent.OrderNumber);
    }

    // ============================================================
    // ORDER STATUS CHANGED
    // ============================================================

    public async Task SendOrderStatusChangedAsync(
        OrderStatusChangedEvent orderEvent,
        CancellationToken cancellationToken = default)
    {
        var title =
            GetStatusTitle(
                orderEvent.NewStatus);

        var message =
            GetStatusMessage(
                orderEvent.NewStatus);

        var headerColor =
            GetStatusColor(
                orderEvent.NewStatus);

        var subject =
            $"{title} - {orderEvent.OrderNumber}";

        var body =
            $"""
            <div style="font-family:Arial,sans-serif;max-width:650px;margin:auto;color:#1e293b;">

                <div style="background:{headerColor};padding:24px;color:#fff;border-radius:8px 8px 0 0;">

                    <h2 style="margin:0;">
                        {title}
                    </h2>

                </div>

                <div style="border:1px solid #e2e8f0;padding:25px;border-radius:0 0 8px 8px;">

                    <p>
                        Hello <strong>{orderEvent.CustomerName}</strong>,
                    </p>

                    <p>
                        {message}
                    </p>

                    <div style="background:#f8fafc;padding:18px;border-radius:8px;margin:20px 0;">

                        <p>
                            <strong>Order Number:</strong>
                            {orderEvent.OrderNumber}
                        </p>

                        <p>
                            <strong>Previous Status:</strong>
                            {orderEvent.PreviousStatus}
                        </p>

                        <p>
                            <strong>Current Status:</strong>
                            {orderEvent.NewStatus}
                        </p>

                        <p>
                            <strong>Total Amount:</strong>
                            ₹{orderEvent.TotalAmount:N2}
                        </p>

                        <p>
                            <strong>Shipping Address:</strong>
                            {orderEvent.ShippingAddress}
                        </p>

                        <p>
                            <strong>Updated:</strong>
                            {orderEvent.ChangedAt:dd MMM yyyy HH:mm}
                        </p>

                    </div>

                    <p>
                        Regards,<br />
                        <strong>Enterprise E-Commerce</strong>
                    </p>

                </div>

            </div>
            """;

        await SendEmailAsync(
            orderEvent.CustomerEmail,
            orderEvent.CustomerName,
            subject,
            body,
            cancellationToken);

        _logger.LogInformation(
            "Order status email sent for order {OrderNumber}. New status: {NewStatus}",
            orderEvent.OrderNumber,
            orderEvent.NewStatus);
    }

    // ============================================================
    // STATUS CONTENT
    // ============================================================

    private static string GetStatusTitle(
        string status)
    {
        return status.ToLowerInvariant() switch
        {
            "confirmed" =>
                "Order Confirmed",

            "processing" =>
                "Order Processing",

            "shipped" =>
                "Your Order Has Been Shipped",

            "delivered" =>
                "Order Delivered",

            "cancelled" =>
                "Order Cancelled",

            _ =>
                "Order Status Updated"
        };
    }

    private static string GetStatusMessage(
        string status)
    {
        return status.ToLowerInvariant() switch
        {
            "confirmed" =>
                "Your order has been confirmed and will soon move into processing.",

            "processing" =>
                "Your order is currently being prepared for shipment.",

            "shipped" =>
                "Your order has been shipped and is now on the way to you.",

            "delivered" =>
                "Your order has been marked as delivered. Thank you for shopping with us.",

            "cancelled" =>
                "Your order has been cancelled. Any applicable payment handling will follow according to the order policy.",

            _ =>
                "The status of your order has been updated."
        };
    }

    private static string GetStatusColor(
        string status)
    {
        return status.ToLowerInvariant() switch
        {
            "confirmed" =>
                "#2563eb",

            "processing" =>
                "#4f46e5",

            "shipped" =>
                "#7c3aed",

            "delivered" =>
                "#16a34a",

            "cancelled" =>
                "#dc2626",

            _ =>
                "#475569"
        };
    }

    // ============================================================
    // COMMON SMTP SENDER
    // ============================================================

    private async Task SendEmailAsync(
        string customerEmail,
        string customerName,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(
            customerEmail))
        {
            throw new InvalidOperationException(
                "Customer email is missing.");
        }

        var host =
            _configuration["Email:SmtpHost"]
            ?? throw new InvalidOperationException(
                "Email SMTP host is not configured.");

        var username =
            _configuration["Email:Username"]
            ?? throw new InvalidOperationException(
                "Email username is not configured.");

        var password =
            _configuration["Email:Password"]
            ?? throw new InvalidOperationException(
                "Email password is not configured.");

        var fromEmail =
            _configuration["Email:FromEmail"]
            ?? username;

        var port =
            _configuration
                .GetValue<int>(
                    "Email:SmtpPort");

        if (port <= 0)
        {
            throw new InvalidOperationException(
                "Email SMTP port is not configured.");
        }

        var email =
            new MimeMessage();

        email.From.Add(
            new MailboxAddress(
                "Enterprise E-Commerce",
                fromEmail));

        email.To.Add(
            new MailboxAddress(
                customerName,
                customerEmail));

        email.Subject =
            subject;

        email.Body =
            new TextPart("html")
            {
                Text =
                    htmlBody
            };

        using var client =
            new SmtpClient();

        await client.ConnectAsync(
            host,
            port,
            SecureSocketOptions.StartTls,
            cancellationToken);

        await client.AuthenticateAsync(
            username,
            password,
            cancellationToken);

        await client.SendAsync(
            email,
            cancellationToken);

        await client.DisconnectAsync(
            true,
            cancellationToken);
    }
}