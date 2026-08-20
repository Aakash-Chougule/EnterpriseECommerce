import {
    useEffect,
    useState
} from 'react'

import {
    useNavigate
} from 'react-router-dom'

import {
    getCart
} from '../services/cartService'

import {
    createOrder
} from '../services/orderService'

import {
    createPayment
} from '../services/paymentService'

import './CheckoutPage.css'

// ============================================================
// CHECKOUT PAGE
// ============================================================

function CheckoutPage() {

    const navigate =
        useNavigate()

    const [cart, setCart] =
        useState(null)

    const [
        shippingAddress,
        setShippingAddress
    ] = useState('')

    const [
        paymentMethod,
        setPaymentMethod
    ] = useState('UPI')

    const [loading, setLoading] =
        useState(true)

    const [processing, setProcessing] =
        useState(false)

    const [error, setError] =
        useState('')

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
                'Failed to load checkout cart:',
                err
            )

            setError(
                err.response?.data?.message ||
                'Unable to load checkout information.'
            )
        }
        finally {

            setLoading(false)
        }
    }

    // ========================================================
    // PLACE ORDER
    // ========================================================

    const handlePlaceOrder =
        async (event) => {

            event.preventDefault()

            if (!shippingAddress.trim()) {

                setError(
                    'Shipping address is required.'
                )

                return
            }

            if (!paymentMethod) {

                setError(
                    'Please select a payment method.'
                )

                return
            }

            try {

                setProcessing(true)
                setError('')

                // =============================================
                // CREATE ORDER
                // =============================================

                const order =
                    await createOrder(
                        shippingAddress.trim(),
                        paymentMethod
                    )

                console.log(
                    'Order created:',
                    order
                )

                // =============================================
                // CREATE PAYMENT RECORD
                // =============================================

                const payment =
                    await createPayment(
                        order.id,
                        paymentMethod
                    )

                console.log(
                    'Payment created:',
                    payment
                )

                // =============================================
                // CASH ON DELIVERY
                // =============================================

                if (paymentMethod === 'COD') {

                    navigate(
                        `/order-success/${order.id}`
                    )

                    return
                }

                // =============================================
                // ONLINE PAYMENT
                // =============================================

                navigate(
                    `/payment/${order.id}`
                )
            }
            catch (err) {

                console.error(
                    'Checkout failed:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Checkout failed. Please try again.'
                )
            }
            finally {

                setProcessing(false)
            }
        }

    // ========================================================
    // INITIAL LOAD
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

            <main className="checkout-page">

                <div className="checkout-container">

                    <div className="checkout-loading">

                        <div className="checkout-spinner" />

                        <h2>
                            Loading checkout...
                        </h2>

                        <p>
                            Please wait while we prepare
                            your order.
                        </p>

                    </div>

                </div>

            </main>
        )
    }

    const items =
        cart?.items ?? []

    // ========================================================
    // EMPTY CART
    // ========================================================

    if (items.length === 0) {

        return (

            <main className="checkout-page">

                <div className="checkout-container">

                    <section className="checkout-empty">

                        <div className="checkout-empty-icon">
                            🛒
                        </div>

                        <h1>
                            Your cart is empty
                        </h1>

                        <p>
                            Add some products before
                            proceeding to checkout.
                        </p>

                        <button
                            type="button"
                            onClick={() =>
                                navigate('/products')
                            }
                        >
                            Browse Products
                        </button>

                    </section>

                </div>

            </main>
        )
    }

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

        <main className="checkout-page">

            <div className="checkout-container">

                <header className="checkout-header">

                    <span className="checkout-eyebrow">
                        Complete your purchase
                    </span>

                    <h1>
                        Checkout
                    </h1>

                    <p>
                        Review your order and enter
                        your delivery information.
                    </p>

                </header>

                {error && (

                    <div
                        className="checkout-alert"
                        role="alert"
                    >
                        {error}
                    </div>

                )}

                <div className="checkout-layout">

                    <section className="checkout-form-section">

                        <div className="checkout-card">

                            <div className="checkout-section-header">

                                <div className="checkout-step">
                                    1
                                </div>

                                <div>

                                    <h2>
                                        Delivery Information
                                    </h2>

                                    <p>
                                        Enter the address where
                                        your order should be delivered.
                                    </p>

                                </div>

                            </div>

                            <form
                                onSubmit={handlePlaceOrder}
                                className="checkout-form"
                            >

                                <div className="checkout-field">

                                    <label
                                        htmlFor="shippingAddress"
                                    >
                                        Shipping Address
                                    </label>

                                    <textarea
                                        id="shippingAddress"
                                        rows="5"
                                        value={shippingAddress}
                                        placeholder="House / Flat No., Street, Area, City, State, PIN Code"
                                        onChange={
                                            event =>
                                                setShippingAddress(
                                                    event.target.value
                                                )
                                        }
                                    />

                                    <small>
                                        Enter your complete
                                        delivery address including
                                        PIN code.
                                    </small>

                                </div>

                                <div className="checkout-payment-section">

                                    <div className="checkout-section-header">

                                        <div className="checkout-step">
                                            2
                                        </div>

                                        <div>

                                            <h2>
                                                Payment Method
                                            </h2>

                                            <p>
                                                Choose how you want
                                                to pay for your order.
                                            </p>

                                        </div>

                                    </div>

                                    <div className="checkout-field">

                                        <label
                                            htmlFor="paymentMethod"
                                        >
                                            Select Payment Method
                                        </label>

                                        <select
                                            id="paymentMethod"
                                            value={paymentMethod}
                                            onChange={
                                                event =>
                                                    setPaymentMethod(
                                                        event.target.value
                                                    )
                                            }
                                        >

                                            <option value="UPI">
                                                UPI
                                            </option>

                                            <option value="Card">
                                                Debit / Credit Card
                                            </option>

                                            <option value="NetBanking">
                                                Net Banking
                                            </option>

                                            <option value="COD">
                                                Cash on Delivery
                                            </option>

                                        </select>

                                        {paymentMethod === 'COD' && (

                                            <small>
                                                You will pay the order
                                                amount when the product
                                                is delivered.
                                            </small>

                                        )}

                                    </div>

                                </div>

                                <div className="checkout-form-actions">

                                    <button
                                        type="button"
                                        className="checkout-back-button"
                                        disabled={processing}
                                        onClick={() =>
                                            navigate('/cart')
                                        }
                                    >
                                        ← Back to Cart
                                    </button>

                                    <button
                                        type="submit"
                                        className="place-order-button"
                                        disabled={processing}
                                    >
                                        {processing
                                            ? 'Placing Order...'
                                            : paymentMethod === 'COD'
                                                ? 'Place COD Order'
                                                : 'Place Order'}
                                    </button>

                                </div>

                            </form>

                        </div>

                    </section>

                    <aside className="checkout-summary">

                        <div className="checkout-summary-header">

                            <span>
                                Your Order
                            </span>

                            <h2>
                                Order Summary
                            </h2>

                        </div>

                        <div className="checkout-products">

                            {items.map(
                                item => (

                                    <div
                                        className="checkout-product"
                                        key={item.id}
                                    >

                                        <div className="checkout-product-icon">

                                            {item.productName
                                                ?.charAt(0)
                                                .toUpperCase()
                                                || 'P'}

                                        </div>

                                        <div className="checkout-product-info">

                                            <h3>
                                                {item.productName}
                                            </h3>

                                            <p>
                                                ₹{formatPrice(
                                                    item.unitPrice
                                                )}
                                                {' × '}
                                                {item.quantity}
                                            </p>

                                        </div>

                                        <strong className="checkout-product-total">

                                            ₹{formatPrice(
                                                item.totalPrice
                                            )}

                                        </strong>

                                    </div>

                                )
                            )}

                        </div>

                        <div className="checkout-summary-divider" />

                        <div className="checkout-summary-row">

                            <span>
                                Products
                            </span>

                            <strong>
                                {items.length}
                            </strong>

                        </div>

                        <div className="checkout-summary-row">

                            <span>
                                Total Quantity
                            </span>

                            <strong>
                                {totalItems}
                            </strong>

                        </div>

                        <div className="checkout-summary-divider" />

                        <div className="checkout-grand-total">

                            <span>
                                Total
                            </span>

                            <strong>
                                ₹{formatPrice(
                                    cart?.totalAmount
                                )}
                            </strong>

                        </div>

                        {paymentMethod === 'COD' && (

                            <div className="checkout-secure-note">

                                <span>
                                    💵
                                </span>

                                <p>
                                    ₹{formatPrice(
                                        cart?.totalAmount
                                    )}
                                    {' '}
                                    will be payable on delivery.
                                </p>

                            </div>

                        )}

                        {paymentMethod !== 'COD' && (

                            <div className="checkout-secure-note">

                                <span>
                                    🔒
                                </span>

                                <p>
                                    Your order information is
                                    securely processed.
                                </p>

                            </div>

                        )}

                    </aside>

                </div>

            </div>

        </main>
    )
}

export default CheckoutPage