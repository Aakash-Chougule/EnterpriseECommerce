using System.Globalization;

using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Events;
using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;

using Microsoft.Extensions.Configuration;

namespace EnterpriseECommerce.Application.Services;

/// <summary>
/// Handles order and checkout-related business logic.
///
/// Responsibilities:
/// - Create orders from the customer's cart
/// - Validate stock
/// - Snapshot product pricing and GST
/// - Calculate shipping
/// - Calculate GST breakdown
/// - Reduce stock
/// - Clear cart
/// - Retrieve customer/admin orders
/// - Manage order status
/// - Restore stock on cancellation
/// - Publish Kafka order events
/// </summary>
public class OrderService
{
    private readonly IOrderRepository
        _orderRepository;

    private readonly ICartRepository
        _cartRepository;

    private readonly IProductRepository
        _productRepository;

    private readonly IUserRepository
        _userRepository;

    private readonly IUnitOfWork
        _unitOfWork;

    private readonly IKafkaProducer
        _kafkaProducer;

    private readonly IConfiguration
        _configuration;

    public OrderService(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IKafkaProducer kafkaProducer,
        IConfiguration configuration)
    {
        _orderRepository =
            orderRepository;

        _cartRepository =
            cartRepository;

        _productRepository =
            productRepository;

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
    // CREATE ORDER / CHECKOUT
    // ============================================================

    public async Task<OrderDto>
        CreateOrderAsync(
            Guid userId,
            CreateOrderRequest request)
    {
        // ========================================================
        // BASIC VALIDATION
        // ========================================================

        if (userId ==
            Guid.Empty)
        {
            throw new ArgumentException(
                "UserId is required.");
        }

        ArgumentNullException.ThrowIfNull(
            request);

        if (string.IsNullOrWhiteSpace(
            request.ShippingAddress))
        {
            throw new ArgumentException(
                "Shipping address is required.");
        }

        if (string.IsNullOrWhiteSpace(
            request.ShippingState))
        {
            throw new ArgumentException(
                "Shipping state is required.");
        }

        if (string.IsNullOrWhiteSpace(
            request.ShippingStateCode))
        {
            throw new ArgumentException(
                "Shipping state code is required.");
        }

        if (string.IsNullOrWhiteSpace(
            request.PostalCode))
        {
            throw new ArgumentException(
                "PIN code is required.");
        }

        if (string.IsNullOrWhiteSpace(
            request.PaymentMethod))
        {
            throw new ArgumentException(
                "Payment method is required.");
        }

        // ========================================================
        // PIN VALIDATION
        // ========================================================

        var postalCode =
            request.PostalCode.Trim();

        if (postalCode.Length != 6 ||
            !postalCode.All(
                char.IsDigit))
        {
            throw new ArgumentException(
                "A valid 6-digit PIN code is required.");
        }

        // ========================================================
        // PAYMENT METHOD
        // ========================================================

        var supportedPaymentMethods =
            new[]
            {
                "UPI",
                "Card",
                "NetBanking",
                "COD"
            };

        var paymentMethod =
            request.PaymentMethod
                .Trim();

        if (!supportedPaymentMethods.Contains(
            paymentMethod,
            StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Invalid payment method.");
        }

        // ========================================================
        // CONFIGURATION
        // ========================================================

        var orderEventsTopic =
            _configuration[
                "Kafka:OrderEventsTopic"]
            ?? throw new InvalidOperationException(
                "Kafka OrderEventsTopic is not configured.");

        var sellerStateCode =
            _configuration[
                "Commerce:SellerStateCode"];

        if (string.IsNullOrWhiteSpace(
            sellerStateCode))
        {
            throw new InvalidOperationException(
                "Commerce SellerStateCode is not configured.");
        }

        var defaultShippingCharge =
            GetDecimalSetting(
                "Commerce:DefaultShippingCharge",
                40m);

        var freeShippingThreshold =
            GetDecimalSetting(
                "Commerce:FreeShippingThreshold",
                500m);

        if (defaultShippingCharge < 0)
        {
            throw new InvalidOperationException(
                "Default shipping charge cannot be negative.");
        }

        if (freeShippingThreshold < 0)
        {
            throw new InvalidOperationException(
                "Free shipping threshold cannot be negative.");
        }

        // ========================================================
        // CUSTOMER
        // ========================================================

        var user =
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
                "Inactive users cannot create orders.");
        }

        Order? order =
            null;

        // ========================================================
        // TRANSACTION
        // ========================================================

        await _unitOfWork
            .BeginTransactionAsync();

        try
        {
            // ====================================================
            // LOAD CART
            // ====================================================

            var cart =
                await _cartRepository
                    .GetByUserIdAsync(
                        userId);

            if (cart is null ||
                cart.Items.Count == 0)
            {
                throw new InvalidOperationException(
                    "Cart is empty.");
            }

            // ====================================================
            // CREATE ORDER
            // ====================================================

            order =
                new Order(
                    userId:
                        userId,

                    shippingAddress:
                        request
                            .ShippingAddress
                            .Trim(),

                    shippingState:
                        request
                            .ShippingState
                            .Trim(),

                    shippingStateCode:
                        request
                            .ShippingStateCode
                            .Trim(),

                    postalCode:
                        postalCode,

                    sellerStateCode:
                        sellerStateCode.Trim());

            // ====================================================
            // CART ITEMS -> ORDER ITEMS
            // ====================================================

            foreach (
                var cartItem in
                cart.Items)
            {
                var product =
                    await _productRepository
                        .GetByIdAsync(
                            cartItem.ProductId);

                // ================================================
                // PRODUCT AVAILABILITY
                // ================================================

                if (product is null ||
                    !product.IsActive)
                {
                    throw new InvalidOperationException(
                        $"Product '{cartItem.ProductId}' " +
                        "is no longer available.");
                }

                // ================================================
                // STOCK
                // ================================================

                if (cartItem.Quantity >
                    product.StockQuantity)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for product " +
                        $"'{product.Name}'. " +
                        $"Available stock: " +
                        $"{product.StockQuantity}. " +
                        $"Requested quantity: " +
                        $"{cartItem.Quantity}.");
                }

                // ================================================
                // ORDER ITEM SNAPSHOT
                // ================================================
                //
                // We permanently copy:
                //
                // - Product name
                // - SKU
                // - HSN
                // - Quantity
                // - Price
                // - GST rate
                //
                // Old invoices therefore remain correct even if
                // product data changes in the future.
                // ================================================

                var orderItem =
                    new OrderItem(
                        productId:
                            product.Id,

                        productName:
                            product.Name,

                        sku:
                            product.SKU,

                        hsnCode:
                            product.HsnCode,

                        quantity:
                            cartItem.Quantity,

                        unitPrice:
                            product.Price,

                        gstRate:
                            product.GstRate,

                        isInterState:
                            order.IsInterState);

                order.AddItem(
                    orderItem);

                // ================================================
                // REDUCE INVENTORY
                // ================================================

                product.ReduceStock(
                    cartItem.Quantity);

                await _productRepository
                    .UpdateAsync(
                        product);
            }

            // ====================================================
            // SHIPPING
            // ====================================================
            //
            // Current policy:
            //
            // Subtotal >= configured threshold
            //      -> FREE SHIPPING
            //
            // Otherwise
            //      -> configured shipping charge
            //
            // ====================================================

            var shippingCharge =
                order.Subtotal >=
                freeShippingThreshold
                    ? 0m
                    : defaultShippingCharge;

            order.SetShippingCharge(
                shippingCharge);

            // ====================================================
            // DISCOUNT
            // ====================================================
            //
            // Coupon system will be added later.
            // For now no discount is applied.
            // ====================================================

            order.SetDiscount(
                0m);

            // ====================================================
            // AUTO CONFIRM
            // ====================================================

            order.Confirm();

            // ====================================================
            // SAVE ORDER
            // ====================================================

            await _orderRepository
                .AddAsync(
                    order);

            // ====================================================
            // CLEAR CART
            // ====================================================

            cart.Clear();

            await _cartRepository
                .UpdateAsync(
                    cart);

            // ====================================================
            // COMMIT
            // ====================================================

            await _unitOfWork
                .CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork
                .RollbackTransactionAsync();

            throw;
        }

        if (order is null)
        {
            throw new InvalidOperationException(
                "Order creation failed.");
        }

        // ========================================================
        // ORDER CREATED EVENT
        // ========================================================

        var orderCreatedEvent =
            new OrderCreatedEvent
            {
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

                TotalAmount =
                    order.TotalAmount,

                PaymentMethod =
                    paymentMethod,

                CreatedAt =
                    order.CreatedAt
            };

        // Kafka is published only after successful DB commit.

        await _kafkaProducer
            .PublishAsync(
                orderEventsTopic,
                orderCreatedEvent);

        return MapToDto(
            order);
    }

    // ============================================================
    // GET USER ORDERS
    // ============================================================

    public async Task<IReadOnlyList<OrderDto>>
        GetUserOrdersAsync(
            Guid userId)
    {
        if (userId ==
            Guid.Empty)
        {
            throw new ArgumentException(
                "UserId is required.");
        }

        var orders =
            await _orderRepository
                .GetByUserIdAsync(
                    userId);

        return orders
            .Select(
                MapToDto)
            .ToList();
    }

    // ============================================================
    // GET ORDER BY ID
    // ============================================================

    public async Task<OrderDto?>
        GetOrderByIdAsync(
            Guid userId,
            Guid orderId)
    {
        if (userId ==
            Guid.Empty)
        {
            throw new ArgumentException(
                "UserId is required.");
        }

        if (orderId ==
            Guid.Empty)
        {
            throw new ArgumentException(
                "OrderId is required.");
        }

        var order =
            await _orderRepository
                .GetByIdAsync(
                    orderId);

        if (order is null ||
            order.UserId !=
            userId)
        {
            return null;
        }

        return MapToDto(
            order);
    }

    // ============================================================
    // ADMIN - GET ALL ORDERS
    // ============================================================

    public async Task<IReadOnlyList<OrderDto>>
        GetAllOrdersAsync()
    {
        var orders =
            await _orderRepository
                .GetAllAsync();

        var result =
            new List<OrderDto>();

        foreach (
            var order in
            orders)
        {
            var user =
                await _userRepository
                    .GetByIdAsync(
                        order.UserId);

            var dto =
                MapToDto(
                    order);

            if (user is not null)
            {
                dto.CustomerName =
                    $"{user.FirstName} {user.LastName}"
                        .Trim();

                dto.CustomerEmail =
                    user.Email;

                dto.CustomerPhoneNumber =
                    user.PhoneNumber;
            }
            else
            {
                dto.CustomerName =
                    "Unknown Customer";

                dto.CustomerEmail =
                    string.Empty;

                dto.CustomerPhoneNumber =
                    null;
            }

            result.Add(
                dto);
        }

        return result;
    }

    // ============================================================
    // ADMIN - CONFIRM ORDER
    // ============================================================

    public async Task<OrderDto>
        ConfirmOrderAsync(
            Guid orderId)
    {
        var order =
            await GetRequiredOrderAsync(
                orderId);

        var previousStatus =
            order.Status
                .ToString();

        order.Confirm();

        await _orderRepository
            .UpdateAsync(
                order);

        await PublishOrderStatusChangedEventAsync(
            order,
            previousStatus);

        return MapToDto(
            order);
    }

    // ============================================================
    // ADMIN - START PROCESSING
    // ============================================================

    public async Task<OrderDto>
        StartProcessingAsync(
            Guid orderId)
    {
        var order =
            await GetRequiredOrderAsync(
                orderId);

        var previousStatus =
            order.Status
                .ToString();

        order.StartProcessing();

        await _orderRepository
            .UpdateAsync(
                order);

        await PublishOrderStatusChangedEventAsync(
            order,
            previousStatus);

        return MapToDto(
            order);
    }

    // ============================================================
    // ADMIN - SHIP
    // ============================================================

    public async Task<OrderDto>
        ShipOrderAsync(
            Guid orderId)
    {
        var order =
            await GetRequiredOrderAsync(
                orderId);

        var previousStatus =
            order.Status
                .ToString();

        order.Ship();

        await _orderRepository
            .UpdateAsync(
                order);

        await PublishOrderStatusChangedEventAsync(
            order,
            previousStatus);

        return MapToDto(
            order);
    }

    // ============================================================
    // ADMIN - DELIVER
    // ============================================================

    public async Task<OrderDto>
        DeliverOrderAsync(
            Guid orderId)
    {
        var order =
            await GetRequiredOrderAsync(
                orderId);

        var previousStatus =
            order.Status
                .ToString();

        order.Deliver();

        await _orderRepository
            .UpdateAsync(
                order);

        await PublishOrderStatusChangedEventAsync(
            order,
            previousStatus);

        return MapToDto(
            order);
    }

    // ============================================================
    // ADMIN - CANCEL ORDER
    // ============================================================

    public async Task<OrderDto>
        CancelOrderAsync(
            Guid orderId)
    {
        if (orderId ==
            Guid.Empty)
        {
            throw new ArgumentException(
                "OrderId is required.");
        }

        Order? order =
            null;

        string? previousStatus =
            null;

        await _unitOfWork
            .BeginTransactionAsync();

        try
        {
            order =
                await _orderRepository
                    .GetByIdAsync(
                        orderId);

            if (order is null)
            {
                throw new KeyNotFoundException(
                    "Order not found.");
            }

            previousStatus =
                order.Status
                    .ToString();

            // ====================================================
            // CANCEL
            // ====================================================

            order.Cancel();

            // ====================================================
            // RESTORE STOCK
            // ====================================================

            foreach (
                var orderItem in
                order.OrderItems)
            {
                var product =
                    await _productRepository
                        .GetByIdAsync(
                            orderItem.ProductId);

                if (product is null)
                {
                    throw new InvalidOperationException(
                        $"Product " +
                        $"'{orderItem.ProductId}' " +
                        "was not found.");
                }

                product.IncreaseStock(
                    orderItem.Quantity);

                await _productRepository
                    .UpdateAsync(
                        product);
            }

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

        if (order is null ||
            string.IsNullOrWhiteSpace(
                previousStatus))
        {
            throw new InvalidOperationException(
                "Order cancellation failed.");
        }

        await PublishOrderStatusChangedEventAsync(
            order,
            previousStatus);

        return MapToDto(
            order);
    }

    // ============================================================
    // PUBLISH STATUS EVENT
    // ============================================================

    private async Task
        PublishOrderStatusChangedEventAsync(
            Order order,
            string previousStatus)
    {
        var topic =
            _configuration[
                "Kafka:OrderStatusEventsTopic"]
            ?? throw new InvalidOperationException(
                "Kafka OrderStatusEventsTopic is not configured.");

        var user =
            await _userRepository
                .GetByIdAsync(
                    order.UserId);

        if (user is null)
        {
            throw new KeyNotFoundException(
                "Customer was not found for the order.");
        }

        if (string.IsNullOrWhiteSpace(
            user.Email))
        {
            throw new InvalidOperationException(
                "Customer email is missing.");
        }

        var orderStatusChangedEvent =
            new OrderStatusChangedEvent
            {
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

                PreviousStatus =
                    previousStatus,

                NewStatus =
                    order.Status
                        .ToString(),

                TotalAmount =
                    order.TotalAmount,

                ShippingAddress =
                    order.ShippingAddress,

                ChangedAt =
                    order.UpdatedAt
                    ?? DateTime.UtcNow
            };

        await _kafkaProducer
            .PublishAsync(
                topic,
                orderStatusChangedEvent);
    }

    // ============================================================
    // REQUIRED ORDER
    // ============================================================

    private async Task<Order>
        GetRequiredOrderAsync(
            Guid orderId)
    {
        if (orderId ==
            Guid.Empty)
        {
            throw new ArgumentException(
                "OrderId is required.");
        }

        var order =
            await _orderRepository
                .GetByIdAsync(
                    orderId);

        if (order is null)
        {
            throw new KeyNotFoundException(
                "Order not found.");
        }

        return order;
    }

    // ============================================================
    // CONFIGURATION DECIMAL
    // ============================================================

    private decimal GetDecimalSetting(
        string key,
        decimal defaultValue)
    {
        var value =
            _configuration[
                key];

        if (string.IsNullOrWhiteSpace(
            value))
        {
            return defaultValue;
        }

        if (!decimal.TryParse(
            value,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var result))
        {
            throw new InvalidOperationException(
                $"Configuration value '{key}' is invalid.");
        }

        return result;
    }

    // ============================================================
    // ENTITY -> DTO
    // ============================================================

    private static OrderDto MapToDto(
        Order order)
    {
        return new OrderDto
        {
            Id =
                order.Id,

            OrderNumber =
                order.OrderNumber,

            UserId =
                order.UserId,

            // ====================================================
            // FINANCIAL
            // ====================================================

            Subtotal =
                order.Subtotal,

            TaxableAmount =
                order.TaxableAmount,

            TotalGst =
                order.TotalGst,

            TotalCgst =
                order.TotalCgst,

            TotalSgst =
                order.TotalSgst,

            TotalIgst =
                order.TotalIgst,

            ShippingCharge =
                order.ShippingCharge,

            DiscountAmount =
                order.DiscountAmount,

            TotalAmount =
                order.TotalAmount,

            // ====================================================
            // STATUS
            // ====================================================

            Status =
                order.Status,

            PaymentStatus =
                order.PaymentStatus,

            // ====================================================
            // SHIPPING
            // ====================================================

            ShippingAddress =
                order.ShippingAddress,

            ShippingState =
                order.ShippingState,

            ShippingStateCode =
                order.ShippingStateCode,

            PostalCode =
                order.PostalCode,

            IsInterState =
                order.IsInterState,

            // ====================================================
            // DATES
            // ====================================================

            CreatedAt =
                order.CreatedAt,

            UpdatedAt =
                order.UpdatedAt,

            // ====================================================
            // ITEMS
            // ====================================================

            OrderItems =
                order.OrderItems
                    .Select(
                        item =>
                            new OrderItemDto
                            {
                                Id =
                                    item.Id,

                                ProductId =
                                    item.ProductId,

                                ProductName =
                                    item.ProductName,

                                SKU =
                                    item.SKU,

                                HsnCode =
                                    item.HsnCode,

                                Quantity =
                                    item.Quantity,

                                UnitPrice =
                                    item.UnitPrice,

                                GstRate =
                                    item.GstRate,

                                TaxableAmount =
                                    item.TaxableAmount,

                                GstAmount =
                                    item.GstAmount,

                                CgstAmount =
                                    item.CgstAmount,

                                SgstAmount =
                                    item.SgstAmount,

                                IgstAmount =
                                    item.IgstAmount,

                                TotalPrice =
                                    item.TotalPrice
                            })
                    .ToList()
        };
    }
}