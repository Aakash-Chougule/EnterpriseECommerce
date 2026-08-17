import {
    useEffect,
    useState
} from 'react'

import {
    useNavigate
} from 'react-router-dom'

import {
    getOrders
} from '../services/orderService'

function OrdersPage() {

    const navigate =
        useNavigate()

    const [orders, setOrders] =
        useState([])

    const [loading, setLoading] =
        useState(true)

    const [error, setError] =
        useState('')

    const loadOrders = async () => {

        try {
            setLoading(true)
            setError('')

            const data =
                await getOrders()

            setOrders(data)
        }
        catch (err) {
            console.error(
                'Failed to load orders:',
                err
            )

            setError(
                err.response?.data?.message ||
                'Unable to load orders.'
            )
        }
        finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        loadOrders()
    }, [])

    if (loading) {
        return (
            <div>
                <h1>My Orders</h1>
                <p>Loading orders...</p>
            </div>
        )
    }

    return (
        <div>

            <h1>
                My Orders
            </h1>

            {error && (
                <p>
                    {error}
                </p>
            )}

            {orders.length === 0 ? (
                <p>
                    You have not placed any orders yet.
                </p>
            ) : (
                orders.map((order) => (
                    <div key={order.id}>

                        <h2>
                            {order.orderNumber}
                        </h2>

                        <p>
                            Total:
                            {' '}
                            ₹{Number(
                                order.totalAmount
                            ).toLocaleString('en-IN')}
                        </p>

                        <p>
                            Status:
                            {' '}
                            {order.status}
                        </p>

                        <p>
                            Payment Status:
                            {' '}
                            {order.paymentStatus}
                        </p>

                        <p>
                            Created:
                            {' '}
                            {new Date(
                                order.createdAt
                            ).toLocaleString()}
                        </p>

                        <button
                            type="button"
                            onClick={() =>
                                navigate(
                                    `/order-success/${order.id}`
                                )
                            }
                        >
                            View Details
                        </button>

                        <hr />

                    </div>
                ))
            )}

        </div>
    )
}

export default OrdersPage