using EnterpriseECommerce.Domain.Entities;
using EnterpriseECommerce.Domain.Enums;

namespace EnterpriseECommerce.UnitTests;

public class PaymentTests
{
    // ============================================================
    // TEST HELPER
    // ============================================================

    private static Payment CreatePayment(
        decimal amount = 5000m,
        string paymentMethod = "UPI")
    {
        return new Payment(
            Guid.NewGuid(),
            amount,
            paymentMethod);
    }

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    [Fact]
    public void Constructor_WithValidData_CreatesPendingPayment()
    {
        var orderId =
            Guid.NewGuid();

        var payment =
            new Payment(
                orderId,
                5000m,
                "UPI");

        Assert.NotEqual(
            Guid.Empty,
            payment.Id);

        Assert.Equal(
            orderId,
            payment.OrderId);

        Assert.Equal(
            5000m,
            payment.Amount);

        Assert.Equal(
            "UPI",
            payment.PaymentMethod);

        Assert.Equal(
            PaymentStatus.Pending,
            payment.Status);

        Assert.Null(
            payment.TransactionId);

        Assert.Null(
            payment.FailureReason);

        Assert.True(
            payment.CreatedAt <=
            DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithEmptyOrderId_ThrowsException()
    {
        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    new Payment(
                        Guid.Empty,
                        5000m,
                        "UPI"));

        Assert.Equal(
            "OrderId is required.",
            exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_WithInvalidAmount_ThrowsException(
        decimal amount)
    {
        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    new Payment(
                        Guid.NewGuid(),
                        amount,
                        "UPI"));

        Assert.Equal(
            "Payment amount must be greater than zero.",
            exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidPaymentMethod_ThrowsException(
        string paymentMethod)
    {
        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    new Payment(
                        Guid.NewGuid(),
                        5000m,
                        paymentMethod));

        Assert.Equal(
            "Payment method is required.",
            exception.Message);
    }

    [Fact]
    public void Constructor_TrimsPaymentMethod()
    {
        var payment =
            new Payment(
                Guid.NewGuid(),
                5000m,
                "  UPI  ");

        Assert.Equal(
            "UPI",
            payment.PaymentMethod);
    }

    // ============================================================
    // MARK SUCCESSFUL
    // ============================================================

    [Fact]
    public void MarkSuccessful_WithValidTransactionId_UpdatesPayment()
    {
        var payment =
            CreatePayment();

        payment.MarkSuccessful(
            "TXN-12345");

        Assert.Equal(
            PaymentStatus.Success,
            payment.Status);

        Assert.Equal(
            "TXN-12345",
            payment.TransactionId);

        Assert.Null(
            payment.FailureReason);

        Assert.NotNull(
            payment.UpdatedAt);
    }

    [Fact]
    public void MarkSuccessful_TrimsTransactionId()
    {
        var payment =
            CreatePayment();

        payment.MarkSuccessful(
            "  TXN-12345  ");

        Assert.Equal(
            "TXN-12345",
            payment.TransactionId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void MarkSuccessful_WithInvalidTransactionId_ThrowsException(
        string transactionId)
    {
        var payment =
            CreatePayment();

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    payment.MarkSuccessful(
                        transactionId));

        Assert.Equal(
            "TransactionId is required.",
            exception.Message);
    }

    [Fact]
    public void MarkSuccessful_WhenAlreadySuccessful_ThrowsException()
    {
        var payment =
            CreatePayment();

        payment.MarkSuccessful(
            "TXN-001");

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    payment.MarkSuccessful(
                        "TXN-002"));

        Assert.Equal(
            "Payment is already successful.",
            exception.Message);

        Assert.Equal(
            "TXN-001",
            payment.TransactionId);
    }

    // ============================================================
    // MARK FAILED
    // ============================================================

    [Fact]
    public void MarkFailed_WhenPending_ChangesStatusToFailed()
    {
        var payment =
            CreatePayment();

        payment.MarkFailed(
            "Bank declined payment.");

        Assert.Equal(
            PaymentStatus.Failed,
            payment.Status);

        Assert.Equal(
            "Bank declined payment.",
            payment.FailureReason);

        Assert.NotNull(
            payment.UpdatedAt);
    }

    [Fact]
    public void MarkFailed_TrimsFailureReason()
    {
        var payment =
            CreatePayment();

        payment.MarkFailed(
            "  Insufficient funds  ");

        Assert.Equal(
            "Insufficient funds",
            payment.FailureReason);
    }

    [Fact]
    public void MarkFailed_WithNullReason_IsAllowed()
    {
        var payment =
            CreatePayment();

        payment.MarkFailed(
            null);

        Assert.Equal(
            PaymentStatus.Failed,
            payment.Status);

        Assert.Null(
            payment.FailureReason);
    }

    [Fact]
    public void MarkFailed_WhenSuccessful_ThrowsException()
    {
        var payment =
            CreatePayment();

        payment.MarkSuccessful(
            "TXN-001");

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    payment.MarkFailed(
                        "Failure"));

        Assert.Equal(
            "A successful payment cannot be marked as failed.",
            exception.Message);

        Assert.Equal(
            PaymentStatus.Success,
            payment.Status);
    }

    // ============================================================
    // RETRY FAILED PAYMENT
    // ============================================================

    [Fact]
    public void MarkSuccessful_AfterFailedPayment_IsAllowed()
    {
        var payment =
            CreatePayment();

        payment.MarkFailed(
            "Temporary failure");

        payment.MarkSuccessful(
            "TXN-RETRY-001");

        Assert.Equal(
            PaymentStatus.Success,
            payment.Status);

        Assert.Equal(
            "TXN-RETRY-001",
            payment.TransactionId);

        Assert.Null(
            payment.FailureReason);
    }

    // ============================================================
    // REFUND
    // ============================================================

    [Fact]
    public void MarkRefunded_WhenSuccessful_ChangesStatusToRefunded()
    {
        var payment =
            CreatePayment();

        payment.MarkSuccessful(
            "TXN-001");

        payment.MarkRefunded();

        Assert.Equal(
            PaymentStatus.Refunded,
            payment.Status);

        Assert.NotNull(
            payment.UpdatedAt);
    }

    [Fact]
    public void MarkRefunded_WhenPending_ThrowsException()
    {
        var payment =
            CreatePayment();

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    payment.MarkRefunded());

        Assert.Equal(
            "Only successful payments can be refunded.",
            exception.Message);

        Assert.Equal(
            PaymentStatus.Pending,
            payment.Status);
    }

    [Fact]
    public void MarkRefunded_WhenFailed_ThrowsException()
    {
        var payment =
            CreatePayment();

        payment.MarkFailed(
            "Payment failed");

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    payment.MarkRefunded());

        Assert.Equal(
            "Only successful payments can be refunded.",
            exception.Message);

        Assert.Equal(
            PaymentStatus.Failed,
            payment.Status);
    }

    // ============================================================
    // FULL PAYMENT FLOW
    // ============================================================

    [Fact]
    public void PaymentFlow_PendingToSuccessToRefunded_WorksCorrectly()
    {
        var payment =
            CreatePayment(
                7009m,
                "UPI");

        Assert.Equal(
            PaymentStatus.Pending,
            payment.Status);

        payment.MarkSuccessful(
            "TXN-7009");

        Assert.Equal(
            PaymentStatus.Success,
            payment.Status);

        payment.MarkRefunded();

        Assert.Equal(
            PaymentStatus.Refunded,
            payment.Status);
    }
}