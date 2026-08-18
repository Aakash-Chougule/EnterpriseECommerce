using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Events;
using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;

using Microsoft.Extensions.Configuration;

namespace EnterpriseECommerce.Application.Services;

public class PaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly IConfiguration _configuration;

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public PaymentService(
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IKafkaProducer kafkaProducer,
        IConfiguration configuration)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _kafkaProducer = kafkaProducer;
        _configuration = configuration;
    }

    // ============================================================
    // CREATE PAYMENT
    // ============================================================

    public async Task<PaymentDto> CreatePaymentAsync(
        Guid userId,
        CreatePaymentRequest request)
    {
        // --------------------------------------------------------
        // Validate user
        // --------------------------------------------------------

        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "UserId is required.");
        }

        // --------------------------------------------------------
        // Validate request
        // --------------------------------------------------------

        if (request is null)
        {
            throw new ArgumentNullException(
                nameof(request));
        }

        if (request.OrderId == Guid.Empty)
        {
            throw new ArgumentException(
                "OrderId is required.");
        }

        if (string.IsNullOrWhiteSpace(
            request.PaymentMethod))
        {
            throw new ArgumentException(
                "Payment method is required.");
        }

        // --------------------------------------------------------
        // Load order
        // --------------------------------------------------------

        var order =
            await _orderRepository
                .GetByIdAsync(
                    request.OrderId);

        if (order is null ||
            order.UserId != userId)
        {
            throw new KeyNotFoundException(
                "Order not found.");
        }

        // --------------------------------------------------------
        // Prevent duplicate payments
        // --------------------------------------------------------

        var existingPayment =
            await _paymentRepository
                .GetByOrderIdAsync(
                    order.Id);

        if (existingPayment is not null)
        {
            throw new InvalidOperationException(
                "Payment already exists for this order.");
        }

        // --------------------------------------------------------
        // Create payment
        // --------------------------------------------------------

        var payment =
            new Payment(
                order.Id,
                order.TotalAmount,
                request.PaymentMethod.Trim());

        await _paymentRepository
            .AddAsync(payment);

        return MapToDto(payment);
    }

    // ============================================================
    // GET PAYMENT BY ORDER
    // ============================================================

    public async Task<PaymentDto?>
        GetPaymentByOrderIdAsync(
            Guid userId,
            Guid orderId)
    {
        // --------------------------------------------------------
        // Validation
        // --------------------------------------------------------

        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "UserId is required.");
        }

        if (orderId == Guid.Empty)
        {
            throw new ArgumentException(
                "OrderId is required.");
        }

        // --------------------------------------------------------
        // Verify order ownership
        // --------------------------------------------------------

        var order =
            await _orderRepository
                .GetByIdAsync(orderId);

        if (order is null ||
            order.UserId != userId)
        {
            return null;
        }

        // --------------------------------------------------------
        // Load payment
        // --------------------------------------------------------

        var payment =
            await _paymentRepository
                .GetByOrderIdAsync(
                    orderId);

        return payment is null
            ? null
            : MapToDto(payment);
    }

    // ============================================================
    // PAYMENT SUCCESS
    // ============================================================

    public async Task<PaymentDto>
        MarkPaymentSuccessfulAsync(
            Guid userId,
            Guid paymentId,
            string transactionId)
    {
        // --------------------------------------------------------
        // Validation
        // --------------------------------------------------------

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

        if (string.IsNullOrWhiteSpace(
            transactionId))
        {
            throw new ArgumentException(
                "TransactionId is required.");
        }

        // --------------------------------------------------------
        // Kafka topic
        // --------------------------------------------------------

        var topic =
            _configuration[
                "Kafka:PaymentEventsTopic"]
            ?? throw new InvalidOperationException(
                "Kafka PaymentEventsTopic is not configured.");

        // --------------------------------------------------------
        // Variables required after DB commit
        // --------------------------------------------------------

        Payment? payment = null;

        Order? order = null;

        User? user = null;

        // --------------------------------------------------------
        // Begin transaction
        // --------------------------------------------------------

        await _unitOfWork
            .BeginTransactionAsync();

        try
        {
            // ====================================================
            // LOAD PAYMENT
            // ====================================================

            payment =
                await _paymentRepository
                    .GetByIdAsync(
                        paymentId);

            if (payment is null)
            {
                throw new KeyNotFoundException(
                    "Payment not found.");
            }

            // ====================================================
            // LOAD ORDER
            // ====================================================

            order =
                await _orderRepository
                    .GetByIdAsync(
                        payment.OrderId);

            if (order is null ||
                order.UserId != userId)
            {
                throw new KeyNotFoundException(
                    "Order not found.");
            }

            // ====================================================
            // LOAD CUSTOMER
            // ====================================================

            user =
                await _userRepository
                    .GetByIdAsync(
                        userId);

            if (user is null)
            {
                throw new KeyNotFoundException(
                    "User not found.");
            }

            if (!user.IsActive)
            {
                throw new InvalidOperationException(
                    "Inactive users cannot complete payments.");
            }

            // ====================================================
            // UPDATE PAYMENT
            // ====================================================

            payment.MarkSuccessful(
                transactionId.Trim());

            await _paymentRepository
                .UpdateAsync(payment);

            // ====================================================
            // UPDATE ORDER PAYMENT STATUS
            // ====================================================

            order.MarkPaymentSuccessful();

            await _orderRepository
                .UpdateAsync(order);

            // ====================================================
            // COMMIT DATABASE TRANSACTION
            // ====================================================

            await _unitOfWork
                .CommitTransactionAsync();
        }
        catch
        {
            // ====================================================
            // ROLLBACK
            // ====================================================

            await _unitOfWork
                .RollbackTransactionAsync();

            throw;
        }

        // --------------------------------------------------------
        // Ensure required objects exist after successful commit
        // --------------------------------------------------------

        if (payment is null ||
            order is null ||
            user is null)
        {
            throw new InvalidOperationException(
                "Payment processing failed.");
        }

        // ============================================================
        // CREATE PAYMENT SUCCEEDED EVENT
        // ============================================================

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
                    payment.TransactionId
                    ?? transactionId.Trim(),

                PaidAt =
                    payment.UpdatedAt
                    ?? DateTime.UtcNow
            };

        // ============================================================
        // PUBLISH EVENT AFTER DATABASE COMMIT
        // ============================================================

        await _kafkaProducer
            .PublishAsync(
                topic,
                paymentSucceededEvent);

        // ============================================================
        // RETURN PAYMENT
        // ============================================================

        return MapToDto(payment);
    }

    // ============================================================
    // PAYMENT FAILED
    // ============================================================

    public async Task<PaymentDto>
        MarkPaymentFailedAsync(
            Guid userId,
            Guid paymentId,
            string? reason)
    {
        // --------------------------------------------------------
        // Validation
        // --------------------------------------------------------

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

        // --------------------------------------------------------
        // Begin transaction
        // --------------------------------------------------------

        await _unitOfWork
            .BeginTransactionAsync();

        try
        {
            // ====================================================
            // LOAD PAYMENT
            // ====================================================

            var payment =
                await _paymentRepository
                    .GetByIdAsync(
                        paymentId);

            if (payment is null)
            {
                throw new KeyNotFoundException(
                    "Payment not found.");
            }

            // ====================================================
            // LOAD ORDER
            // ====================================================

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

            // ====================================================
            // UPDATE PAYMENT
            // ====================================================

            payment.MarkFailed(
                reason?.Trim());

            await _paymentRepository
                .UpdateAsync(payment);

            // ====================================================
            // UPDATE ORDER PAYMENT STATUS
            // ====================================================

            order.MarkPaymentFailed();

            await _orderRepository
                .UpdateAsync(order);

            // ====================================================
            // COMMIT
            // ====================================================

            await _unitOfWork
                .CommitTransactionAsync();

            return MapToDto(payment);
        }
        catch
        {
            // ====================================================
            // ROLLBACK
            // ====================================================

            await _unitOfWork
                .RollbackTransactionAsync();

            throw;
        }
    }

    // ============================================================
    // MAPPING
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