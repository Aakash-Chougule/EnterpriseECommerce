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
        _configuration =
            configuration;

        _logger =
            logger;
    }

    // ============================================================
    // ORDER CONFIRMATION
    // ============================================================

    public async Task SendOrderConfirmationAsync(
        OrderCreatedEvent orderEvent,
        CancellationToken cancellationToken = default)
    {
        var isCod =
            orderEvent.PaymentMethod.Equals(
                "COD",
                StringComparison.OrdinalIgnoreCase);

        var subject =
            $"Order Confirmed - {orderEvent.OrderNumber}";

        // ========================================================
        // PAYMENT INFORMATION BLOCK
        // ========================================================

        string paymentBlock;

        string additionalMessage;

        if (isCod)
        {
            paymentBlock =
                $"""
                <div style="
                    background:#fff7ed;
                    border:1px solid #fed7aa;
                    padding:18px;
                    border-radius:8px;
                    margin:20px 0;
                ">

                    <p style="margin:0 0 10px;">
                        <strong>
                            Payment Method:
                        </strong>

                        Cash on Delivery
                    </p>

                    <p style="margin:0;">
                        Please pay
                        <strong>
                            ₹{orderEvent.TotalAmount:N2}
                        </strong>
                        when your order is delivered.
                    </p>

                </div>
                """;

            additionalMessage =
                """
                <p>
                    Your order is confirmed.
                    No online payment is required.
                    Please keep the payable amount ready
                    when the order is delivered.
                </p>
                """;
        }
        else
        {
            paymentBlock =
                $"""
                <div style="
                    background:#eff6ff;
                    border:1px solid #bfdbfe;
                    padding:18px;
                    border-radius:8px;
                    margin:20px 0;
                ">

                    <p style="margin:0;">
                        <strong>
                            Payment Method:
                        </strong>

                        {orderEvent.PaymentMethod}
                    </p>

                </div>
                """;

            additionalMessage =
                """
                <p>
                    Your order is confirmed.
                    Please complete your payment using
                    the selected payment method.
                </p>
                """;
        }

        // ========================================================
        // EMAIL BODY
        // ========================================================

        var body =
            $"""
            <div style="
                font-family:Arial,sans-serif;
                max-width:650px;
                margin:auto;
                color:#1e293b;
            ">

                <div style="
                    background:#2563eb;
                    padding:24px;
                    color:#ffffff;
                    border-radius:8px 8px 0 0;
                ">

                    <h2 style="margin:0;">
                        Order Confirmed
                    </h2>

                </div>

                <div style="
                    border:1px solid #e2e8f0;
                    padding:25px;
                    border-radius:0 0 8px 8px;
                ">

                    <p>
                        Hello
                        <strong>
                            {orderEvent.CustomerName}
                        </strong>,
                    </p>

                    <p>
                        Thank you for your order.
                        Your order has been confirmed successfully.
                    </p>

                    <div style="
                        background:#f8fafc;
                        padding:18px;
                        border-radius:8px;
                        margin:20px 0;
                    ">

                        <p>
                            <strong>
                                Order Number:
                            </strong>

                            {orderEvent.OrderNumber}
                        </p>

                        <p>
                            <strong>
                                Total Amount:
                            </strong>

                            ₹{orderEvent.TotalAmount:N2}
                        </p>

                        <p>
                            <strong>
                                Order Date:
                            </strong>

                            {orderEvent.CreatedAt:dd MMM yyyy HH:mm}
                        </p>

                    </div>

                    {paymentBlock}

                    {additionalMessage}

                    <p>
                        Regards,
                        <br />

                        <strong>
                            Enterprise E-Commerce
                        </strong>
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
        // ========================================================
        // COD DOES NOT REQUIRE PAYMENT SUCCESS EMAIL
        // ========================================================

        if (paymentEvent.PaymentMethod.Equals(
            "COD",
            StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation(
                "Payment success email skipped for COD order {OrderNumber}.",
                paymentEvent.OrderNumber);

            return;
        }

        var subject =
            $"Payment Successful - {paymentEvent.OrderNumber}";

        var body =
            $"""
            <div style="
                font-family:Arial,sans-serif;
                max-width:650px;
                margin:auto;
                color:#1e293b;
            ">

                <div style="
                    background:#16a34a;
                    padding:24px;
                    color:#ffffff;
                    border-radius:8px 8px 0 0;
                ">

                    <h2 style="margin:0;">
                        Payment Successful
                    </h2>

                </div>

                <div style="
                    border:1px solid #e2e8f0;
                    padding:25px;
                    border-radius:0 0 8px 8px;
                ">

                    <p>
                        Hello
                        <strong>
                            {paymentEvent.CustomerName}
                        </strong>,
                    </p>

                    <p>
                        Your payment has been received successfully.
                    </p>

                    <div style="
                        background:#f0fdf4;
                        border:1px solid #bbf7d0;
                        padding:18px;
                        border-radius:8px;
                        margin:20px 0;
                    ">

                        <p>
                            <strong>
                                Order Number:
                            </strong>

                            {paymentEvent.OrderNumber}
                        </p>

                        <p>
                            <strong>
                                Amount Paid:
                            </strong>

                            ₹{paymentEvent.Amount:N2}
                        </p>

                        <p>
                            <strong>
                                Payment Method:
                            </strong>

                            {paymentEvent.PaymentMethod}
                        </p>

                        <p>
                            <strong>
                                Transaction ID:
                            </strong>

                            {paymentEvent.TransactionId}
                        </p>

                        <p>
                            <strong>
                                Payment Date:
                            </strong>

                            {paymentEvent.PaidAt:dd MMM yyyy HH:mm}
                        </p>

                    </div>

                    <p>
                        Your payment is complete and
                        your order remains confirmed.
                    </p>

                    <p>
                        Regards,
                        <br />

                        <strong>
                            Enterprise E-Commerce
                        </strong>
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
    // ORDER STATUS EMAIL
    // ============================================================

    public async Task SendOrderStatusChangedAsync(
        OrderStatusChangedEvent orderEvent,
        CancellationToken cancellationToken = default)
    {
        var status =
            orderEvent.NewStatus
                .Trim()
                .ToLowerInvariant();

        // ========================================================
        // ONLY TWO STATUS EMAILS
        // ========================================================

        if (status != "delivered" &&
            status != "cancelled")
        {
            _logger.LogInformation(
                "Order status email skipped for status {Status}.",
                orderEvent.NewStatus);

            return;
        }

        string title;
        string message;
        string headerColor;

        if (status == "delivered")
        {
            title =
                "Order Delivered";

            message =
                "Your order has been delivered successfully. " +
                "Thank you for shopping with us.";

            headerColor =
                "#16a34a";
        }
        else
        {
            title =
                "Order Cancelled";

            message =
                "Your order has been cancelled.";

            headerColor =
                "#dc2626";
        }

        var subject =
            $"{title} - {orderEvent.OrderNumber}";

        var body =
            $"""
            <div style="
                font-family:Arial,sans-serif;
                max-width:650px;
                margin:auto;
                color:#1e293b;
            ">

                <div style="
                    background:{headerColor};
                    padding:24px;
                    color:#ffffff;
                    border-radius:8px 8px 0 0;
                ">

                    <h2 style="margin:0;">
                        {title}
                    </h2>

                </div>

                <div style="
                    border:1px solid #e2e8f0;
                    padding:25px;
                    border-radius:0 0 8px 8px;
                ">

                    <p>
                        Hello
                        <strong>
                            {orderEvent.CustomerName}
                        </strong>,
                    </p>

                    <p>
                        {message}
                    </p>

                    <div style="
                        background:#f8fafc;
                        padding:18px;
                        border-radius:8px;
                        margin:20px 0;
                    ">

                        <p>
                            <strong>
                                Order Number:
                            </strong>

                            {orderEvent.OrderNumber}
                        </p>

                        <p>
                            <strong>
                                Status:
                            </strong>

                            {orderEvent.NewStatus}
                        </p>

                        <p>
                            <strong>
                                Total Amount:
                            </strong>

                            ₹{orderEvent.TotalAmount:N2}
                        </p>

                        <p>
                            <strong>
                                Shipping Address:
                            </strong>

                            {orderEvent.ShippingAddress}
                        </p>

                        <p>
                            <strong>
                                Updated:
                            </strong>

                            {orderEvent.ChangedAt:dd MMM yyyy HH:mm}
                        </p>

                    </div>

                    <p>
                        Regards,
                        <br />

                        <strong>
                            Enterprise E-Commerce
                        </strong>
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
            "Order {Status} email sent for order {OrderNumber}.",
            orderEvent.NewStatus,
            orderEvent.OrderNumber);
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
            _configuration[
                "Email:SmtpHost"]
            ?? throw new InvalidOperationException(
                "Email SMTP host is not configured.");

        var username =
            _configuration[
                "Email:Username"]
            ?? throw new InvalidOperationException(
                "Email username is not configured.");

        var password =
            _configuration[
                "Email:Password"]
            ?? throw new InvalidOperationException(
                "Email password is not configured.");

        var fromEmail =
            _configuration[
                "Email:FromEmail"]
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