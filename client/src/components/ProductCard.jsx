// ============================================================
// PRODUCT CARD COMPONENT
// ============================================================
//
// Reusable product card used inside ProductsPage.
//
// Displays:
// - Product name
// - Description
// - SKU
// - Price
// - Stock status
// - Add to Cart button
// ============================================================

function ProductCard({
    product,
    onAddToCart,
    adding
}) {

    // ========================================================
    // STOCK STATUS
    // ========================================================

    const stock =
        product.stockQuantity ?? 0

    const isOutOfStock =
        stock <= 0

    const isLowStock =
        stock > 0 && stock <= 5

    // ========================================================
    // PRICE FORMAT
    // ========================================================

    const formattedPrice =
        Number(
            product.price || 0
        ).toLocaleString(
            'en-IN',
            {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            }
        )

    return (

        <article className="product-card">

            {/* ==================================================
                CARD TOP
               ================================================== */}

            <div className="product-card-top">

                <div className="product-card-icon">
                    {product.name
                        ?.charAt(0)
                        .toUpperCase() || 'P'}
                </div>

                <div
                    className={
                        `product-stock-badge ${isOutOfStock
                            ? 'out-of-stock'
                            : isLowStock
                                ? 'low-stock'
                                : 'in-stock'
                        }`
                    }
                >

                    {isOutOfStock
                        ? 'Out of Stock'
                        : isLowStock
                            ? 'Low Stock'
                            : 'In Stock'}

                </div>

            </div>

            {/* ==================================================
                PRODUCT INFORMATION
               ================================================== */}

            <div className="product-card-content">

                {product.sku && (

                    <span className="product-sku">
                        SKU: {product.sku}
                    </span>

                )}

                <h2>
                    {product.name}
                </h2>

                <p className="product-description">

                    {product.description ||
                        'No description available.'}

                </p>

            </div>

            {/* ==================================================
                PRICE
               ================================================== */}

            <div className="product-price">

                <span className="product-price-label">
                    Price
                </span>

                <strong>
                    ₹{formattedPrice}
                </strong>

            </div>

            {/* ==================================================
                STOCK
               ================================================== */}

            <div className="product-stock-info">

                <span>
                    Available Stock
                </span>

                <strong
                    className={
                        isOutOfStock
                            ? 'stock-danger'
                            : isLowStock
                                ? 'stock-warning'
                                : ''
                    }
                >
                    {stock}
                </strong>

            </div>

            {/* ==================================================
                ADD TO CART
               ================================================== */}

            <button
                type="button"
                className="product-add-button"
                disabled={
                    adding ||
                    isOutOfStock
                }
                onClick={() =>
                    onAddToCart(product)
                }
            >

                {adding
                    ? 'Adding...'
                    : isOutOfStock
                        ? 'Out of Stock'
                        : 'Add to Cart'}

            </button>

        </article>
    )
}

export default ProductCard