using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Events;
using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;

using Microsoft.Extensions.Configuration;

namespace EnterpriseECommerce.Infrastructure.Payments;

public class RazorpayPaymentService
    : IRazorpayPaymentService
{
    private readonly HttpClient _httpClient;

    private readonly IPaymentRepository
        _paymentRepository;

    private readonly IOrderRepository
        _orderRepository;

    private readonly IUserRepository
        _userRepository;

    private readonly IUnitOfWork
        _unitOfWork;

    private readonly IKafkaProducer
        _kafkaProducer;

    private readonly IConfiguration
        _configuration;

    public RazorpayPaymentService(
        HttpClient httpClient,
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IKafkaProducer kafkaProducer,
        IConfiguration configuration)
    {
        _httpClient =
            httpClient;

        _paymentRepository =
            paymentRepository;

        _orderRepository =
            orderRepository;

        _userRepository =
            userRepository;

        _unitOfWork =
            unitOfWork;

        _kafkaProducer =
            kafkaProducer;

        _configuration =
            configuration;
    }

    // ============================================================
    // CREATE RAZORPAY ORDER
    // ============================================================

    public async Task<RazorpayOrderDto>
        CreateOrderAsync(
            Guid userId,
            Guid paymentId,
            CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "UserId is required.");
        }

        if (paymentId == Guid.Empty)
        {
            throw new ArgumentException(
                "PaymentId is required.");
        }

        var payment =
            await _paymentRepository
                .GetByIdAsync(
                    paymentId);

        if (payment is null)
        {
            throw new KeyNotFoundException(
                "Payment not found.");
        }

        var order =
            await _orderRepository
                .GetByIdAsync(
                    payment.OrderId);

        if (order is null ||
            order.UserId != userId)
        {
            throw new KeyNotFoundException(
                "Order not found.");
        }

        if (payment.PaymentMethod.Equals(
            "COD",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Cash on Delivery does not require Razorpay.");
        }

        var user =
            await _userRepository
                .GetByIdAsync(
                    userId);

        if (user is null)
        {
            throw new KeyNotFoundException(
                "User not found.");
        }

        var keyId =
            GetRequiredSetting(
                "Razorpay:KeyId");

        var keySecret =
            GetRequiredSetting(
                "Razorpay:KeySecret");

        // Razorpay amount is in paise.
        //
        // ₹5000.00 => 500000
        var amountInPaise =
            checked(
                (long)Math.Round(
                    payment.Amount * 100m,
                    MidpointRounding.AwayFromZero));

        // ========================================================
        // REUSE EXISTING GATEWAY ORDER
        // ========================================================

        if (!string.IsNullOrWhiteSpace(
            payment.RazorpayOrderId))
        {
            return new RazorpayOrderDto
            {
                PaymentId =
                    payment.Id,

                KeyId =
                    keyId,

                RazorpayOrderId =
                    payment.RazorpayOrderId,

                Amount =
                    amountInPaise,

                Currency =
                    "INR",

                OrderNumber =
                    order.OrderNumber,

                CustomerName =
                    $"{user.FirstName} {user.LastName}"
                        .Trim(),

                CustomerEmail =
                    user.Email,

                CustomerPhone =
                    user.PhoneNumber
            };
        }

        // ========================================================
        // AUTHORIZATION
        // ========================================================

        var credentials =
            Convert.ToBase64String(
                Encoding.ASCII.GetBytes(
                    $"{keyId}:{keySecret}"));

        using var request =
            new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.razorpay.com/v1/orders");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Basic",
                credentials);

        // ========================================================
        // RAZORPAY REQUEST
        // ========================================================

        var requestBody =
            new
            {
                amount =
                    amountInPaise,

                currency =
                    "INR",

                receipt =
                    order.OrderNumber,

                notes =
                    new
                    {
                        internalOrderId =
                            order.Id.ToString(),

                        paymentId =
                            payment.Id.ToString()
                    }
            };

        request.Content =
            new StringContent(
                JsonSerializer.Serialize(
                    requestBody),
                Encoding.UTF8,
                "application/json");

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        var json =
            await response.Content
                .ReadAsStringAsync(
                    cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Razorpay order creation failed: {json}");
        }

        using var document =
            JsonDocument.Parse(
                json);

        if (!document.RootElement.TryGetProperty(
            "id",
            out var orderIdElement))
        {
            throw new InvalidOperationException(
                "Razorpay did not return an order ID.");
        }

        var razorpayOrderId =
            orderIdElement.GetString();

        if (string.IsNullOrWhiteSpace(
            razorpayOrderId))
        {
            throw new InvalidOperationException(
                "Razorpay returned an invalid order ID.");
        }

        // ========================================================
        // STORE GATEWAY ORDER ID
        // ========================================================

        payment.SetRazorpayOrderId(
            razorpayOrderId);

        await _paymentRepository
            .UpdateAsync(
                payment);

        return new RazorpayOrderDto
        {
            PaymentId =
                payment.Id,

            KeyId =
                keyId,

            RazorpayOrderId =
                razorpayOrderId,

            Amount =
                amountInPaise,

            Currency =
                "INR",

            OrderNumber =
                order.OrderNumber,

            CustomerName =
                $"{user.FirstName} {user.LastName}"
                    .Trim(),

            CustomerEmail =
                user.Email,

            CustomerPhone =
                user.PhoneNumber
        };
    }

    // ============================================================
    // VERIFY RAZORPAY PAYMENT
    // ============================================================

    public async Task<PaymentDto>
        VerifyPaymentAsync(
            Guid userId,
            Guid paymentId,
            VerifyRazorpayPaymentRequest request,
            CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "UserId is required.");
        }

        if (paymentId == Guid.Empty)
        {
            throw new ArgumentException(
                "PaymentId is required.");
        }

        if (request is null)
        {
            throw new ArgumentNullException(
                nameof(request));
        }

        if (string.IsNullOrWhiteSpace(
            request.RazorpayPaymentId))
        {
            throw new ArgumentException(
                "Razorpay Payment ID is required.");
        }

        if (string.IsNullOrWhiteSpace(
            request.RazorpaySignature))
        {
            throw new ArgumentException(
                "Razorpay signature is required.");
        }

        var payment =
            await _paymentRepository
                .GetByIdAsync(
                    paymentId);

        if (payment is null)
        {
            throw new KeyNotFoundException(
                "Payment not found.");
        }

        var order =
            await _orderRepository
                .GetByIdAsync(
                    payment.OrderId);

        if (order is null ||
            order.UserId != userId)
        {
            throw new KeyNotFoundException(
                "Order not found.");
        }

        if (string.IsNullOrWhiteSpace(
            payment.RazorpayOrderId))
        {
            throw new InvalidOperationException(
                "Razorpay Order ID was not created for this payment.");
        }

        // ========================================================
        // IMPORTANT
        // ========================================================
        //
        // Do NOT trust browser RazorpayOrderId for verification.
        //
        // We use payment.RazorpayOrderId from OUR database.
        // ========================================================

        if (!string.Equals(
            request.RazorpayOrderId,
            payment.RazorpayOrderId,
            StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Razorpay Order ID does not match this payment.");
        }

        var secret =
            GetRequiredSetting(
                "Razorpay:KeySecret");

        var verificationText =
            $"{payment.RazorpayOrderId}|" +
            $"{request.RazorpayPaymentId}";

        using var hmac =
            new HMACSHA256(
                Encoding.UTF8.GetBytes(
                    secret));

        var hash =
            hmac.ComputeHash(
                Encoding.UTF8.GetBytes(
                    verificationText));

        var expectedSignature =
            Convert.ToHexString(
                hash)
                .ToLowerInvariant();

        var expectedBytes =
            Encoding.UTF8.GetBytes(
                expectedSignature);

        var receivedBytes =
            Encoding.UTF8.GetBytes(
                request.RazorpaySignature
                    .Trim()
                    .ToLowerInvariant());

        var signatureValid =
            expectedBytes.Length ==
                receivedBytes.Length
            &&
            CryptographicOperations
                .FixedTimeEquals(
                    expectedBytes,
                    receivedBytes);

        if (!signatureValid)
        {
            throw new UnauthorizedAccessException(
                "Razorpay payment signature verification failed.");
        }

        // ========================================================
        // UPDATE PAYMENT + ORDER
        // ========================================================

        await _unitOfWork
            .BeginTransactionAsync();

        try
        {
            payment.MarkRazorpaySuccessful(
                request.RazorpayPaymentId,
                request.RazorpaySignature);

            await _paymentRepository
                .UpdateAsync(
                    payment);

            order.MarkPaymentSuccessful();

            await _orderRepository
                .UpdateAsync(
                    order);

            await _unitOfWork
                .CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork
                .RollbackTransactionAsync();

            throw;
        }

        // ========================================================
        // PAYMENT EVENT
        // ========================================================

        var user =
            await _userRepository
                .GetByIdAsync(
                    userId)
            ?? throw new KeyNotFoundException(
                "User not found.");

        var topic =
            _configuration[
                "Kafka:PaymentEventsTopic"]
            ?? throw new InvalidOperationException(
                "Kafka PaymentEventsTopic is not configured.");

        var paymentSucceededEvent =
            new PaymentSucceededEvent
            {
                PaymentId =
                    payment.Id,

                OrderId =
                    order.Id,

                OrderNumber =
                    order.OrderNumber,

                UserId =
                    order.UserId,

                CustomerEmail =
                    user.Email,

                CustomerName =
                    $"{user.FirstName} {user.LastName}"
                        .Trim(),

                Amount =
                    payment.Amount,

                PaymentMethod =
                    payment.PaymentMethod,

                TransactionId =
                    request.RazorpayPaymentId,

                PaidAt =
                    payment.UpdatedAt
                    ?? DateTime.UtcNow
            };

        await _kafkaProducer
            .PublishAsync(
                topic,
                paymentSucceededEvent,
                cancellationToken);

        return MapToDto(
            payment);
    }

    // ============================================================
    // CONFIG
    // ============================================================

    private string GetRequiredSetting(
        string key)
    {
        return _configuration[key]
               ?? throw new InvalidOperationException(
                   $"{key} is not configured.");
    }

    // ============================================================
    // MAP
    // ============================================================

    private static PaymentDto MapToDto(
        Payment payment)
    {
        return new PaymentDto
        {
            Id =
                payment.Id,

            OrderId =
                payment.OrderId,

            Amount =
                payment.Amount,

            PaymentMethod =
                payment.PaymentMethod,

            TransactionId =
                payment.TransactionId,

            Status =
                payment.Status,

            FailureReason =
                payment.FailureReason,

            CreatedAt =
                payment.CreatedAt,

            UpdatedAt =
                payment.UpdatedAt
        };
    }
}