using EnterpriseECommerce.Domain.Entities;
using EnterpriseECommerce.Domain.Enums;

namespace EnterpriseECommerce.UnitTests;

public class OrderTests
{
    // ============================================================
    // CREATE ORDER
    // ============================================================

    [Fact]
    public void Constructor_WithValidData_CreatesPendingOrder()
    {
        // Arrange
        var userId =
            Guid.NewGuid();

        var shippingAddress =
            "Airoli Sector 5, Navi Mumbai";

        // Act
        var order =
            new Order(
                userId,
                shippingAddress);

        // Assert
        Assert.NotEqual(
            Guid.Empty,
            order.Id);

        Assert.Equal(
            userId,
            order.UserId);

        Assert.Equal(
            shippingAddress,
            order.ShippingAddress);

        Assert.Equal(
            OrderStatus.Pending,
            order.Status);

        Assert.Equal(
            PaymentStatus.Pending,
            order.PaymentStatus);

        Assert.NotEmpty(
            order.OrderNumber);

        Assert.True(
            order.CreatedAt <=
            DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    new Order(
                        Guid.Empty,
                        "Valid Address"));

        // Assert
        Assert.Equal(
            "User is required.",
            exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidShippingAddress_ThrowsArgumentException(
        string shippingAddress)
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    new Order(
                        Guid.NewGuid(),
                        shippingAddress));

        // Assert
        Assert.Equal(
            "Shipping address is required.",
            exception.Message);
    }

    // ============================================================
    // ORDER STATUS FLOW
    // ============================================================

    [Fact]
    public void Confirm_WhenPending_ChangesStatusToConfirmed()
    {
        var order =
            CreateOrder();

        order.Confirm();

        Assert.Equal(
            OrderStatus.Confirmed,
            order.Status);

        Assert.NotNull(
            order.UpdatedAt);
    }

    [Fact]
    public void StartProcessing_WhenConfirmed_ChangesStatusToProcessing()
    {
        var order =
            CreateOrder();

        order.Confirm();

        order.StartProcessing();

        Assert.Equal(
            OrderStatus.Processing,
            order.Status);
    }

    [Fact]
    public void Ship_WhenProcessing_ChangesStatusToShipped()
    {
        var order =
            CreateOrder();

        order.Confirm();
        order.StartProcessing();

        order.Ship();

        Assert.Equal(
            OrderStatus.Shipped,
            order.Status);
    }

    [Fact]
    public void Deliver_WhenShipped_ChangesStatusToDelivered()
    {
        var order =
            CreateOrder();

        order.Confirm();
        order.StartProcessing();
        order.Ship();

        order.Deliver();

        Assert.Equal(
            OrderStatus.Delivered,
            order.Status);
    }

    // ============================================================
    // INVALID ORDER TRANSITIONS
    // ============================================================

    [Fact]
    public void Confirm_WhenNotPending_ThrowsInvalidOperationException()
    {
        var order =
            CreateOrder();

        order.Confirm();

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    order.Confirm());

        Assert.Equal(
            "Only pending orders can be confirmed.",
            exception.Message);
    }

    [Fact]
    public void StartProcessing_WhenPending_ThrowsInvalidOperationException()
    {
        var order =
            CreateOrder();

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    order.StartProcessing());

        Assert.Equal(
            "Only confirmed orders can be moved to processing.",
            exception.Message);
    }

    [Fact]
    public void Ship_WhenPending_ThrowsInvalidOperationException()
    {
        var order =
            CreateOrder();

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    order.Ship());

        Assert.Equal(
            "Only processing orders can be shipped.",
            exception.Message);
    }

    [Fact]
    public void Deliver_WhenPending_ThrowsInvalidOperationException()
    {
        var order =
            CreateOrder();

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    order.Deliver());

        Assert.Equal(
            "Only shipped orders can be delivered.",
            exception.Message);
    }

    // ============================================================
    // CANCEL ORDER
    // ============================================================

    [Fact]
    public void Cancel_WhenPending_ChangesStatusToCancelled()
    {
        var order =
            CreateOrder();

        order.Cancel();

        Assert.Equal(
            OrderStatus.Cancelled,
            order.Status);
    }

    [Fact]
    public void Cancel_WhenConfirmed_ChangesStatusToCancelled()
    {
        var order =
            CreateOrder();

        order.Confirm();

        order.Cancel();

        Assert.Equal(
            OrderStatus.Cancelled,
            order.Status);
    }

    [Fact]
    public void Cancel_WhenProcessing_ChangesStatusToCancelled()
    {
        var order =
            CreateOrder();

        order.Confirm();
        order.StartProcessing();

        order.Cancel();

        Assert.Equal(
            OrderStatus.Cancelled,
            order.Status);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ThrowsInvalidOperationException()
    {
        var order =
            CreateOrder();

        order.Cancel();

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    order.Cancel());

        Assert.Equal(
            "Order is already cancelled.",
            exception.Message);
    }

    [Fact]
    public void Cancel_WhenShipped_ThrowsInvalidOperationException()
    {
        var order =
            CreateOrder();

        order.Confirm();
        order.StartProcessing();
        order.Ship();

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    order.Cancel());

        Assert.Equal(
            "A shipped or delivered order cannot be cancelled.",
            exception.Message);
    }

    [Fact]
    public void Cancel_WhenDelivered_ThrowsInvalidOperationException()
    {
        var order =
            CreateOrder();

        order.Confirm();
        order.StartProcessing();
        order.Ship();
        order.Deliver();

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    order.Cancel());

        Assert.Equal(
            "A shipped or delivered order cannot be cancelled.",
            exception.Message);
    }

    // ============================================================
    // PAYMENT STATUS
    // ============================================================

    [Fact]
    public void MarkPaymentSuccessful_WhenValid_ChangesPaymentStatus()
    {
        var order =
            CreateOrder();

        order.MarkPaymentSuccessful();

        Assert.Equal(
            PaymentStatus.Success,
            order.PaymentStatus);
    }

    [Fact]
    public void MarkPaymentSuccessful_WhenAlreadySuccessful_ThrowsException()
    {
        var order =
            CreateOrder();

        order.MarkPaymentSuccessful();

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    order.MarkPaymentSuccessful());

        Assert.Equal(
            "Payment is already marked as successful.",
            exception.Message);
    }

    [Fact]
    public void MarkPaymentSuccessful_WhenOrderCancelled_ThrowsException()
    {
        var order =
            CreateOrder();

        order.Cancel();

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    order.MarkPaymentSuccessful());

        Assert.Equal(
            "Payment cannot be completed for a cancelled order.",
            exception.Message);
    }

    [Fact]
    public void MarkPaymentFailed_WhenValid_ChangesStatusToFailed()
    {
        var order =
            CreateOrder();

        order.MarkPaymentFailed();

        Assert.Equal(
            PaymentStatus.Failed,
            order.PaymentStatus);
    }

    [Fact]
    public void MarkPaymentFailed_AfterSuccessfulPayment_ThrowsException()
    {
        var order =
            CreateOrder();

        order.MarkPaymentSuccessful();

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    order.MarkPaymentFailed());

        Assert.Equal(
            "A successful payment cannot be marked as failed.",
            exception.Message);
    }

    // ============================================================
    // ORDER ITEMS / TOTAL
    // ============================================================

    [Fact]
    public void AddItem_AddsItemAndCalculatesTotal()
    {
        var order =
            CreateOrder();

        var productId =
            Guid.NewGuid();

        var item =
            new OrderItem(
                productId,
                "Mechanical Keyboard",
                2,
                2500m);

        order.AddItem(
            item);

        Assert.Single(
            order.OrderItems);

        Assert.Equal(
            5000m,
            order.TotalAmount);
    }

    [Fact]
    public void AddMultipleItems_CalculatesCorrectTotal()
    {
        var order =
            CreateOrder();

        order.AddItem(
            new OrderItem(
                Guid.NewGuid(),
                "Keyboard",
                2,
                2500m));

        order.AddItem(
            new OrderItem(
                Guid.NewGuid(),
                "Mouse",
                1,
                1500m));

        Assert.Equal(
            6500m,
            order.TotalAmount);

        Assert.Equal(
            2,
            order.OrderItems.Count);
    }

    // ============================================================
    // TEST HELPER
    // ============================================================

    private static Order CreateOrder()
    {
        return new Order(
            Guid.NewGuid(),
            "Test Shipping Address");
    }
}