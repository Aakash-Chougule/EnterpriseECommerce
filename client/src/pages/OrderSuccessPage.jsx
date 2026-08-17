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

    const loadDetails = async () => {

        try {
            setLoading(true)
            setError('')

            const orderData =
                await getOrderById(orderId)

            setOrder(orderData)

            // Payment might not exist if payment creation failed,
            // so we handle it separately.
            try {
                const paymentData =
                    await getPaymentByOrderId(
                        orderId
                    )

                setPayment(paymentData)
            }
            catch (paymentError) {
                console.error(
                    'Unable to load payment:',
                    paymentError
                )

                setPayment(null)
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

    if (loading) {
        return (
            <div>
                <h1>Order</h1>
                <p>Loading order details...</p>
            </div>
        )
    }

    if (error) {
        return (
            <div>
                <h1>Order</h1>
                <p>{error}</p>
            </div>
        )
    }

    return (
        <div>

            <h1>
                Order Placed Successfully
            </h1>

            <p>
                Thank you for your order.
            </p>

            <h2>
                Order Details
            </h2>

            <p>
                <strong>Order Number:</strong>
                {' '}
                {order?.orderNumber}
            </p>

            <p>
                <strong>Total:</strong>
                {' '}
                ₹{Number(
                    order?.totalAmount ?? 0
                ).toLocaleString('en-IN')}
            </p>

            <p>
                <strong>Order Status:</strong>
                {' '}
                {order?.status}
            </p>

            <p>
                <strong>Payment Status:</strong>
                {' '}
                {order?.paymentStatus}
            </p>

            <p>
                <strong>Shipping Address:</strong>
                {' '}
                {order?.shippingAddress}
            </p>

            <h2>
                Ordered Items
            </h2>

            {order?.orderItems?.map(
                (item) => (
                    <div key={item.id}>

                        <h3>
                            {item.productName}
                        </h3>

                        <p>
                            Quantity: {item.quantity}
                        </p>

                        <p>
                            Unit Price:
                            {' '}
                            ₹{Number(
                                item.unitPrice
                            ).toLocaleString('en-IN')}
                        </p>

                        <p>
                            Subtotal:
                            {' '}
                            ₹{Number(
                                item.totalPrice
                            ).toLocaleString('en-IN')}
                        </p>

                        <hr />

                    </div>
                )
            )}

            {payment && (
                <>
                    <h2>
                        Payment
                    </h2>

                    <p>
                        <strong>Method:</strong>
                        {' '}
                        {payment.paymentMethod}
                    </p>

                    <p>
                        <strong>Amount:</strong>
                        {' '}
                        ₹{Number(
                            payment.amount
                        ).toLocaleString('en-IN')}
                    </p>

                    <p>
                        <strong>Status:</strong>
                        {' '}
                        {payment.status}
                    </p>
                </>
            )}

            <button
                type="button"
                onClick={() =>
                    navigate('/products')
                }
            >
                Continue Shopping
            </button>

            {' '}

            <button
                type="button"
                onClick={() =>
                    navigate('/orders')
                }
            >
                My Orders
            </button>

        </div>
    )
}

export default OrderSuccessPage