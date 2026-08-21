using EnterpriseECommerce.Domain.Enums;

namespace EnterpriseECommerce.Domain.Entities;

public class Payment
{
    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public decimal Amount { get; private set; }

    public string PaymentMethod { get; private set; } =
        string.Empty;

    public string? TransactionId { get; private set; }

    public PaymentStatus Status { get; private set; }

    public string? FailureReason { get; private set; }

    // ============================================================
    // RAZORPAY
    // ============================================================

    public string? RazorpayOrderId { get; private set; }

    public string? RazorpayPaymentId { get; private set; }

    public string? RazorpaySignature { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    private Payment()
    {
    }

    public Payment(
        Guid orderId,
        decimal amount,
        string paymentMethod)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException(
                "OrderId is required.");
        }

        if (amount <= 0)
        {
            throw new ArgumentException(
                "Payment amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(
            paymentMethod))
        {
            throw new ArgumentException(
                "Payment method is required.");
        }

        Id =
            Guid.NewGuid();

        OrderId =
            orderId;

        Amount =
            amount;

        PaymentMethod =
            paymentMethod.Trim();

        Status =
            PaymentStatus.Pending;

        CreatedAt =
            DateTime.UtcNow;
    }

    // ============================================================
    // RAZORPAY ORDER
    // ============================================================

    public void SetRazorpayOrderId(
        string razorpayOrderId)
    {
        if (string.IsNullOrWhiteSpace(
            razorpayOrderId))
        {
            throw new ArgumentException(
                "Razorpay Order ID is required.");
        }

        RazorpayOrderId =
            razorpayOrderId.Trim();

        UpdatedAt =
            DateTime.UtcNow;
    }

    // ============================================================
    // PAYMENT SUCCESS
    // ============================================================

    public void MarkSuccessful(
        string transactionId)
    {
        if (string.IsNullOrWhiteSpace(
            transactionId))
        {
            throw new ArgumentException(
                "TransactionId is required.");
        }

        if (Status ==
            PaymentStatus.Success)
        {
            throw new InvalidOperationException(
                "Payment is already successful.");
        }

        TransactionId =
            transactionId.Trim();

        Status =
            PaymentStatus.Success;

        FailureReason =
            null;

        UpdatedAt =
            DateTime.UtcNow;
    }

    // ============================================================
    // RAZORPAY PAYMENT SUCCESS
    // ============================================================

    public void MarkRazorpaySuccessful(
        string razorpayPaymentId,
        string razorpaySignature)
    {
        if (string.IsNullOrWhiteSpace(
            razorpayPaymentId))
        {
            throw new ArgumentException(
                "Razorpay Payment ID is required.");
        }

        if (string.IsNullOrWhiteSpace(
            razorpaySignature))
        {
            throw new ArgumentException(
                "Razorpay signature is required.");
        }

        if (Status ==
            PaymentStatus.Success)
        {
            throw new InvalidOperationException(
                "Payment is already successful.");
        }

        RazorpayPaymentId =
            razorpayPaymentId.Trim();

        RazorpaySignature =
            razorpaySignature.Trim();

        TransactionId =
            razorpayPaymentId.Trim();

        Status =
            PaymentStatus.Success;

        FailureReason =
            null;

        UpdatedAt =
            DateTime.UtcNow;
    }

    // ============================================================
    // PAYMENT FAILED
    // ============================================================

    public void MarkFailed(
        string? reason)
    {
        if (Status ==
            PaymentStatus.Success)
        {
            throw new InvalidOperationException(
                "A successful payment cannot be marked as failed.");
        }

        Status =
            PaymentStatus.Failed;

        FailureReason =
            reason?.Trim();

        UpdatedAt =
            DateTime.UtcNow;
    }

    // ============================================================
    // REFUND
    // ============================================================

    public void MarkRefunded()
    {
        if (Status !=
            PaymentStatus.Success)
        {
            throw new InvalidOperationException(
                "Only successful payments can be refunded.");
        }

        Status =
            PaymentStatus.Refunded;

        UpdatedAt =
            DateTime.UtcNow;
    }
}