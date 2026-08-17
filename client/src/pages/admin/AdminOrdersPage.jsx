import {
    useEffect,
    useState
} from 'react'

import {
    getAdminOrders,
    confirmOrder,
    startProcessingOrder,
    shipOrder,
    deliverOrder,
    cancelOrder
} from '../../services/adminOrderService'


// ============================================================
// ENUM LABEL HELPERS
// ============================================================
//
// ASP.NET currently returns enum values as numbers.
//
// These helpers convert backend numeric values into readable
// labels for the UI.
// ============================================================

const orderStatusLabels = {
    1: 'Pending',
    2: 'Confirmed',
    3: 'Processing',
    4: 'Shipped',
    5: 'Delivered',
    6: 'Cancelled'
}

const paymentStatusLabels = {
    1: 'Pending',
    2: 'Success',
    3: 'Failed',
    4: 'Refunded'
}

function getOrderStatusLabel(status) {
    return orderStatusLabels[status] ?? 'Unknown'
}

function getPaymentStatusLabel(status) {
    return paymentStatusLabels[status] ?? 'Unknown'
}
// ============================================================
// ADMIN ORDERS PAGE
// ============================================================
//
// Admin capabilities:
// - View all orders
// - Confirm pending orders
// - Move confirmed orders to processing
// - Ship processing orders
// - Deliver shipped orders
// - Cancel eligible orders
//
// The UI follows the same lifecycle rules as the backend.
// ============================================================

function AdminOrdersPage() {

    const [orders, setOrders] =
        useState([])

    const [loading, setLoading] =
        useState(true)

    const [processingOrderId, setProcessingOrderId] =
        useState(null)

    const [error, setError] =
        useState('')

    const [message, setMessage] =
        useState('')

    // ==========================================================
    // LOAD ORDERS
    // ==========================================================

    const loadOrders = async () => {

        try {

            setLoading(true)
            setError('')

            const data =
                await getAdminOrders()

            setOrders(data)
        }
        catch (err) {

            console.error(
                'Failed to load admin orders:',
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

    // ==========================================================
    // RUN STATUS CHANGE
    // ==========================================================
    //
    // We reuse one function for all order status operations.
    // ==========================================================

    const handleStatusChange =
        async (
            orderId,
            operation,
            successMessage
        ) => {

            try {

                setProcessingOrderId(orderId)

                setError('')
                setMessage('')

                await operation(orderId)

                setMessage(successMessage)

                await loadOrders()
            }
            catch (err) {

                console.error(
                    'Order status update failed:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Unable to update order.'
                )
            }
            finally {

                setProcessingOrderId(null)
            }
        }

    // ==========================================================
    // INITIAL LOAD
    // ==========================================================

    useEffect(() => {

        loadOrders()

    }, [])

    // ==========================================================
    // LOADING
    // ==========================================================

    if (loading) {
        return (
            <div>
                <h1>
                    Admin Order Management
                </h1>

                <p>
                    Loading orders...
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
                Admin Order Management
            </h1>

            {error && (
                <p>
                    {error}
                </p>
            )}

            {message && (
                <p>
                    {message}
                </p>
            )}

            {orders.length === 0 ? (

                <p>
                    No orders found.
                </p>

            ) : (

                orders.map((order) => {

                    const isProcessing =
                        processingOrderId ===
                        order.id

                    return (

                        <div key={order.id}>

                            <h2>
                                {order.orderNumber}
                            </h2>

                            <p>
                                <strong>
                                    Order ID:
                                </strong>

                                {' '}

                                {order.id}
                            </p>

                            <p>
                                <strong>
                                    User ID:
                                </strong>

                                {' '}

                                {order.userId}
                            </p>

                            <p>
                                <strong>
                                    Total:
                                </strong>

                                {' '}

                                ₹{Number(
                                    order.totalAmount
                                ).toLocaleString(
                                    'en-IN'
                                )}
                            </p>

                            <p>
                                <strong>
                                    Status:
                                </strong>

                                {' '}

                                {getOrderStatusLabel(
                                    order.status
                                )}
                            </p>

                            <p>
                                <strong>
                                    Payment Status:
                                </strong>

                                {' '}

                                {getPaymentStatusLabel(
                                    order.paymentStatus
                                )}
                            </p>

                            <p>
                                <strong>
                                    Shipping Address:
                                </strong>

                                {' '}

                                {order.shippingAddress}
                            </p>

                            <p>
                                <strong>
                                    Created:
                                </strong>

                                {' '}

                                {new Date(
                                    order.createdAt
                                ).toLocaleString()}
                            </p>

                            {/* =================================================
                  ORDER ITEMS
                 ================================================= */}

                            <h3>
                                Items
                            </h3>

                            {order.orderItems?.map(
                                (item) => (

                                    <div key={item.id}>

                                        <p>
                                            {item.productName}
                                        </p>

                                        <p>
                                            Quantity:
                                            {' '}
                                            {item.quantity}
                                        </p>

                                        <p>
                                            Price:
                                            {' '}
                                            ₹{Number(
                                                item.unitPrice
                                            ).toLocaleString(
                                                'en-IN'
                                            )}
                                        </p>

                                        <p>
                                            Subtotal:
                                            {' '}
                                            ₹{Number(
                                                item.totalPrice
                                            ).toLocaleString(
                                                'en-IN'
                                            )}
                                        </p>

                                    </div>

                                )
                            )}

                            {/* =================================================
                  STATUS ACTIONS
                 ================================================= */}

                            <div>

                                {/* -----------------------------------------------
                    Pending → Confirm
                   ----------------------------------------------- */}

                                {order.status === 1 && (
                                    <button
                                        type="button"
                                        disabled={isProcessing}
                                        onClick={() =>
                                            handleStatusChange(
                                                order.id,
                                                confirmOrder,
                                                'Order confirmed successfully.'
                                            )
                                        }
                                    >
                                        Confirm
                                    </button>
                                )}

                                {/* -----------------------------------------------
                    Confirmed → Processing
                   ----------------------------------------------- */}

                                {order.status === 2 && (
                                    <button
                                        type="button"
                                        disabled={isProcessing}
                                        onClick={() =>
                                            handleStatusChange(
                                                order.id,
                                                startProcessingOrder,
                                                'Order moved to processing.'
                                            )
                                        }
                                    >
                                        Start Processing
                                    </button>
                                )}

                                {/* -----------------------------------------------
                    Processing → Shipped
                   ----------------------------------------------- */}

                                {order.status === 3 && (
                                    <button
                                        type="button"
                                        disabled={isProcessing}
                                        onClick={() =>
                                            handleStatusChange(
                                                order.id,
                                                shipOrder,
                                                'Order shipped successfully.'
                                            )
                                        }
                                    >
                                        Ship
                                    </button>
                                )}

                                {/* -----------------------------------------------
                    Shipped → Delivered
                   ----------------------------------------------- */}

                                {order.status === 4 && (
                                    <button
                                        type="button"
                                        disabled={isProcessing}
                                        onClick={() =>
                                            handleStatusChange(
                                                order.id,
                                                deliverOrder,
                                                'Order delivered successfully.'
                                            )
                                        }
                                    >
                                        Deliver
                                    </button>
                                )}

                                {/* -----------------------------------------------
                    Cancel

                    Your backend prevents cancellation after
                    Shipped or Delivered.
                   ----------------------------------------------- */}

                                {order.status !== 4 &&
                                    order.status !== 5 &&
                                    order.status !== 6 && (
                                        <>
                                            {' '}

                                            <button
                                                type="button"
                                                disabled={isProcessing}
                                                onClick={() =>
                                                    handleStatusChange(
                                                        order.id,
                                                        cancelOrder,
                                                        'Order cancelled successfully.'
                                                    )
                                                }
                                            >
                                                Cancel
                                            </button>
                                        </>
                                    )}

                            </div>

                            <hr />

                        </div>
                    )
                })

            )}

        </div>
    )
}

export default AdminOrdersPage