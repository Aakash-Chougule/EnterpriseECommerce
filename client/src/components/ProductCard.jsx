// ============================================================
// PRODUCT CARD COMPONENT
// ============================================================
//
// This is a reusable component.
//
// Instead of writing the product HTML repeatedly inside
// ProductsPage, we create it once here.
//
// The "product" value is received from the parent component
// through React props.
// ============================================================

function ProductCard({
    product,
    onAddToCart,
    adding
}) {
    return (
        <div className="product-card">

            <h2>
                {product.name}
            </h2>

            <p>
                {product.description}
            </p>

            <p>
                <strong>Price:</strong>
                {' '}
                ₹{product.price}
            </p>

            {product.stockQuantity !== undefined && (
                <p>
                    <strong>Stock:</strong>
                    {' '}
                    {product.stockQuantity}
                </p>
            )}

            <button
                type="button"
                disabled={adding}
                onClick={() =>
                    onAddToCart(product)
                }
            >
                {adding
                    ? 'Adding...'
                    : 'Add to Cart'}
            </button>

        </div>
    )
}

export default ProductCard