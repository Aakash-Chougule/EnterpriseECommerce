using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Services;

/// <summary>
/// Handles order and checkout-related business logic.
/// </summary>
public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;

    public OrderService(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    // ============================================================
    // CREATE ORDER / CHECKOUT
    // ============================================================

    /// <summary>
    /// Converts the authenticated user's cart into an order.
    ///
    /// During checkout:
    /// - Products are validated.
    /// - Current product prices are copied into OrderItems.
    /// - Product stock is reduced.
    /// - The order is saved.
    /// - The cart is cleared.
    /// </summary>
    public async Task<OrderDto> CreateOrderAsync(
        Guid userId,
        CreateOrderRequest request)
    {
        // --------------------------------------------------------
        // Validate UserId
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

        if (string.IsNullOrWhiteSpace(
            request.ShippingAddress))
        {
            throw new ArgumentException(
                "Shipping address is required.");
        }

        // --------------------------------------------------------
        // Load user's cart
        // --------------------------------------------------------

        var cart = await _cartRepository
            .GetByUserIdAsync(userId);

        if (cart is null ||
            cart.Items.Count == 0)
        {
            throw new InvalidOperationException(
                "Cart is empty.");
        }

        // --------------------------------------------------------
        // Create Order
        // --------------------------------------------------------

        var order = new Order(
            userId,
            request.ShippingAddress.Trim());

        // --------------------------------------------------------
        // Convert CartItems → OrderItems
        // --------------------------------------------------------

        foreach (var cartItem in cart.Items)
        {
            // ----------------------------------------------------
            // Load product
            // ----------------------------------------------------

            var product = await _productRepository
                .GetByIdAsync(cartItem.ProductId);

            // ----------------------------------------------------
            // Product must exist and be active
            // ----------------------------------------------------

            if (product is null ||
                !product.IsActive)
            {
                throw new InvalidOperationException(
                    $"Product '{cartItem.ProductId}' " +
                    "is no longer available.");
            }

            // ----------------------------------------------------
            // Validate stock
            // ----------------------------------------------------

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

            // ----------------------------------------------------
            // Create OrderItem
            //
            // IMPORTANT:
            // Product.Price is copied into UnitPrice.
            //
            // This preserves the purchase price even if the
            // admin changes Product.Price later.
            // ----------------------------------------------------

            var orderItem = new OrderItem(
                product.Id,
                product.Name,
                cartItem.Quantity,
                product.Price);

            // ----------------------------------------------------
            // Add item to Order
            //
            // Order.AddItem() automatically recalculates
            // TotalAmount.
            // ----------------------------------------------------

            order.AddItem(orderItem);

            // ----------------------------------------------------
            // Reduce product stock
            // ----------------------------------------------------

            product.ReduceStock(
                cartItem.Quantity);

            // ----------------------------------------------------
            // Persist stock change
            // ----------------------------------------------------

            await _productRepository
                .UpdateAsync(product);
        }

        // --------------------------------------------------------
        // Save Order
        // --------------------------------------------------------

        await _orderRepository
            .AddAsync(order);

        // --------------------------------------------------------
        // Clear cart only after order has been created.
        // --------------------------------------------------------

        cart.Clear();

        await _cartRepository
            .UpdateAsync(cart);

        // --------------------------------------------------------
        // Return API DTO
        // --------------------------------------------------------

        return MapToDto(order);
    }

    // ============================================================
    // GET USER ORDERS
    // ============================================================

    /// <summary>
    /// Returns all orders belonging to a user.
    /// </summary>
    public async Task<IReadOnlyList<OrderDto>>
        GetUserOrdersAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "UserId is required.");
        }

        var orders = await _orderRepository
            .GetByUserIdAsync(userId);

        return orders
            .Select(MapToDto)
            .ToList();
    }

    // ============================================================
    // GET ORDER BY ID
    // ============================================================

    /// <summary>
    /// Returns a specific order belonging to the user.
    ///
    /// A user cannot retrieve another user's order.
    /// </summary>
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

        var order = await _orderRepository
            .GetByIdAsync(orderId);

        if (order is null ||
            order.UserId != userId)
        {
            return null;
        }

        return MapToDto(order);
    }

    // ============================================================
    // MAPPING
    // ============================================================

    private static OrderDto MapToDto(
        Order order)
    {
        return new OrderDto
        {
            Id = order.Id,

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