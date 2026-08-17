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

// ============================================================
// PAYMENT PAGE
// ============================================================
//
// This is a SIMULATED payment page.
//
// We are not integrating Razorpay/Stripe yet.
//
// Purpose:
// - Learn the complete frontend payment flow.
// - Test the existing backend payment success/failure endpoints.
// - Confirm Order.PaymentStatus also changes correctly.
// ============================================================

function PaymentPage() {

    // ----------------------------------------------------------
    // URL parameter
    // ----------------------------------------------------------
    //
    // Example URL:
    //
    // /payment/123-order-id
    //
    // React Router gives us:
    //
    // orderId = "123-order-id"
    // ----------------------------------------------------------

    const { orderId } =
        useParams()

    const navigate =
        useNavigate()

    // ----------------------------------------------------------
    // PAYMENT STATE
    // ----------------------------------------------------------

    const [payment, setPayment] =
        useState(null)

    const [loading, setLoading] =
        useState(true)

    const [processing, setProcessing] =
        useState(false)

    const [error, setError] =
        useState('')

    // ==========================================================
    // LOAD PAYMENT
    // ==========================================================

    const loadPayment = async () => {

        try {

            setLoading(true)
            setError('')

            const data =
                await getPaymentByOrderId(
                    orderId
                )

            console.log(
                'Payment received:',
                data
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

    // ==========================================================
    // SIMULATE SUCCESSFUL PAYMENT
    // ==========================================================

    const handleSuccess = async () => {

        if (!payment) {
            return
        }

        try {

            setProcessing(true)
            setError('')

            // ------------------------------------------------------
            // Generate a temporary transaction ID.
            //
            // Later Razorpay/Stripe will give us a real transaction
            // or payment ID.
            // ------------------------------------------------------

            const transactionId =
                `TEST-${Date.now()}`

            const updatedPayment =
                await markPaymentSuccessful(
                    payment.id,
                    transactionId
                )

            console.log(
                'Payment successful:',
                updatedPayment
            )

            setPayment(updatedPayment)

            // ------------------------------------------------------
            // After success, go to the order confirmation page.
            // ------------------------------------------------------

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

    // ==========================================================
    // SIMULATE FAILED PAYMENT
    // ==========================================================

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

            console.log(
                'Payment failed:',
                updatedPayment
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

    // ==========================================================
    // INITIAL LOAD
    // ==========================================================

    useEffect(() => {

        loadPayment()

    }, [orderId])

    // ==========================================================
    // LOADING UI
    // ==========================================================

    if (loading) {

        return (
            <div>

                <h1>
                    Payment
                </h1>

                <p>
                    Loading payment...
                </p>

            </div>
        )
    }

    // ==========================================================
    // MAIN UI
    // ==========================================================

    return (
        <div>

            <h1>
                Payment
            </h1>

            {error && (
                <p>
                    {error}
                </p>
            )}

            {!payment ? (

                <p>
                    Payment information was not found.
                </p>

            ) : (

                <>

                    <p>
                        <strong>
                            Payment ID:
                        </strong>

                        {' '}

                        {payment.id}
                    </p>

                    <p>
                        <strong>
                            Order ID:
                        </strong>

                        {' '}

                        {payment.orderId}
                    </p>

                    <p>
                        <strong>
                            Payment Method:
                        </strong>

                        {' '}

                        {payment.paymentMethod}
                    </p>

                    <p>
                        <strong>
                            Amount:
                        </strong>

                        {' '}

                        ₹{Number(
                            payment.amount
                        ).toLocaleString(
                            'en-IN'
                        )}
                    </p>

                    <p>
                        <strong>
                            Status:
                        </strong>

                        {' '}

                        {payment.status}
                    </p>

                    {payment.transactionId && (
                        <p>
                            <strong>
                                Transaction ID:
                            </strong>

                            {' '}

                            {payment.transactionId}
                        </p>
                    )}

                    {payment.failureReason && (
                        <p>
                            <strong>
                                Failure Reason:
                            </strong>

                            {' '}

                            {payment.failureReason}
                        </p>
                    )}

                    <hr />

                    {/* ================================================
              PAYMENT TEST BUTTONS
             ================================================ */}

                    <button
                        type="button"
                        disabled={processing}
                        onClick={handleSuccess}
                    >
                        {
                            processing
                                ? 'Processing...'
                                : 'Simulate Successful Payment'
                        }
                    </button>

                    {' '}

                    <button
                        type="button"
                        disabled={processing}
                        onClick={handleFailure}
                    >
                        Simulate Failed Payment
                    </button>

                </>

            )}

        </div>
    )
}

export default PaymentPage