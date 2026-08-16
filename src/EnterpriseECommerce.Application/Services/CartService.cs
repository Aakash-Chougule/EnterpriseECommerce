using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Services;

/// <summary>
/// Contains shopping-cart business logic.
///
/// The service is responsible for:
/// - Finding or creating a user's cart
/// - Validating products
/// - Adding products to the cart
/// - Removing products from the cart
/// - Clearing the cart
/// - Mapping domain entities to DTOs
///
/// Controllers should not directly access repositories.
/// </summary>
public class CartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;

    public CartService(
        ICartRepository cartRepository,
        IProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
    }

    // ------------------------------------------------------------
    // Get user's cart
    // ------------------------------------------------------------

    /// <summary>
    /// Gets the current user's cart.
    ///
    /// If the user does not have a cart yet, a new empty cart
    /// is automatically created.
    /// </summary>
    public async Task<CartDto> GetCartAsync(Guid userId)
    {
        ValidateUserId(userId);

        var cart = await _cartRepository.GetByUserIdAsync(userId);

        if (cart is null)
        {
            cart = new Cart(userId);

            await _cartRepository.AddAsync(cart);
        }

        return MapToDto(cart);
    }

    // ------------------------------------------------------------
    // Add product to cart
    // ------------------------------------------------------------

    /// <summary>
    /// Adds a product to the user's cart.
    ///
    /// If the product already exists in the cart, the quantity
    /// is increased by the requested quantity.
    /// </summary>
    public async Task<CartDto> AddItemAsync(
        Guid userId,
        AddCartItemRequest request)
    {
        ValidateUserId(userId);

        if (request is null)
        {
            throw new ArgumentNullException(
                nameof(request),
                "Request is required.");
        }

        if (request.ProductId == Guid.Empty)
        {
            throw new ArgumentException(
                "ProductId is required.");
        }

        if (request.Quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }

        // --------------------------------------------------------
        // Find the product
        // --------------------------------------------------------

        var product = await _productRepository
            .GetByIdAsync(request.ProductId);

        if (product is null || !product.IsActive)
        {
            throw new KeyNotFoundException(
                "Product not found.");
        }

        // --------------------------------------------------------
        // Find existing cart
        // --------------------------------------------------------

        var existingCart = await _cartRepository
            .GetByUserIdAsync(userId);

        Cart cart;

        // --------------------------------------------------------
        // Create new cart
        // --------------------------------------------------------

        if (existingCart is null)
        {
            // The requested quantity cannot be greater than
            // the available stock.
            if (request.Quantity > product.StockQuantity)
            {
                throw new ArgumentException(
                    "Requested quantity is greater than available stock.");
            }

            cart = new Cart(userId);

            cart.AddItem(
                request.ProductId,
                request.Quantity);

            await _cartRepository.AddAsync(cart);
        }
        else
        {
            cart = existingCart;

            // ----------------------------------------------------
            // Check whether this product is already in the cart.
            // ----------------------------------------------------

            var existingItem = cart.Items
                .FirstOrDefault(item =>
                    item.ProductId == request.ProductId);

            var currentQuantity = existingItem?.Quantity ?? 0;

            var totalQuantity =
                currentQuantity + request.Quantity;

            // ----------------------------------------------------
            // Make sure total cart quantity does not exceed stock.
            // ----------------------------------------------------

            if (totalQuantity > product.StockQuantity)
            {
                throw new ArgumentException(
                    $"Requested quantity exceeds available stock. " +
                    $"Available stock: {product.StockQuantity}. " +
                    $"Already in cart: {currentQuantity}.");
            }

            cart.AddItem(
                request.ProductId,
                request.Quantity);

            await _cartRepository.UpdateAsync(cart);
        }

        return MapToDto(cart);
    }

    // ------------------------------------------------------------
    // Remove product from cart
    // ------------------------------------------------------------

    /// <summary>
    /// Removes a product completely from the user's cart.
    /// </summary>
    public async Task<CartDto> RemoveItemAsync(
        Guid userId,
        Guid productId)
    {
        ValidateUserId(userId);

        if (productId == Guid.Empty)
        {
            throw new ArgumentException(
                "ProductId is required.");
        }

        var cart = await _cartRepository
            .GetByUserIdAsync(userId);

        if (cart is null)
        {
            throw new KeyNotFoundException(
                "Cart not found.");
        }

        cart.RemoveItem(productId);

        await _cartRepository.UpdateAsync(cart);

        return MapToDto(cart);
    }

    // ------------------------------------------------------------
    // Clear cart
    // ------------------------------------------------------------

    /// <summary>
    /// Removes all products from the user's cart.
    /// </summary>
    public async Task<CartDto> ClearCartAsync(Guid userId)
    {
        ValidateUserId(userId);

        var cart = await _cartRepository
            .GetByUserIdAsync(userId);

        if (cart is null)
        {
            throw new KeyNotFoundException(
                "Cart not found.");
        }

        cart.Clear();

        await _cartRepository.UpdateAsync(cart);

        return MapToDto(cart);
    }

    // ------------------------------------------------------------
    // Map domain entity to DTO
    // ------------------------------------------------------------

    private static CartDto MapToDto(Cart cart)
    {
        return new CartDto
        {
            Id = cart.Id,

            UserId = cart.UserId,

            CreatedAt = cart.CreatedAt,

            UpdatedAt = cart.UpdatedAt,

            Items = cart.Items
                .Select(item => new CartItemDto
                {
                    Id = item.Id,

                    ProductId = item.ProductId,

                    Quantity = item.Quantity
                })
                .ToList()
        };
    }

    // ------------------------------------------------------------
    // Validation
    // ------------------------------------------------------------

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "UserId is required.");
        }
    }
}
