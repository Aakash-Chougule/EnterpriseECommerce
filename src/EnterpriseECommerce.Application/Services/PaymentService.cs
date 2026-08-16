using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Services;

public class PaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    // ============================================================
    // CREATE PAYMENT
    // ============================================================

    public async Task<PaymentDto> CreatePaymentAsync(
        Guid userId,
        CreatePaymentRequest request)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "UserId is required.");
        }

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

        var order = await _orderRepository
            .GetByIdAsync(request.OrderId);

        if (order is null ||
            order.UserId != userId)
        {
            throw new KeyNotFoundException(
                "Order not found.");
        }

        var existingPayment =
            await _paymentRepository
                .GetByOrderIdAsync(order.Id);

        if (existingPayment is not null)
        {
            throw new InvalidOperationException(
                "Payment already exists for this order.");
        }

        var payment = new Payment(
            order.Id,
            order.TotalAmount,
            request.PaymentMethod);

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

        var order = await _orderRepository
            .GetByIdAsync(orderId);

        if (order is null ||
            order.UserId != userId)
        {
            return null;
        }

        var payment = await _paymentRepository
            .GetByOrderIdAsync(orderId);

        return payment is null
            ? null
            : MapToDto(payment);
    }

    // ============================================================
    // PAYMENT SUCCESS
    // ============================================================

    public async Task<PaymentDto> MarkPaymentSuccessfulAsync(
        Guid userId,
        Guid paymentId,
        string transactionId)
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

        if (string.IsNullOrWhiteSpace(
            transactionId))
        {
            throw new ArgumentException(
                "TransactionId is required.");
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var payment = await _paymentRepository
                .GetByIdAsync(paymentId);

            if (payment is null)
            {
                throw new KeyNotFoundException(
                    "Payment not found.");
            }

            var order = await _orderRepository
                .GetByIdAsync(payment.OrderId);

            if (order is null ||
                order.UserId != userId)
            {
                throw new KeyNotFoundException(
                    "Order not found.");
            }

            // --------------------------------------------
            // Update payment
            // --------------------------------------------

            payment.MarkSuccessful(
                transactionId);

            await _paymentRepository
                .UpdateAsync(payment);

            // --------------------------------------------
            // Update order payment status
            // --------------------------------------------

            order.MarkPaymentSuccessful();

            await _orderRepository
                .UpdateAsync(order);

            // --------------------------------------------
            // Commit both changes together
            // --------------------------------------------

            await _unitOfWork
                .CommitTransactionAsync();

            return MapToDto(payment);
        }
        catch
        {
            await _unitOfWork
                .RollbackTransactionAsync();

            throw;
        }
    }

    // ============================================================
    // PAYMENT FAILED
    // ============================================================

    public async Task<PaymentDto> MarkPaymentFailedAsync(
        Guid userId,
        Guid paymentId,
        string? reason)
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

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var payment = await _paymentRepository
                .GetByIdAsync(paymentId);

            if (payment is null)
            {
                throw new KeyNotFoundException(
                    "Payment not found.");
            }

            var order = await _orderRepository
                .GetByIdAsync(payment.OrderId);

            if (order is null ||
                order.UserId != userId)
            {
                throw new KeyNotFoundException(
                    "Order not found.");
            }

            // --------------------------------------------
            // Update payment
            // --------------------------------------------

            payment.MarkFailed(reason);

            await _paymentRepository
                .UpdateAsync(payment);

            // --------------------------------------------
            // Update order payment status
            // --------------------------------------------

            order.MarkPaymentFailed();

            await _orderRepository
                .UpdateAsync(order);

            // --------------------------------------------
            // Commit both changes
            // --------------------------------------------

            await _unitOfWork
                .CommitTransactionAsync();

            return MapToDto(payment);
        }
        catch
        {
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