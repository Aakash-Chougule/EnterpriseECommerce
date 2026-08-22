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
        resolve => {

            // Razorpay already available.
            if (window.Razorpay) {

                resolve(true)

                return
            }

            // Prevent duplicate script tags.
            const existingScript =
                document.querySelector(
                    'script[src="https://checkout.razorpay.com/v1/checkout.js"]'
                )

            if (existingScript) {

                existingScript.addEventListener(
                    'load',
                    () =>
                        resolve(true),
                    {
                        once: true
                    }
                )

                existingScript.addEventListener(
                    'error',
                    () =>
                        resolve(false),
                    {
                        once: true
                    }
                )

                return
            }

            const script =
                document.createElement(
                    'script'
                )

            script.src =
                'https://checkout.razorpay.com/v1/checkout.js'

            script.async =
                true

            script.onload =
                () =>
                    resolve(true)

            script.onerror =
                () =>
                    resolve(false)

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

    const {
        orderId
    } =
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
    // NORMALIZE PAYMENT METHOD
    // ========================================================

    const normalizePaymentMethod =
        value => {

            return String(
                value ?? ''
            )
                .trim()
                .toUpperCase()
                .replace(/\s+/g, '')
        }

    // ========================================================
    // PAYMENT STATUS HELPERS
    // ========================================================

    const getPaymentStatusLabel =
        status => {

            switch (
            Number(status)
            ) {

                case 1:
                    return 'Pending'

                case 2:
                    return 'Successful'

                case 3:
                    return 'Failed'

                case 4:
                    return 'Refunded'

                default:
                    return 'Unknown'
            }
        }

    // ========================================================
    // LOAD EXISTING PAYMENT
    // ========================================================
    //
    // IMPORTANT:
    //
    // We DO NOT create another internal Payment here.
    //
    // Continue Payment:
    //
    // orderId
    //   ↓
    // existing Payment
    //   ↓
    // Razorpay
    // ========================================================

    const loadPayment =
        async () => {

            if (!orderId) {

                setError(
                    'Order ID is missing.'
                )

                setLoading(
                    false
                )

                return
            }

            try {

                setLoading(
                    true
                )

                setError('')

                const data =
                    await getPaymentByOrderId(
                        orderId
                    )

                setPayment(
                    data
                )

                const paymentMethod =
                    normalizePaymentMethod(
                        data.paymentMethod
                    )

                // =============================================
                // COD SHOULD NOT USE RAZORPAY
                // =============================================

                if (
                    paymentMethod === 'COD' ||
                    paymentMethod === 'CASHONDELIVERY'
                ) {

                    navigate(
                        `/order-success/${orderId}`,
                        {
                            replace: true
                        }
                    )

                    return
                }

                // =============================================
                // ALREADY PAID
                // =============================================
                //
                // PaymentStatus.Success = 2
                // =============================================

                if (
                    Number(
                        data.status
                    ) === 2
                ) {

                    navigate(
                        `/order-success/${orderId}`,
                        {
                            replace: true
                        }
                    )

                    return
                }

                // =============================================
                // REFUNDED
                // =============================================

                if (
                    Number(
                        data.status
                    ) === 4
                ) {

                    setError(
                        'This payment has already been refunded and cannot be paid again.'
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

                setLoading(
                    false
                )
            }
        }

    // ========================================================
    // INITIAL LOAD
    // ========================================================

    useEffect(
        () => {

            loadPayment()

        },
        [orderId]
    )

    // ========================================================
    // PAY WITH RAZORPAY
    // ========================================================

    const handlePayNow =
        async () => {

            if (!payment) {

                return
            }

            if (processing) {

                return
            }

            // =============================================
            // DO NOT PAY AGAIN IF SUCCESSFUL
            // =============================================

            if (
                Number(
                    payment.status
                ) === 2
            ) {

                navigate(
                    `/order-success/${orderId}`
                )

                return
            }

            // =============================================
            // DO NOT PAY REFUNDED PAYMENT
            // =============================================

            if (
                Number(
                    payment.status
                ) === 4
            ) {

                setError(
                    'This payment has already been refunded.'
                )

                return
            }

            try {

                setProcessing(
                    true
                )

                setError('')

                // =============================================
                // LOAD RAZORPAY CHECKOUT SDK
                // =============================================

                const loaded =
                    await loadRazorpayScript()

                if (!loaded) {

                    throw new Error(
                        'Unable to load Razorpay Checkout. Please check your internet connection.'
                    )
                }

                // =============================================
                // CREATE / GET RAZORPAY ORDER
                // =============================================
                //
                // This uses:
                //
                // payment.id
                //
                // NOT:
                //
                // createPayment(...)
                //
                // So the existing payment is reused.
                // =============================================

                const razorpayOrder =
                    await createRazorpayOrder(
                        payment.id
                    )

                if (
                    !razorpayOrder
                        ?.razorpayOrderId
                ) {

                    throw new Error(
                        'Razorpay Order ID was not returned by the server.'
                    )
                }

                if (
                    !razorpayOrder
                        ?.keyId
                ) {

                    throw new Error(
                        'Razorpay Key ID was not returned by the server.'
                    )
                }

                // =============================================
                // RAZORPAY CHECKOUT OPTIONS
                // =============================================

                const options =
                {

                    // -------------------------------------
                    // PUBLIC RAZORPAY KEY
                    // -------------------------------------

                    key:
                        razorpayOrder.keyId,

                    // -------------------------------------
                    // AMOUNT
                    //
                    // Razorpay expects paise.
                    //
                    // Backend should already return the
                    // correct Razorpay amount.
                    // -------------------------------------

                    amount:
                        razorpayOrder.amount,

                    currency:
                        razorpayOrder.currency ||
                        'INR',

                    // -------------------------------------
                    // BUSINESS
                    // -------------------------------------

                    name:
                        'Enterprise E-Commerce',

                    description:
                        razorpayOrder.orderNumber
                            ? `Payment for ${razorpayOrder.orderNumber}`
                            : 'Order Payment',

                    // -------------------------------------
                    // RAZORPAY ORDER
                    // -------------------------------------

                    order_id:
                        razorpayOrder
                            .razorpayOrderId,

                    // =====================================
                    // SUCCESS CALLBACK
                    // =====================================

                    handler:
                        async function (
                            response
                        ) {

                            try {

                                setProcessing(
                                    true
                                )

                                setError('')

                                // =================================
                                // VERIFY ON BACKEND
                                // =================================
                                //
                                // Never trust the frontend alone.
                                // Backend verifies signature using
                                // Razorpay Secret.
                                // =================================

                                const verifiedPayment =
                                    await verifyRazorpayPayment(
                                        payment.id,
                                        response
                                            .razorpay_payment_id,
                                        response
                                            .razorpay_order_id,
                                        response
                                            .razorpay_signature
                                    )

                                setPayment(
                                    verifiedPayment
                                )

                                // =================================
                                // PAYMENT COMPLETE
                                // =================================

                                navigate(
                                    `/order-success/${orderId}`,
                                    {
                                        replace: true
                                    }
                                )
                            }
                            catch (
                            verifyError
                            ) {

                                console.error(
                                    'Razorpay verification failed:',
                                    verifyError
                                )

                                setError(
                                    verifyError
                                        .response
                                        ?.data
                                        ?.message ||
                                    'Payment was received but verification failed. Please do not make another payment until the payment status is checked.'
                                )

                                setProcessing(
                                    false
                                )
                            }
                        },

                    // =====================================
                    // CUSTOMER PREFILL
                    // =====================================

                    prefill:
                    {

                        name:
                            razorpayOrder
                                .customerName ||
                            '',

                        email:
                            razorpayOrder
                                .customerEmail ||
                            '',

                        contact:
                            razorpayOrder
                                .customerPhone ||
                            ''
                    },

                    // =====================================
                    // INTERNAL REFERENCES
                    // =====================================

                    notes:
                    {

                        internalPaymentId:
                            payment.id,

                        internalOrderId:
                            orderId
                    },

                    // =====================================
                    // UI
                    // =====================================

                    theme:
                    {

                        color:
                            '#2563eb'
                    },

                    // =====================================
                    // MODAL
                    // =====================================

                    modal:
                    {

                        ondismiss:
                            function () {

                                setProcessing(
                                    false
                                )
                            }
                    }
                }

                // =============================================
                // OPEN RAZORPAY CHECKOUT
                // =============================================

                const razorpay =
                    new window.Razorpay(
                        options
                    )

                // =============================================
                // PAYMENT FAILED
                // =============================================

                razorpay.on(
                    'payment.failed',
                    async function (
                        response
                    ) {

                        console.error(
                            'Razorpay payment failed:',
                            response.error
                        )

                        const reason =
                            response
                                .error
                                ?.description ||
                            'Razorpay payment failed.'

                        // =====================================
                        // STORE FAILURE
                        // =====================================

                        try {

                            const failedPayment =
                                await markPaymentFailed(
                                    payment.id,
                                    reason
                                )

                            setPayment(
                                failedPayment
                            )
                        }
                        catch (
                        failureUpdateError
                        ) {

                            console.error(
                                'Unable to save payment failure:',
                                failureUpdateError
                            )
                        }

                        setError(
                            `${reason} You can return to your orders and try again if the order is still eligible for payment.`
                        )

                        setProcessing(
                            false
                        )
                    }
                )

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

                setProcessing(
                    false
                )
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
                    minimumFractionDigits:
                        2,

                    maximumFractionDigits:
                        2
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
    // PAYMENT STATE
    // ========================================================

    const paymentStatus =
        Number(
            payment?.status
        )

    const paymentStatusLabel =
        getPaymentStatusLabel(
            paymentStatus
        )

    const canPay =
        payment &&
        paymentStatus !== 2 &&
        paymentStatus !== 4

    // ========================================================
    // UI
    // ========================================================

    return (

        <main className="payment-page">

            <div className="payment-container">

                {/* ==================================================
                    HEADER
                   ================================================== */}

                <header className="payment-header">

                    <span className="payment-eyebrow">
                        Secure Payment
                    </span>

                    <h1>
                        Complete Payment
                    </h1>

                    <p>
                        Your order already exists.
                        Complete the pending payment
                        securely through Razorpay.
                    </p>

                </header>

                {/* ==================================================
                    ERROR
                   ================================================== */}

                {error && (

                    <div
                        className="payment-alert"
                        role="alert"
                    >
                        {error}
                    </div>

                )}

                {/* ==================================================
                    PAYMENT NOT FOUND
                   ================================================== */}

                {!payment ? (

                    <section className="payment-empty">

                        <h2>
                            Payment not found
                        </h2>

                        <p>
                            We could not find a payment
                            for this order.
                        </p>

                        <button
                            type="button"
                            onClick={() =>
                                navigate(
                                    '/orders'
                                )
                            }
                        >
                            View My Orders
                        </button>

                    </section>

                ) : (

                    <div className="payment-layout">

                        {/* ==========================================
                            PAYMENT INFORMATION
                           ========================================== */}

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

                                {/* PAYMENT METHOD */}

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

                                {/* STATUS */}

                                <div className="payment-detail-row">

                                    <span>
                                        Payment Status
                                    </span>

                                    <strong>
                                        {
                                            paymentStatusLabel
                                        }
                                    </strong>

                                </div>

                                {/* PAYMENT ID */}

                                <div className="payment-detail-row">

                                    <span>
                                        Payment ID
                                    </span>

                                    <strong className="payment-code">
                                        {payment.id}
                                    </strong>

                                </div>

                                {/* ORDER ID */}

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

                        {/* ==========================================
                            RAZORPAY ACTION
                           ========================================== */}

                        <aside className="payment-actions-card">

                            <span className="payment-actions-label">
                                Razorpay
                            </span>

                            {/* ======================================
                                SUCCESS
                               ====================================== */}

                            {paymentStatus === 2 ? (

                                <>

                                    <h2>
                                        Payment Complete
                                    </h2>

                                    <p>
                                        This payment has already
                                        been completed successfully.
                                    </p>

                                    <button
                                        type="button"
                                        className="payment-success-button"
                                        onClick={() =>
                                            navigate(
                                                `/order-success/${orderId}`
                                            )
                                        }
                                    >
                                        View Order
                                    </button>

                                </>

                            ) : paymentStatus === 4 ? (

                                // =================================
                                // REFUNDED
                                // =================================

                                <>

                                    <h2>
                                        Payment Refunded
                                    </h2>

                                    <p>
                                        This payment has already
                                        been refunded and cannot
                                        be paid again.
                                    </p>

                                    <button
                                        type="button"
                                        className="payment-success-button"
                                        onClick={() =>
                                            navigate(
                                                '/orders'
                                            )
                                        }
                                    >
                                        View My Orders
                                    </button>

                                </>

                            ) : (

                                // =================================
                                // PENDING / FAILED
                                // =================================

                                <>

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
                                        disabled={
                                            processing ||
                                            !canPay
                                        }
                                        onClick={
                                            handlePayNow
                                        }
                                    >

                                        {
                                            processing
                                                ? 'Please wait...'
                                                : paymentStatus === 3
                                                    ? `Retry Payment ₹${formatPrice(
                                                        payment.amount
                                                    )}`
                                                    : `Pay ₹${formatPrice(
                                                        payment.amount
                                                    )}`
                                        }

                                    </button>

                                    {/* RETURN WITHOUT CREATING
                                        ANOTHER ORDER */}

                                    <button
                                        type="button"
                                        className="payment-back-button"
                                        disabled={
                                            processing
                                        }
                                        onClick={() =>
                                            navigate(
                                                `/order-success/${orderId}`
                                            )
                                        }
                                    >
                                        Back to Order
                                    </button>

                                </>

                            )}

                            {/* ======================================
                                SECURITY
                               ====================================== */}

                            <div className="payment-security-note">

                                <span>
                                    🔒
                                </span>

                                <p>
                                    Payment details are handled
                                    securely by Razorpay.
                                    Your Razorpay secret key is
                                    never exposed to the browser.
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