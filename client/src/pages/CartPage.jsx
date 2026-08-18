import {
    useEffect,
    useState
} from 'react'

import {
    getCart,
    updateCartItemQuantity,
    removeItemFromCart,
    clearCart
} from '../services/cartService'

import {
    useNavigate
} from 'react-router-dom'

import './CartPage.css'

// ============================================================
// CART PAGE
// ============================================================

function CartPage() {

    const [cart, setCart] =
        useState(null)

    const [loading, setLoading] =
        useState(true)

    const [error, setError] =
        useState('')

    const [
        updatingProductId,
        setUpdatingProductId
    ] = useState(null)

    const [clearing, setClearing] =
        useState(false)

    const navigate =
        useNavigate()

    // ========================================================
    // LOAD CART
    // ========================================================

    const loadCart = async () => {

        try {

            setLoading(true)
            setError('')

            const data =
                await getCart()

            setCart(data)
        }
        catch (err) {

            console.error(
                'Failed to load cart:',
                err
            )

            setError(
                err.response?.data?.message ||
                'Unable to load cart.'
            )
        }
        finally {

            setLoading(false)
        }
    }

    // ========================================================
    // UPDATE QUANTITY
    // ========================================================

    const handleQuantityChange =
        async (
            productId,
            newQuantity
        ) => {

            if (newQuantity < 1) {
                return
            }

            try {

                setError('')

                setUpdatingProductId(
                    productId
                )

                const updatedCart =
                    await updateCartItemQuantity(
                        productId,
                        newQuantity
                    )

                setCart(updatedCart)
            }
            catch (err) {

                console.error(
                    'Failed to update quantity:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Unable to update quantity.'
                )
            }
            finally {

                setUpdatingProductId(null)
            }
        }

    // ========================================================
    // REMOVE ITEM
    // ========================================================

    const handleRemove =
        async (productId) => {

            try {

                setError('')

                setUpdatingProductId(
                    productId
                )

                const updatedCart =
                    await removeItemFromCart(
                        productId
                    )

                setCart(updatedCart)
            }
            catch (err) {

                console.error(
                    'Failed to remove product:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Unable to remove product.'
                )
            }
            finally {

                setUpdatingProductId(null)
            }
        }

    // ========================================================
    // CLEAR CART
    // ========================================================

    const handleClear =
        async () => {

            try {

                setError('')
                setClearing(true)

                const updatedCart =
                    await clearCart()

                setCart(updatedCart)
            }
            catch (err) {

                console.error(
                    'Failed to clear cart:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Unable to clear cart.'
                )
            }
            finally {

                setClearing(false)
            }
        }

    // ========================================================
    // LOAD CART
    // ========================================================

    useEffect(() => {

        loadCart()

    }, [])

    // ========================================================
    // PRICE FORMATTER
    // ========================================================

    const formatPrice =
        (price) =>
            Number(
                price ?? 0
            ).toLocaleString(
                'en-IN',
                {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                }
            )

    // ========================================================
    // LOADING
    // ========================================================

    if (loading) {

        return (

            <main className="cart-page">

                <div className="cart-container">

                    <div className="cart-loading">

                        <div className="cart-spinner" />

                        <h2>
                            Loading your cart...
                        </h2>

                    </div>

                </div>

            </main>
        )
    }

    const items =
        cart?.items ?? []

    const totalItems =
        items.reduce(
            (total, item) =>
                total + item.quantity,
            0
        )

    // ========================================================
    // MAIN UI
    // ========================================================

    return (

        <main className="cart-page">

            <div className="cart-container">

                {/* ==============================================
                    HEADER
                   ============================================== */}

                <header className="cart-header">

                    <div>

                        <span className="cart-eyebrow">
                            Your order
                        </span>

                        <h1>
                            Shopping Cart
                        </h1>

                        <p>
                            Review your products and quantities
                            before checkout.
                        </p>

                    </div>

                    {items.length > 0 && (

                        <span className="cart-item-count">
                            {totalItems}
                            {' '}
                            {totalItems === 1
                                ? 'item'
                                : 'items'}
                        </span>

                    )}

                </header>

                {/* ==============================================
                    ERROR
                   ============================================== */}

                {error && (

                    <div
                        className="cart-alert"
                        role="alert"
                    >
                        {error}
                    </div>

                )}

                {/* ==============================================
                    EMPTY CART
                   ============================================== */}

                {items.length === 0 ? (

                    <section className="empty-cart">

                        <div className="empty-cart-icon">
                            🛒
                        </div>

                        <h2>
                            Your cart is empty
                        </h2>

                        <p>
                            You haven't added any products
                            to your cart yet.
                        </p>

                        <button
                            type="button"
                            className="continue-shopping-button"
                            onClick={() =>
                                navigate('/products')
                            }
                        >
                            Browse Products
                        </button>

                    </section>

                ) : (

                    <div className="cart-layout">

                        {/* ======================================
                            ITEMS
                           ====================================== */}

                        <section className="cart-items-section">

                            <div className="cart-section-heading">

                                <h2>
                                    Cart Items
                                </h2>

                                <span>
                                    {items.length}
                                    {' '}
                                    {items.length === 1
                                        ? 'product'
                                        : 'products'}
                                </span>

                            </div>

                            <div className="cart-items-list">

                                {items.map((item) => {

                                    const isUpdating =
                                        updatingProductId ===
                                        item.productId

                                    return (

                                        <article
                                            className="cart-item"
                                            key={item.id}
                                        >

                                            {/* ==================
                                                PRODUCT ICON
                                               ================== */}

                                            <div className="cart-product-icon">

                                                {item.productName
                                                    ?.charAt(0)
                                                    .toUpperCase()
                                                    || 'P'}

                                            </div>

                                            {/* ==================
                                                INFORMATION
                                               ================== */}

                                            <div className="cart-product-info">

                                                <span className="cart-product-label">
                                                    Product
                                                </span>

                                                <h3>
                                                    {item.productName}
                                                </h3>

                                                <div className="cart-unit-price">

                                                    <span>
                                                        Unit Price
                                                    </span>

                                                    <strong>
                                                        ₹{formatPrice(
                                                            item.unitPrice
                                                        )}
                                                    </strong>

                                                </div>

                                            </div>

                                            {/* ==================
                                                QUANTITY
                                               ================== */}

                                            <div className="cart-quantity-area">

                                                <span className="cart-control-label">
                                                    Quantity
                                                </span>

                                                <div className="quantity-control">

                                                    <button
                                                        type="button"
                                                        aria-label="Decrease quantity"
                                                        disabled={
                                                            item.quantity <= 1 ||
                                                            isUpdating
                                                        }
                                                        onClick={() =>
                                                            handleQuantityChange(
                                                                item.productId,
                                                                item.quantity - 1
                                                            )
                                                        }
                                                    >
                                                        −
                                                    </button>

                                                    <span>
                                                        {isUpdating
                                                            ? '...'
                                                            : item.quantity}
                                                    </span>

                                                    <button
                                                        type="button"
                                                        aria-label="Increase quantity"
                                                        disabled={
                                                            isUpdating
                                                        }
                                                        onClick={() =>
                                                            handleQuantityChange(
                                                                item.productId,
                                                                item.quantity + 1
                                                            )
                                                        }
                                                    >
                                                        +
                                                    </button>

                                                </div>

                                            </div>

                                            {/* ==================
                                                SUBTOTAL
                                               ================== */}

                                            <div className="cart-subtotal">

                                                <span>
                                                    Subtotal
                                                </span>

                                                <strong>
                                                    ₹{formatPrice(
                                                        item.totalPrice
                                                    )}
                                                </strong>

                                            </div>

                                            {/* ==================
                                                REMOVE
                                               ================== */}

                                            <button
                                                type="button"
                                                className="cart-remove-button"
                                                disabled={
                                                    isUpdating
                                                }
                                                onClick={() =>
                                                    handleRemove(
                                                        item.productId
                                                    )
                                                }
                                            >
                                                {isUpdating
                                                    ? 'Please wait...'
                                                    : 'Remove'}
                                            </button>

                                        </article>
                                    )
                                })}

                            </div>

                            <button
                                type="button"
                                className="continue-shopping-link"
                                onClick={() =>
                                    navigate('/products')
                                }
                            >
                                ← Continue Shopping
                            </button>

                        </section>

                        {/* ======================================
                            ORDER SUMMARY
                           ====================================== */}

                        <aside className="cart-summary">

                            <div className="summary-heading">

                                <span>
                                    Order Summary
                                </span>

                                <h2>
                                    Cart Total
                                </h2>

                            </div>

                            <div className="summary-row">

                                <span>
                                    Products
                                </span>

                                <strong>
                                    {items.length}
                                </strong>

                            </div>

                            <div className="summary-row">

                                <span>
                                    Total Quantity
                                </span>

                                <strong>
                                    {totalItems}
                                </strong>

                            </div>

                            <div className="summary-divider" />

                            <div className="summary-total">

                                <span>
                                    Total
                                </span>

                                <strong>
                                    ₹{formatPrice(
                                        cart?.totalAmount
                                    )}
                                </strong>

                            </div>

                            <button
                                type="button"
                                className="checkout-button"
                                onClick={() =>
                                    navigate('/checkout')
                                }
                            >
                                Proceed to Checkout
                            </button>

                            <button
                                type="button"
                                className="clear-cart-button"
                                disabled={clearing}
                                onClick={handleClear}
                            >
                                {clearing
                                    ? 'Clearing...'
                                    : 'Clear Cart'}
                            </button>

                            <p className="summary-note">
                                Final order details will be
                                confirmed during checkout.
                            </p>

                        </aside>

                    </div>

                )}

            </div>

        </main>
    )
}

export default CartPage