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

    public async Task SendOrderConfirmationAsync(
        OrderCreatedEvent orderEvent,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderEvent.CustomerEmail))
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
            _configuration.GetValue<int>("Email:SmtpPort");

        var message = new MimeMessage();

        message.From.Add(
            new MailboxAddress(
                "Enterprise E-Commerce",
                fromEmail));

        message.To.Add(
            new MailboxAddress(
                orderEvent.CustomerName,
                orderEvent.CustomerEmail));

        message.Subject =
            $"Order Confirmation - {orderEvent.OrderNumber}";

        message.Body = new TextPart("html")
        {
            Text = $"""
                    <h2>Order Confirmed</h2>

                    <p>Hello {orderEvent.CustomerName},</p>

                    <p>Thank you for your order.</p>

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

                    <p>
                        We will notify you when your order
                        moves to the next stage.
                    </p>

                    <p>
                        Enterprise E-Commerce
                    </p>
                    """
        };

        using var client = new SmtpClient();

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
            message,
            cancellationToken);

        await client.DisconnectAsync(
            true,
            cancellationToken);

        _logger.LogInformation(
            "Order confirmation email sent to {Email} for order {OrderNumber}.",
            orderEvent.CustomerEmail,
            orderEvent.OrderNumber);
    }
}