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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly IConfiguration _configuration;

    public OrderService(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IKafkaProducer kafkaProducer,
        IConfiguration configuration)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _kafkaProducer = kafkaProducer;
        _configuration = configuration;
    }

    // ============================================================
    // CREATE ORDER / CHECKOUT
    // ============================================================

    /// <summary>
    /// Converts the authenticated user's cart into an order.
    ///
    /// Database changes are executed inside one transaction.
    /// After the transaction commits successfully, an
    /// OrderCreatedEvent is published to Kafka.
    /// </summary>
    public async Task<OrderDto> CreateOrderAsync(
        Guid userId,
        CreateOrderRequest request)
    {
        // --------------------------------------------------------
        // Validation
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

        // --------------------------------------------------------
        // Read Kafka configuration BEFORE changing the database.
        // --------------------------------------------------------

        var topic =
            _configuration["Kafka:OrderEventsTopic"]
            ?? throw new InvalidOperationException(
                "Kafka OrderEventsTopic is not configured.");

        Order? order = null;

        // --------------------------------------------------------
        // Start database transaction
        // --------------------------------------------------------

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // ----------------------------------------------------
            // Load user's cart
            // ----------------------------------------------------

            var cart = await _cartRepository
                .GetByUserIdAsync(userId);

            if (cart is null ||
                cart.Items.Count == 0)
            {
                throw new InvalidOperationException(
                    "Cart is empty.");
            }

            // ----------------------------------------------------
            // Create order
            // ----------------------------------------------------

            order = new Order(
                userId,
                request.ShippingAddress.Trim());

            // ----------------------------------------------------
            // Convert CartItems → OrderItems
            // ----------------------------------------------------

            foreach (var cartItem in cart.Items)
            {
                var product =
                    await _productRepository
                        .GetByIdAsync(
                            cartItem.ProductId);

                // ------------------------------------------------
                // Product must exist and be active
                // ------------------------------------------------

                if (product is null ||
                    !product.IsActive)
                {
                    throw new InvalidOperationException(
                        $"Product '{cartItem.ProductId}' " +
                        "is no longer available.");
                }

                // ------------------------------------------------
                // Validate available stock
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
                // Snapshot the current product price.
                //
                // If the admin changes the product price later,
                // this OrderItem keeps the purchase-time price.
                // ------------------------------------------------

                var orderItem = new OrderItem(
                    product.Id,
                    product.Name,
                    cartItem.Quantity,
                    product.Price);

                order.AddItem(orderItem);

                // ------------------------------------------------
                // Reduce product inventory
                // ------------------------------------------------

                product.ReduceStock(
                    cartItem.Quantity);

                await _productRepository
                    .UpdateAsync(product);
            }

            // ----------------------------------------------------
            // Save order + order items
            // ----------------------------------------------------

            await _orderRepository
                .AddAsync(order);

            // ----------------------------------------------------
            // Clear user's cart
            // ----------------------------------------------------

            cart.Clear();

            await _cartRepository
                .UpdateAsync(cart);

            // ----------------------------------------------------
            // Commit database transaction
            // ----------------------------------------------------

            await _unitOfWork
                .CommitTransactionAsync();
        }
        catch
        {
            // ----------------------------------------------------
            // Rollback database operations if anything BEFORE
            // commit fails.
            //
            // This rolls back:
            // - Product stock changes
            // - Order
            // - OrderItems
            // - Cart clearing
            // ----------------------------------------------------

            await _unitOfWork
                .RollbackTransactionAsync();

            throw;
        }

        // --------------------------------------------------------
        // Transaction has successfully committed.
        //
        // Kafka publishing happens AFTER database commit.
        // Therefore a Kafka failure cannot roll back an order
        // that has already been permanently saved.
        // --------------------------------------------------------

        if (order is null)
        {
            throw new InvalidOperationException(
                "Order creation failed.");
        }

        var orderCreatedEvent =
            new OrderCreatedEvent
            {
                OrderId =
                    order.Id,

                OrderNumber =
                    order.OrderNumber,

                UserId =
                    order.UserId,

                TotalAmount =
                    order.TotalAmount,

                CreatedAt =
                    order.CreatedAt
            };

        await _kafkaProducer.PublishAsync(
            topic,
            orderCreatedEvent);

        return MapToDto(order);
    }

    // ============================================================
    // GET USER ORDERS
    // ============================================================

    public async Task<IReadOnlyList<OrderDto>>
        GetUserOrdersAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "UserId is required.");
        }

        var orders =
            await _orderRepository
                .GetByUserIdAsync(userId);

        return orders
            .Select(MapToDto)
            .ToList();
    }

    // ============================================================
    // GET ORDER BY ID
    // ============================================================

    public async Task<OrderDto?> GetOrderByIdAsync(
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
                .GetByIdAsync(orderId);

        if (order is null ||
            order.UserId != userId)
        {
            return null;
        }

        return MapToDto(order);
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

        return orders
            .Select(MapToDto)
            .ToList();
    }

    // ============================================================
    // ADMIN - CONFIRM ORDER
    // ============================================================

    public async Task<OrderDto> ConfirmOrderAsync(
        Guid orderId)
    {
        var order =
            await GetRequiredOrderAsync(
                orderId);

        order.Confirm();

        await _orderRepository
            .UpdateAsync(order);

        return MapToDto(order);
    }

    // ============================================================
    // ADMIN - START PROCESSING
    // ============================================================

    public async Task<OrderDto> StartProcessingAsync(
        Guid orderId)
    {
        var order =
            await GetRequiredOrderAsync(
                orderId);

        order.StartProcessing();

        await _orderRepository
            .UpdateAsync(order);

        return MapToDto(order);
    }

    // ============================================================
    // ADMIN - SHIP ORDER
    // ============================================================

    public async Task<OrderDto> ShipOrderAsync(
        Guid orderId)
    {
        var order =
            await GetRequiredOrderAsync(
                orderId);

        order.Ship();

        await _orderRepository
            .UpdateAsync(order);

        return MapToDto(order);
    }

    // ============================================================
    // ADMIN - DELIVER ORDER
    // ============================================================

    public async Task<OrderDto> DeliverOrderAsync(
        Guid orderId)
    {
        var order =
            await GetRequiredOrderAsync(
                orderId);

        order.Deliver();

        await _orderRepository
            .UpdateAsync(order);

        return MapToDto(order);
    }

    // ============================================================
    // ADMIN - CANCEL ORDER
    // ============================================================

    public async Task<OrderDto> CancelOrderAsync(
        Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException(
                "OrderId is required.");
        }

        await _unitOfWork
            .BeginTransactionAsync();

        try
        {
            var order =
                await _orderRepository
                    .GetByIdAsync(orderId);

            if (order is null)
            {
                throw new KeyNotFoundException(
                    "Order not found.");
            }

            // ----------------------------------------------------
            // Validate and cancel through the domain entity.
            // ----------------------------------------------------

            order.Cancel();

            // ----------------------------------------------------
            // Restore inventory for every cancelled order item.
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
                    .UpdateAsync(product);
            }

            // ----------------------------------------------------
            // Save cancelled order status
            // ----------------------------------------------------

            await _orderRepository
                .UpdateAsync(order);

            // ----------------------------------------------------
            // Commit cancellation + inventory restoration
            // ----------------------------------------------------

            await _unitOfWork
                .CommitTransactionAsync();

            return MapToDto(order);
        }
        catch
        {
            await _unitOfWork
                .RollbackTransactionAsync();

            throw;
        }
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
                .GetByIdAsync(orderId);

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

            OrderItems = order.OrderItems
                .Select(item =>
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