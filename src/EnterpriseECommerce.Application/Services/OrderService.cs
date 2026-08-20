using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Events;
using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;

using Microsoft.Extensions.Configuration;

namespace EnterpriseECommerce.Application.Services;

/// <summary>
/// Handles order and checkout-related business logic.
/// </summary>
public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly IConfiguration _configuration;

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

    public async Task<OrderDto> CreateOrderAsync(
        Guid userId,
        CreateOrderRequest request)
    {
        // --------------------------------------------------------
        // VALIDATION
        // --------------------------------------------------------

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

        if (string.IsNullOrWhiteSpace(
            request.ShippingAddress))
        {
            throw new ArgumentException(
                "Shipping address is required.");
        }

        if (string.IsNullOrWhiteSpace(
            request.PaymentMethod))
        {
            throw new ArgumentException(
                "Payment method is required.");
        }

        // --------------------------------------------------------
        // PAYMENT METHOD VALIDATION
        // --------------------------------------------------------

        var supportedPaymentMethods =
            new[]
            {
                "UPI",
                "Card",
                "NetBanking",
                "COD"
            };

        var paymentMethod =
            request.PaymentMethod.Trim();

        if (!supportedPaymentMethods.Contains(
            paymentMethod,
            StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Invalid payment method.");
        }

        // --------------------------------------------------------
        // KAFKA TOPIC
        // --------------------------------------------------------

        var topic =
            _configuration[
                "Kafka:OrderEventsTopic"]
            ?? throw new InvalidOperationException(
                "Kafka OrderEventsTopic is not configured.");

        // --------------------------------------------------------
        // LOAD CUSTOMER
        // --------------------------------------------------------

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

        Order? order = null;

        // --------------------------------------------------------
        // BEGIN TRANSACTION
        // --------------------------------------------------------

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
                    userId,
                    request.ShippingAddress.Trim());

            // ====================================================
            // CART ITEMS -> ORDER ITEMS
            // ====================================================

            foreach (var cartItem in
                     cart.Items)
            {
                var product =
                    await _productRepository
                        .GetByIdAsync(
                            cartItem.ProductId);

                // ------------------------------------------------
                // PRODUCT AVAILABILITY
                // ------------------------------------------------

                if (product is null ||
                    !product.IsActive)
                {
                    throw new InvalidOperationException(
                        $"Product '{cartItem.ProductId}' " +
                        "is no longer available.");
                }

                // ------------------------------------------------
                // STOCK VALIDATION
                // ------------------------------------------------

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

                // ------------------------------------------------
                // ORDER ITEM
                // ------------------------------------------------

                var orderItem =
                    new OrderItem(
                        product.Id,
                        product.Name,
                        cartItem.Quantity,
                        product.Price);

                order.AddItem(
                    orderItem);

                // ------------------------------------------------
                // REDUCE INVENTORY
                // ------------------------------------------------

                product.ReduceStock(
                    cartItem.Quantity);

                await _productRepository
                    .UpdateAsync(
                        product);
            }

            // ====================================================
            // AUTO CONFIRM ORDER
            // ====================================================
            //
            // Checkout itself confirms the order.
            //
            // New orders will therefore start as:
            //
            // Confirmed
            //
            // instead of:
            //
            // Pending
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

        // --------------------------------------------------------
        // PUBLISH AFTER DATABASE COMMIT
        // --------------------------------------------------------

        await _kafkaProducer
            .PublishAsync(
                topic,
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
        if (userId == Guid.Empty)
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

        var order =
            await _orderRepository
                .GetByIdAsync(
                    orderId);

        if (order is null ||
            order.UserId != userId)
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

        foreach (var order in
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
    //
    // Kept for older Pending orders.
    //
    // New checkout orders are automatically Confirmed.
    // ============================================================

    public async Task<OrderDto>
        ConfirmOrderAsync(
            Guid orderId)
    {
        var order =
            await GetRequiredOrderAsync(
                orderId);

        var previousStatus =
            order.Status.ToString();

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
            order.Status.ToString();

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
    // ADMIN - SHIP ORDER
    // ============================================================

    public async Task<OrderDto>
        ShipOrderAsync(
            Guid orderId)
    {
        var order =
            await GetRequiredOrderAsync(
                orderId);

        var previousStatus =
            order.Status.ToString();

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
    // ADMIN - DELIVER ORDER
    // ============================================================

    public async Task<OrderDto>
        DeliverOrderAsync(
            Guid orderId)
    {
        var order =
            await GetRequiredOrderAsync(
                orderId);

        var previousStatus =
            order.Status.ToString();

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
        if (orderId == Guid.Empty)
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
                order.Status.ToString();

            // ----------------------------------------------------
            // CANCEL ORDER
            // ----------------------------------------------------

            order.Cancel();

            // ----------------------------------------------------
            // RESTORE STOCK
            // ----------------------------------------------------

            foreach (var orderItem in
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

        // --------------------------------------------------------
        // PUBLISH AFTER COMMIT
        // --------------------------------------------------------

        await PublishOrderStatusChangedEventAsync(
            order,
            previousStatus);

        return MapToDto(
            order);
    }

    // ============================================================
    // PUBLISH ORDER STATUS EVENT
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
                    order.Status.ToString(),

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
    // INTERNAL ORDER LOOKUP
    // ============================================================

    private async Task<Order>
        GetRequiredOrderAsync(
            Guid orderId)
    {
        if (orderId == Guid.Empty)
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
    // MAPPING
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

            TotalAmount =
                order.TotalAmount,

            Status =
                order.Status,

            PaymentStatus =
                order.PaymentStatus,

            ShippingAddress =
                order.ShippingAddress,

            CreatedAt =
                order.CreatedAt,

            UpdatedAt =
                order.UpdatedAt,

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

                                Quantity =
                                    item.Quantity,

                                UnitPrice =
                                    item.UnitPrice,

                                TotalPrice =
                                    item.TotalPrice
                            })
                    .ToList()
        };
    }
}