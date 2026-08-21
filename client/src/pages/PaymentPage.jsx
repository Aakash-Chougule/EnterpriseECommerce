import {
    useEffect,
    useState
} from 'react'

import {
    useNavigate,
    useParams
} from 'react-router-dom'

import {
    createRazorpayOrder,
    getPaymentByOrderId,
    markPaymentFailed,
    verifyRazorpayPayment
} from '../services/paymentService'

import './PaymentPage.css'

// ============================================================
// RAZORPAY SCRIPT
// ============================================================

function loadRazorpayScript() {

    return new Promise(
        (resolve) => {

            if (window.Razorpay) {
                resolve(true)
                return
            }

            const script =
                document.createElement(
                    'script'
                )

            script.src =
                'https://checkout.razorpay.com/v1/checkout.js'

            script.onload =
                () => resolve(true)

            script.onerror =
                () => resolve(false)

            document.body.appendChild(
                script
            )
        }
    )
}

// ============================================================
// PAYMENT PAGE
// ============================================================

function PaymentPage() {

    const { orderId } =
        useParams()

    const navigate =
        useNavigate()

    const [payment, setPayment] =
        useState(null)

    const [loading, setLoading] =
        useState(true)

    const [processing, setProcessing] =
        useState(false)

    const [error, setError] =
        useState('')

    // ========================================================
    // LOAD PAYMENT
    // ========================================================

    const loadPayment =
        async () => {

            try {

                setLoading(true)
                setError('')

                const data =
                    await getPaymentByOrderId(
                        orderId
                    )

                setPayment(
                    data
                )

                // COD should never reach Razorpay page.
                if (
                    data.paymentMethod
                        ?.toUpperCase() ===
                    'COD'
                ) {
                    navigate(
                        `/order-success/${orderId}`,
                        {
                            replace: true
                        }
                    )
                }
            }
            catch (err) {

                console.error(
                    'Failed to load payment:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Unable to load payment.'
                )
            }
            finally {

                setLoading(false)
            }
        }

    useEffect(() => {

        loadPayment()

    }, [orderId])

    // ========================================================
    // PAY WITH RAZORPAY
    // ========================================================

    const handlePayNow =
        async () => {

            if (!payment) {
                return
            }

            try {

                setProcessing(true)
                setError('')

                // --------------------------------------------
                // Load Razorpay Checkout SDK
                // --------------------------------------------

                const loaded =
                    await loadRazorpayScript()

                if (!loaded) {

                    throw new Error(
                        'Unable to load Razorpay Checkout.'
                    )
                }

                // --------------------------------------------
                // Create gateway order from backend
                // --------------------------------------------

                const razorpayOrder =
                    await createRazorpayOrder(
                        payment.id
                    )

                // --------------------------------------------
                // Checkout configuration
                // --------------------------------------------

                const options = {

                    key:
                        razorpayOrder.keyId,

                    amount:
                        razorpayOrder.amount,

                    currency:
                        razorpayOrder.currency,

                    name:
                        'Enterprise E-Commerce',

                    description:
                        `Payment for ${razorpayOrder.orderNumber}`,

                    order_id:
                        razorpayOrder.razorpayOrderId,

                    // ========================================
                    // SUCCESS CALLBACK
                    // ========================================

                    handler:
                        async function (
                            response
                        ) {

                            try {

                                setProcessing(true)

                                await verifyRazorpayPayment(
                                    payment.id,
                                    response.razorpay_payment_id,
                                    response.razorpay_order_id,
                                    response.razorpay_signature
                                )

                                navigate(
                                    `/order-success/${orderId}`
                                )
                            }
                            catch (verifyError) {

                                console.error(
                                    'Razorpay verification failed:',
                                    verifyError
                                )

                                setError(
                                    verifyError
                                        .response
                                        ?.data
                                        ?.message ||
                                    'Payment verification failed.'
                                )

                                setProcessing(false)
                            }
                        },

                    prefill: {

                        name:
                            razorpayOrder.customerName,

                        email:
                            razorpayOrder.customerEmail,

                        contact:
                            razorpayOrder.customerPhone ||
                            ''
                    },

                    notes: {

                        internalPaymentId:
                            payment.id,

                        internalOrderId:
                            orderId
                    },

                    theme: {
                        color:
                            '#2563eb'
                    },

                    modal: {

                        ondismiss:
                            function () {

                                setProcessing(
                                    false
                                )
                            }
                    }
                }

                const razorpay =
                    new window.Razorpay(
                        options
                    )

                // ============================================
                // PAYMENT FAILURE EVENT
                // ============================================

                razorpay.on(
                    'payment.failed',
                    async function (
                        response
                    ) {

                        console.error(
                            'Razorpay payment failed:',
                            response.error
                        )

                        try {

                            await markPaymentFailed(
                                payment.id,
                                response
                                    .error
                                    ?.description ||
                                'Razorpay payment failed.'
                            )
                        }
                        catch (failureUpdateError) {

                            console.error(
                                'Unable to save payment failure:',
                                failureUpdateError
                            )
                        }

                        setError(
                            response
                                .error
                                ?.description ||
                            'Payment failed.'
                        )

                        setProcessing(
                            false
                        )
                    }
                )

                // ============================================
                // OPEN CHECKOUT
                // ============================================

                razorpay.open()
            }
            catch (err) {

                console.error(
                    'Unable to start Razorpay:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    err.message ||
                    'Unable to start payment.'
                )

                setProcessing(false)
            }
        }

    // ========================================================
    // PRICE
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

    // ========================================================
    // LOADING
    // ========================================================

    if (loading) {

        return (

            <main className="payment-page">

                <div className="payment-container">

                    <div className="payment-loading">

                        <div className="payment-spinner" />

                        <h2>
                            Loading payment...
                        </h2>

                    </div>

                </div>

            </main>
        )
    }

    // ========================================================
    // UI
    // ========================================================

    return (

        <main className="payment-page">

            <div className="payment-container">

                <header className="payment-header">

                    <span className="payment-eyebrow">
                        Secure Payment
                    </span>

                    <h1>
                        Complete Payment
                    </h1>

                    <p>
                        Your payment is securely
                        processed by Razorpay.
                    </p>

                </header>

                {error && (

                    <div
                        className="payment-alert"
                        role="alert"
                    >
                        {error}
                    </div>

                )}

                {!payment ? (

                    <section className="payment-empty">

                        <h2>
                            Payment not found
                        </h2>

                        <button
                            type="button"
                            onClick={() =>
                                navigate('/orders')
                            }
                        >
                            View My Orders
                        </button>

                    </section>

                ) : (

                    <div className="payment-layout">

                        <section className="payment-card">

                            <div className="payment-card-header">

                                <div className="payment-icon">
                                    ₹
                                </div>

                                <div>

                                    <span>
                                        Amount to Pay
                                    </span>

                                    <h2>
                                        ₹{formatPrice(
                                            payment.amount
                                        )}
                                    </h2>

                                </div>

                            </div>

                            <div className="payment-details">

                                <div className="payment-detail-row">

                                    <span>
                                        Payment Method
                                    </span>

                                    <strong>
                                        {
                                            payment
                                                .paymentMethod
                                        }
                                    </strong>

                                </div>

                                <div className="payment-detail-row">

                                    <span>
                                        Payment ID
                                    </span>

                                    <strong className="payment-code">
                                        {payment.id}
                                    </strong>

                                </div>

                                <div className="payment-detail-row">

                                    <span>
                                        Order ID
                                    </span>

                                    <strong className="payment-code">
                                        {payment.orderId}
                                    </strong>

                                </div>

                            </div>

                        </section>

                        <aside className="payment-actions-card">

                            <span className="payment-actions-label">
                                Razorpay
                            </span>

                            <h2>
                                Pay Securely
                            </h2>

                            <p>
                                Pay using UPI, card,
                                net banking or another
                                payment option supported
                                by Razorpay.
                            </p>

                            <button
                                type="button"
                                className="payment-success-button"
                                disabled={processing}
                                onClick={handlePayNow}
                            >
                                {
                                    processing
                                        ? 'Please wait...'
                                        : `Pay ₹${formatPrice(
                                            payment.amount
                                        )}`
                                }
                            </button>

                            <div className="payment-security-note">

                                <span>
                                    🔒
                                </span>

                                <p>
                                    Payment details are
                                    handled securely by
                                    Razorpay.
                                </p>

                            </div>

                        </aside>

                    </div>

                )}

            </div>

        </main>
    )
}

export default PaymentPage