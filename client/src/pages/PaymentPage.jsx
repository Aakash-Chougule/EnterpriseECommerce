import {
    useEffect,
    useState
} from 'react'

import {
    useNavigate,
    useParams
} from 'react-router-dom'

import {
    getPaymentByOrderId,
    markPaymentSuccessful,
    markPaymentFailed
} from '../services/paymentService'

import './PaymentPage.css'

// ============================================================
// PAYMENT PAGE
// ============================================================
//
// Simulated payment page.
//
// Later we can replace this with a real payment gateway
// such as Razorpay or Stripe.
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

    const loadPayment = async () => {

        try {

            setLoading(true)
            setError('')

            const data =
                await getPaymentByOrderId(
                    orderId
                )

            setPayment(data)
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

    // ========================================================
    // SUCCESS
    // ========================================================

    const handleSuccess = async () => {

        if (!payment) {
            return
        }

        try {

            setProcessing(true)
            setError('')

            const transactionId =
                `TEST-${Date.now()}`

            const updatedPayment =
                await markPaymentSuccessful(
                    payment.id,
                    transactionId
                )

            setPayment(updatedPayment)

            navigate(
                `/order-success/${orderId}`
            )
        }
        catch (err) {

            console.error(
                'Payment success update failed:',
                err
            )

            setError(
                err.response?.data?.message ||
                'Unable to complete payment.'
            )
        }
        finally {

            setProcessing(false)
        }
    }

    // ========================================================
    // FAILURE
    // ========================================================

    const handleFailure = async () => {

        if (!payment) {
            return
        }

        try {

            setProcessing(true)
            setError('')

            const updatedPayment =
                await markPaymentFailed(
                    payment.id,
                    'Simulated payment failure from React.'
                )

            setPayment(updatedPayment)
        }
        catch (err) {

            console.error(
                'Payment failure update failed:',
                err
            )

            setError(
                err.response?.data?.message ||
                'Unable to mark payment as failed.'
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

        loadPayment()

    }, [orderId])

    // ========================================================
    // HELPERS
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

    const getPaymentStatusLabel =
        (status) => {

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
                    return String(status)
            }
        }

    const getPaymentStatusClass =
        (status) => {

            switch (status) {

                case 1:
                    return 'payment-status pending'

                case 2:
                    return 'payment-status success'

                case 3:
                    return 'payment-status failed'

                case 4:
                    return 'payment-status refunded'

                default:
                    return 'payment-status'
            }
        }

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

                        <p>
                            Please wait while we load
                            your payment information.
                        </p>

                    </div>

                </div>

            </main>
        )
    }

    // ========================================================
    // MAIN UI
    // ========================================================

    return (

        <main className="payment-page">

            <div className="payment-container">

                <header className="payment-header">

                    <span className="payment-eyebrow">
                        Complete your order
                    </span>

                    <h1>
                        Payment
                    </h1>

                    <p>
                        Review your payment details and
                        complete the simulated transaction.
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

                        <div className="payment-empty-icon">
                            !
                        </div>

                        <h2>
                            Payment not found
                        </h2>

                        <p>
                            We could not find payment
                            information for this order.
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

                ) : (

                    <div className="payment-layout">

                        {/* ======================================
                            PAYMENT DETAILS
                           ====================================== */}

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

                            <div className="payment-status-row">

                                <span>
                                    Payment Status
                                </span>

                                <span
                                    className={
                                        getPaymentStatusClass(
                                            payment.status
                                        )
                                    }
                                >
                                    {
                                        getPaymentStatusLabel(
                                            payment.status
                                        )
                                    }
                                </span>

                            </div>

                            <div className="payment-details">

                                <div className="payment-detail-row">

                                    <span>
                                        Payment Method
                                    </span>

                                    <strong>
                                        {payment.paymentMethod}
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

                                {payment.transactionId && (

                                    <div className="payment-detail-row">

                                        <span>
                                            Transaction ID
                                        </span>

                                        <strong className="payment-code">
                                            {payment.transactionId}
                                        </strong>

                                    </div>

                                )}

                                {payment.failureReason && (

                                    <div className="payment-failure-box">

                                        <strong>
                                            Payment Failure
                                        </strong>

                                        <p>
                                            {
                                                payment.failureReason
                                            }
                                        </p>

                                    </div>

                                )}

                            </div>

                        </section>

                        {/* ======================================
                            ACTIONS
                           ====================================== */}

                        <aside className="payment-actions-card">

                            <span className="payment-actions-label">
                                Demo Payment
                            </span>

                            <h2>
                                Simulate Payment
                            </h2>

                            <p>
                                This page currently simulates
                                payment gateway behavior for
                                development and testing.
                            </p>

                            <button
                                type="button"
                                className="payment-success-button"
                                disabled={processing}
                                onClick={handleSuccess}
                            >
                                {
                                    processing
                                        ? 'Processing...'
                                        : 'Simulate Successful Payment'
                                }
                            </button>

                            <button
                                type="button"
                                className="payment-failure-button"
                                disabled={processing}
                                onClick={handleFailure}
                            >
                                Simulate Failed Payment
                            </button>

                            <div className="payment-security-note">

                                <span>
                                    🔒
                                </span>

                                <p>
                                    Real gateway integration will
                                    replace these simulation controls.
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