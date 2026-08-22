using System.Globalization;

using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Domain.Entities;

using Microsoft.Extensions.Configuration;

namespace EnterpriseECommerce.Application.Services;

public class CheckoutPricingService
{
    private readonly ICartRepository
        _cartRepository;

    private readonly IProductRepository
        _productRepository;

    private readonly IConfiguration
        _configuration;

    public CheckoutPricingService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IConfiguration configuration)
    {
        _cartRepository =
            cartRepository;

        _productRepository =
            productRepository;

        _configuration =
            configuration;
    }

    // ============================================================
    // CHECKOUT PREVIEW
    // ============================================================

    public async Task<CheckoutPreviewDto>
        GetPreviewAsync(
            Guid userId,
            CheckoutPreviewRequest request)
    {
        // ========================================================
        // VALIDATION
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

        // ========================================================
        // CONFIGURATION
        // ========================================================

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
        // CART
        // ========================================================

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

        // ========================================================
        // DETERMINE TAX TYPE
        // ========================================================

        var shippingStateCode =
            request
                .ShippingStateCode
                .Trim();

        var isInterState =
            !string.Equals(
                sellerStateCode.Trim(),
                shippingStateCode,
                StringComparison.OrdinalIgnoreCase);

        var previewItems =
            new List<CheckoutPreviewItemDto>();

        // ========================================================
        // TOTALS
        // ========================================================

        decimal subtotal =
            0m;

        decimal taxableAmount =
            0m;

        decimal totalGst =
            0m;

        decimal totalCgst =
            0m;

        decimal totalSgst =
            0m;

        decimal totalIgst =
            0m;

        var totalQuantity =
            0;

        // ========================================================
        // PRODUCTS
        // ========================================================

        foreach (
            var cartItem in
            cart.Items)
        {
            var product =
                await _productRepository
                    .GetByIdAsync(
                        cartItem.ProductId);

            if (product is null ||
                !product.IsActive)
            {
                throw new InvalidOperationException(
                    $"Product '{cartItem.ProductId}' " +
                    "is no longer available.");
            }

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

            // ====================================================
            // USE DOMAIN TAX CALCULATION
            // ====================================================

            var item =
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
                        isInterState);

            subtotal +=
                item.TotalPrice;

            taxableAmount +=
                item.TaxableAmount;

            totalGst +=
                item.GstAmount;

            totalCgst +=
                item.CgstAmount;

            totalSgst +=
                item.SgstAmount;

            totalIgst +=
                item.IgstAmount;

            totalQuantity +=
                item.Quantity;

            previewItems.Add(
                new CheckoutPreviewItemDto
                {
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
                });
        }

        // ========================================================
        // ROUND
        // ========================================================

        subtotal =
            RoundMoney(
                subtotal);

        taxableAmount =
            RoundMoney(
                taxableAmount);

        totalGst =
            RoundMoney(
                totalGst);

        totalCgst =
            RoundMoney(
                totalCgst);

        totalSgst =
            RoundMoney(
                totalSgst);

        totalIgst =
            RoundMoney(
                totalIgst);

        // ========================================================
        // SHIPPING
        // ========================================================

        var shippingCharge =
            subtotal >=
            freeShippingThreshold
                ? 0m
                : defaultShippingCharge;

        shippingCharge =
            RoundMoney(
                shippingCharge);

        // ========================================================
        // DISCOUNT
        // ========================================================
        //
        // Coupon system is not implemented yet.
        // ========================================================

        const decimal discountAmount =
            0m;

        // ========================================================
        // FINAL PAYABLE AMOUNT
        // ========================================================
        //
        // GST is ALREADY included in subtotal.
        //
        // Do NOT:
        //
        // subtotal + GST + shipping
        //
        // Correct:
        //
        // subtotal + shipping - discount
        //
        // ========================================================

        var totalAmount =
            RoundMoney(
                subtotal +
                shippingCharge -
                discountAmount);

        return new CheckoutPreviewDto
        {
            ProductCount =
                previewItems.Count,

            TotalQuantity =
                totalQuantity,

            Items =
                previewItems,

            Subtotal =
                subtotal,

            TaxableAmount =
                taxableAmount,

            TotalGst =
                totalGst,

            TotalCgst =
                totalCgst,

            TotalSgst =
                totalSgst,

            TotalIgst =
                totalIgst,

            ShippingCharge =
                shippingCharge,

            DiscountAmount =
                discountAmount,

            TotalAmount =
                totalAmount,

            IsInterState =
                isInterState,

            ShippingState =
                request
                    .ShippingState
                    .Trim(),

            ShippingStateCode =
                shippingStateCode
        };
    }

    // ============================================================
    // ROUND MONEY
    // ============================================================

    private static decimal RoundMoney(
        decimal value)
    {
        return Math.Round(
            value,
            2,
            MidpointRounding.AwayFromZero);
    }

    // ============================================================
    // CONFIG DECIMAL
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
}