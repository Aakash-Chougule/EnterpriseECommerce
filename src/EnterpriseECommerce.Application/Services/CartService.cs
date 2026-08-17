using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.Application.Services;

/// <summary>
/// Contains shopping-cart business logic.
///
/// Responsibilities:
/// - Find or create a user's cart.
/// - Validate products.
/// - Validate available stock.
/// - Add products to the cart.
/// - Remove products from the cart.
/// - Clear the cart.
/// - Enrich cart items with product information.
/// - Map domain entities to API DTOs.
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

    // ============================================================
    // GET USER CART
    // ============================================================

    /// <summary>
    /// Returns the authenticated user's cart.
    ///
    /// If the user does not currently have a cart,
    /// a new empty cart is automatically created.
    /// </summary>
    public async Task<CartDto> GetCartAsync(
        Guid userId)
    {
        ValidateUserId(userId);

        var cart = await _cartRepository
            .GetByUserIdAsync(userId);

        // --------------------------------------------------------
        // Automatically create an empty cart for a new user.
        // --------------------------------------------------------

        if (cart is null)
        {
            cart = new Cart(userId);

            await _cartRepository
                .AddAsync(cart);
        }

        return await MapToDtoAsync(cart);
    }

    // ============================================================
    // ADD PRODUCT TO CART
    // ============================================================

    /// <summary>
    /// Adds a product to the user's cart.
    ///
    /// If the product already exists in the cart,
    /// its quantity is increased.
    ///
    /// The total quantity may never exceed available stock.
    /// </summary>
    public async Task<CartDto> AddItemAsync(
        Guid userId,
        AddCartItemRequest request)
    {
        ValidateUserId(userId);

        // --------------------------------------------------------
        // Validate request
        // --------------------------------------------------------

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
        // Load product
        // --------------------------------------------------------

        var product =
            await _productRepository
                .GetByIdAsync(
                    request.ProductId);

        // --------------------------------------------------------
        // Product must exist and be active.
        // --------------------------------------------------------

        if (product is null ||
            !product.IsActive)
        {
            throw new KeyNotFoundException(
                "Product not found.");
        }

        // --------------------------------------------------------
        // Load existing cart
        // --------------------------------------------------------

        var existingCart =
            await _cartRepository
                .GetByUserIdAsync(userId);

        Cart cart;

        // ========================================================
        // CREATE NEW CART
        // ========================================================

        if (existingCart is null)
        {
            // ----------------------------------------------------
            // Check requested quantity against available stock.
            // ----------------------------------------------------

            if (request.Quantity >
                product.StockQuantity)
            {
                throw new ArgumentException(
                    "Requested quantity is greater than " +
                    "available stock.");
            }

            cart = new Cart(userId);

            cart.AddItem(
                request.ProductId,
                request.Quantity);

            await _cartRepository
                .AddAsync(cart);
        }

        // ========================================================
        // UPDATE EXISTING CART
        // ========================================================

        else
        {
            cart = existingCart;

            // ----------------------------------------------------
            // Find existing product inside cart.
            // ----------------------------------------------------

            var existingItem =
                cart.Items
                    .FirstOrDefault(item =>
                        item.ProductId ==
                        request.ProductId);

            var currentQuantity =
                existingItem?.Quantity ?? 0;

            var totalQuantity =
                currentQuantity +
                request.Quantity;

            // ----------------------------------------------------
            // Make sure total quantity does not exceed stock.
            // ----------------------------------------------------

            if (totalQuantity >
                product.StockQuantity)
            {
                throw new ArgumentException(
                    $"Requested quantity exceeds available stock. " +
                    $"Available stock: {product.StockQuantity}. " +
                    $"Already in cart: {currentQuantity}.");
            }

            // ----------------------------------------------------
            // Domain entity handles whether to create a new item
            // or increase an existing item's quantity.
            // ----------------------------------------------------

            cart.AddItem(
                request.ProductId,
                request.Quantity);

            await _cartRepository
                .UpdateAsync(cart);
        }

        return await MapToDtoAsync(cart);
    }

    public async Task<CartDto> UpdateItemQuantityAsync(
    Guid userId,
    Guid productId,
    UpdateCartItemRequest request)
    {
        ValidateUserId(userId);

        if (productId == Guid.Empty)
        {
            throw new ArgumentException(
                "ProductId is required.");
        }

        if (request is null)
        {
            throw new ArgumentNullException(
                nameof(request));
        }

        if (request.Quantity <= 0)
        {
            throw new ArgumentException(
                "Quantity must be greater than zero.");
        }

        // --------------------------------------------------------
        // Load product so we can validate current stock.
        // --------------------------------------------------------

        var product = await _productRepository
            .GetByIdAsync(productId);

        if (product is null ||
            !product.IsActive)
        {
            throw new KeyNotFoundException(
                "Product not found.");
        }

        if (request.Quantity >
            product.StockQuantity)
        {
            throw new ArgumentException(
                $"Requested quantity exceeds available stock. " +
                $"Available stock: {product.StockQuantity}.");
        }

        // --------------------------------------------------------
        // Load user's cart
        // --------------------------------------------------------

        var cart = await _cartRepository
            .GetByUserIdAsync(userId);

        if (cart is null)
        {
            throw new KeyNotFoundException(
                "Cart not found.");
        }

        // --------------------------------------------------------
        // Update quantity through domain entity
        // --------------------------------------------------------

        cart.UpdateItemQuantity(
            productId,
            request.Quantity);

        await _cartRepository
            .UpdateAsync(cart);

        return await MapToDtoAsync(cart);
    }

    // ============================================================
    // REMOVE PRODUCT FROM CART
    // ============================================================

    /// <summary>
    /// Completely removes a product from the user's cart.
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

        var cart =
            await _cartRepository
                .GetByUserIdAsync(userId);

        if (cart is null)
        {
            throw new KeyNotFoundException(
                "Cart not found.");
        }

        // --------------------------------------------------------
        // Domain entity handles removal.
        // --------------------------------------------------------

        cart.RemoveItem(productId);

        await _cartRepository
            .UpdateAsync(cart);

        return await MapToDtoAsync(cart);
    }

    // ============================================================
    // CLEAR CART
    // ============================================================

    /// <summary>
    /// Removes every product from the user's cart.
    /// </summary>
    public async Task<CartDto> ClearCartAsync(
        Guid userId)
    {
        ValidateUserId(userId);

        var cart =
            await _cartRepository
                .GetByUserIdAsync(userId);

        if (cart is null)
        {
            throw new KeyNotFoundException(
                "Cart not found.");
        }

        cart.Clear();

        await _cartRepository
            .UpdateAsync(cart);

        return await MapToDtoAsync(cart);
    }

    // ============================================================
    // MAP CART → CART DTO
    // ============================================================

    /// <summary>
    /// Maps the domain Cart entity into the DTO returned by the API.
    ///
    /// CartItem only stores ProductId and Quantity.
    ///
    /// Therefore we load each Product here so the frontend also
    /// receives:
    ///
    /// - ProductName
    /// - UnitPrice
    /// - TotalPrice
    ///
    /// This keeps product information out of the Cart domain
    /// entity while still providing useful UI data.
    /// </summary>
    private async Task<CartDto> MapToDtoAsync(
        Cart cart)
    {
        var itemDtos =
            new List<CartItemDto>();

        // --------------------------------------------------------
        // Enrich every CartItem with Product information.
        // --------------------------------------------------------

        foreach (var item in cart.Items)
        {
            var product =
                await _productRepository
                    .GetByIdAsync(
                        item.ProductId);

            // ----------------------------------------------------
            // A CartItem should normally always reference an
            // existing Product because of the database FK.
            //
            // We still handle a missing product defensively.
            // ----------------------------------------------------

            if (product is null)
            {
                itemDtos.Add(
                    new CartItemDto
                    {
                        Id =
                            item.Id,

                        ProductId =
                            item.ProductId,

                        ProductName =
                            "Product unavailable",

                        UnitPrice =
                            0,

                        Quantity =
                            item.Quantity,

                        TotalPrice =
                            0
                    });

                continue;
            }

            // ----------------------------------------------------
            // Calculate this cart line's current total.
            // ----------------------------------------------------

            var totalPrice =
                product.Price *
                item.Quantity;

            itemDtos.Add(
                new CartItemDto
                {
                    Id =
                        item.Id,

                    ProductId =
                        item.ProductId,

                    ProductName =
                        product.Name,

                    UnitPrice =
                        product.Price,

                    Quantity =
                        item.Quantity,

                    TotalPrice =
                        totalPrice
                });
        }

        // --------------------------------------------------------
        // Calculate complete cart value.
        // --------------------------------------------------------

        var totalAmount =
            itemDtos.Sum(item =>
                item.TotalPrice);

        return new CartDto
        {
            Id =
                cart.Id,

            UserId =
                cart.UserId,

            Items =
                itemDtos,

            TotalAmount =
                totalAmount,

            CreatedAt =
                cart.CreatedAt,

            UpdatedAt =
                cart.UpdatedAt
        };
    }

    // ============================================================
    // VALIDATION
    // ============================================================

    private static void ValidateUserId(
        Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "UserId is required.");
        }
    }
}