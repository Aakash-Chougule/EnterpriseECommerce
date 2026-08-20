import {
    useEffect,
    useState
} from 'react'

import {
    useNavigate,
    useParams
} from 'react-router-dom'

import {
    getOrderById
} from '../services/orderService'

import {
    getPaymentByOrderId
} from '../services/paymentService'

import './OrderSuccessPage.css'

// ============================================================
// ORDER SUCCESS PAGE
// ============================================================

function OrderSuccessPage() {

    const { orderId } =
        useParams()

    const navigate =
        useNavigate()

    const [order, setOrder] =
        useState(null)

    const [payment, setPayment] =
        useState(null)

    const [loading, setLoading] =
        useState(true)

    const [error, setError] =
        useState('')

    // ========================================================
    // LOAD DETAILS
    // ========================================================

    const loadDetails =
        async () => {

            try {

                setLoading(true)
                setError('')

                const orderData =
                    await getOrderById(
                        orderId
                    )

                setOrder(
                    orderData
                )

                try {

                    const paymentData =
                        await getPaymentByOrderId(
                            orderId
                        )

                    setPayment(
                        paymentData
                    )
                }
                catch (paymentError) {

                    console.error(
                        'Unable to load payment:',
                        paymentError
                    )

                    setPayment(
                        null
                    )
                }
            }
            catch (err) {

                console.error(
                    'Unable to load order:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Unable to load order details.'
                )
            }
            finally {

                setLoading(false)
            }
        }

    useEffect(() => {

        loadDetails()

    }, [orderId])

    // ========================================================
    // HELPERS
    // ========================================================

    const formatPrice =
        value =>
            Number(
                value ?? 0
            ).toLocaleString(
                'en-IN',
                {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                }
            )

    const getOrderStatusLabel =
        status => {

            switch (status) {

                case 1:
                    return 'Pending'

                case 2:
                    return 'Confirmed'

                case 3:
                    return 'Processing'

                case 4:
                    return 'Shipped'

                case 5:
                    return 'Delivered'

                case 6:
                    return 'Cancelled'

                default:
                    return String(
                        status ?? 'Unknown'
                    )
            }
        }

    const getPaymentStatusLabel =
        status => {

            switch (status) {

                case 1:
                    return 'Pending'

                case 2:
                    return 'Successful'

                case 3:
                    return 'Failed'

                case 4:
                    return 'Refunded'

                default:
                    return String(
                        status ?? 'Unknown'
                    )
            }
        }

    const getOrderStatusClass =
        status => {

            switch (status) {

                case 2:
                    return 'info'

                case 3:
                    return 'info'

                case 4:
                    return 'info'

                case 5:
                    return 'success'

                case 6:
                    return 'danger'

                default:
                    return 'warning'
            }
        }

    const getPaymentStatusClass =
        status => {

            switch (status) {

                case 2:
                    return 'success'

                case 3:
                    return 'danger'

                case 4:
                    return 'info'

                default:
                    return 'warning'
            }
        }

    const isCod =
        payment?.paymentMethod
            ?.toUpperCase() ===
        'COD'

    // ========================================================
    // LOADING
    // ========================================================

    if (loading) {

        return (

            <main className="order-success-page">

                <div className="order-success-container">

                    <div className="order-success-loading">

                        <div className="order-success-spinner" />

                        <h2>
                            Loading order details...
                        </h2>

                    </div>

                </div>

            </main>
        )
    }

    // ========================================================
    // ERROR
    // ========================================================

    if (error) {

        return (

            <main className="order-success-page">

                <div className="order-success-container">

                    <section className="order-success-error">

                        <div className="order-error-icon">
                            !
                        </div>

                        <h1>
                            Unable to load order
                        </h1>

                        <p>
                            {error}
                        </p>

                        <button
                            type="button"
                            onClick={() =>
                                navigate('/orders')
                            }
                        >
                            View My Orders
                        </button>

                    </section>

                </div>

            </main>
        )
    }

    // ========================================================
    // MAIN UI
    // ========================================================

    return (

        <main className="order-success-page">

            <div className="order-success-container">

                <section className="order-success-hero">

                    <div className="order-success-check">
                        ✓
                    </div>

                    <span className="order-success-eyebrow">
                        Order confirmed
                    </span>

                    <h1>
                        Order Placed Successfully
                    </h1>

                    <p>

                        {isCod
                            ? 'Your order is confirmed. Payment will be collected when your order is delivered.'
                            : 'Thank you for your order. Your order has been confirmed successfully.'}

                    </p>

                    <div className="order-number-box">

                        <span>
                            Order Number
                        </span>

                        <strong>
                            {order?.orderNumber}
                        </strong>

                    </div>

                </section>

                <div className="order-success-layout">

                    {/* ==================================================
                        LEFT
                       ================================================== */}

                    <section>

                        <div className="order-success-card">

                            <div className="order-card-heading">

                                <div>

                                    <span>
                                        Your purchase
                                    </span>

                                    <h2>
                                        Ordered Items
                                    </h2>

                                </div>

                                <strong>

                                    {
                                        order?.orderItems
                                            ?.length ?? 0
                                    }

                                    {' '}

                                    {
                                        order?.orderItems
                                            ?.length === 1
                                            ? 'item'
                                            : 'items'
                                    }

                                </strong>

                            </div>

                            <div className="ordered-items-list">

                                {order?.orderItems?.map(
                                    item => (

                                        <article
                                            className="ordered-item"
                                            key={item.id}
                                        >

                                            <div className="ordered-item-icon">

                                                {item.productName
                                                    ?.charAt(0)
                                                    .toUpperCase()
                                                    || 'P'}

                                            </div>

                                            <div className="ordered-item-info">

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

                                            <div className="ordered-item-total">

                                                <span>
                                                    Subtotal
                                                </span>

                                                <strong>
                                                    ₹{formatPrice(
                                                        item.totalPrice
                                                    )}
                                                </strong>

                                            </div>

                                        </article>

                                    )
                                )}

                            </div>

                        </div>

                        <div className="order-success-card shipping-card">

                            <div className="order-card-heading">

                                <div>

                                    <span>
                                        Delivery
                                    </span>

                                    <h2>
                                        Shipping Address
                                    </h2>

                                </div>

                            </div>

                            <p className="shipping-address-text">
                                {order?.shippingAddress}
                            </p>

                        </div>

                    </section>

                    {/* ==================================================
                        RIGHT
                       ================================================== */}

                    <aside>

                        <div className="order-success-summary">

                            <span className="summary-eyebrow">
                                Order Summary
                            </span>

                            <h2>
                                Details
                            </h2>

                            <div className="order-summary-row">

                                <span>
                                    Order Status
                                </span>

                                <span
                                    className={
                                        `order-status-badge ${getOrderStatusClass(
                                            order?.status
                                        )}`
                                    }
                                >
                                    {
                                        getOrderStatusLabel(
                                            order?.status
                                        )
                                    }
                                </span>

                            </div>

                            {!isCod && (

                                <div className="order-summary-row">

                                    <span>
                                        Payment Status
                                    </span>

                                    <span
                                        className={
                                            `order-status-badge ${getPaymentStatusClass(
                                                order?.paymentStatus
                                            )}`
                                        }
                                    >
                                        {
                                            getPaymentStatusLabel(
                                                order?.paymentStatus
                                            )
                                        }
                                    </span>

                                </div>

                            )}

                            {isCod && (

                                <div className="order-summary-row">

                                    <span>
                                        Payment
                                    </span>

                                    <span className="order-status-badge warning">
                                        Pay on Delivery
                                    </span>

                                </div>

                            )}

                            <div className="order-summary-divider" />

                            <div className="order-summary-total">

                                <span>
                                    Total Amount
                                </span>

                                <strong>
                                    ₹{formatPrice(
                                        order?.totalAmount
                                    )}
                                </strong>

                            </div>

                        </div>

                        {/* ==================================================
                            PAYMENT
                           ================================================== */}

                        {payment && (

                            <div className="order-payment-card">

                                <span className="summary-eyebrow">
                                    Payment
                                </span>

                                <h2>
                                    Payment Details
                                </h2>

                                <div className="payment-summary-row">

                                    <span>
                                        Method
                                    </span>

                                    <strong>
                                        {
                                            isCod
                                                ? 'Cash on Delivery'
                                                : payment.paymentMethod
                                        }
                                    </strong>

                                </div>

                                <div className="payment-summary-row">

                                    <span>
                                        {
                                            isCod
                                                ? 'Amount to Pay'
                                                : 'Amount'
                                        }
                                    </span>

                                    <strong>
                                        ₹{formatPrice(
                                            payment.amount
                                        )}
                                    </strong>

                                </div>

                                {isCod ? (

                                    <div className="payment-transaction">

                                        <span>
                                            Payment Instructions
                                        </span>

                                        <strong>
                                            Please pay ₹{formatPrice(
                                                payment.amount
                                            )} when your order is delivered.
                                        </strong>

                                    </div>

                                ) : (

                                    <div className="payment-summary-row">

                                        <span>
                                            Status
                                        </span>

                                        <span
                                            className={
                                                `order-status-badge ${getPaymentStatusClass(
                                                    payment.status
                                                )}`
                                            }
                                        >
                                            {
                                                getPaymentStatusLabel(
                                                    payment.status
                                                )
                                            }
                                        </span>

                                    </div>

                                )}

                                {!isCod &&
                                    payment.transactionId && (

                                        <div className="payment-transaction">

                                            <span>
                                                Transaction ID
                                            </span>

                                            <code>
                                                {
                                                    payment.transactionId
                                                }
                                            </code>

                                        </div>

                                    )}

                            </div>

                        )}

                    </aside>

                </div>

                <section className="order-success-actions">

                    <button
                        type="button"
                        className="success-secondary-button"
                        onClick={() =>
                            navigate('/products')
                        }
                    >
                        Continue Shopping
                    </button>

                    <button
                        type="button"
                        className="success-primary-button"
                        onClick={() =>
                            navigate('/orders')
                        }
                    >
                        View My Orders
                    </button>

                </section>

            </div>

        </main>
    )
}

export default OrderSuccessPage