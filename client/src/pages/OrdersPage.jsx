import {
    useEffect,
    useMemo,
    useState
} from 'react'

import {
    useNavigate
} from 'react-router-dom'

import {
    getOrders
} from '../services/orderService'

import './OrdersPage.css'

// ============================================================
// ORDERS PAGE
// ============================================================

function OrdersPage() {

    const navigate =
        useNavigate()

    const [orders, setOrders] =
        useState([])

    const [loading, setLoading] =
        useState(true)

    const [error, setError] =
        useState('')

    const [search, setSearch] =
        useState('')

    // ========================================================
    // LOAD ORDERS
    // ========================================================

    const loadOrders = async () => {

        try {

            setLoading(true)
            setError('')

            const data =
                await getOrders()

            setOrders(
                Array.isArray(data)
                    ? data
                    : []
            )
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

    // ========================================================
    // HELPERS
    // ========================================================

    const formatPrice =
        (value) =>
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
        (status) => {

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
                    return String(
                        status ?? 'Unknown'
                    )
            }
        }

    const getOrderStatusClass =
        (status) => {

            switch (status) {

                case 5:
                    return 'success'

                case 6:
                    return 'danger'

                case 4:
                    return 'info'

                default:
                    return 'warning'
            }
        }

    const getPaymentStatusClass =
        (status) => {

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

    // ========================================================
    // SEARCH
    // ========================================================

    const filteredOrders =
        useMemo(() => {

            const value =
                search
                    .trim()
                    .toLowerCase()

            if (!value) {
                return orders
            }

            return orders.filter(
                (order) => {

                    const searchable = `
                        ${order.orderNumber ?? ''}
                        ${getOrderStatusLabel(order.status)}
                        ${getPaymentStatusLabel(order.paymentStatus)}
                    `.toLowerCase()

                    return searchable.includes(
                        value
                    )
                }
            )

        }, [orders, search])

    // ========================================================
    // LOADING
    // ========================================================

    if (loading) {

        return (

            <main className="orders-page">

                <div className="orders-container">

                    <div className="orders-loading">

                        <div className="orders-spinner" />

                        <h2>
                            Loading your orders...
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

        <main className="orders-page">

            <div className="orders-container">

                {/* ==============================================
                    HEADER
                   ============================================== */}

                <header className="orders-header">

                    <div>

                        <span className="orders-eyebrow">
                            Order history
                        </span>

                        <h1>
                            My Orders
                        </h1>

                        <p>
                            View your previous orders,
                            payment status and delivery progress.
                        </p>

                    </div>

                    <div className="orders-count">

                        <strong>
                            {orders.length}
                        </strong>

                        <span>
                            {
                                orders.length === 1
                                    ? 'Order'
                                    : 'Orders'
                            }
                        </span>

                    </div>

                </header>

                {/* ==============================================
                    ERROR
                   ============================================== */}

                {error && (

                    <div className="orders-alert">
                        {error}
                    </div>

                )}

                {/* ==============================================
                    EMPTY
                   ============================================== */}

                {orders.length === 0 ? (

                    <section className="orders-empty">

                        <div className="orders-empty-icon">
                            📦
                        </div>

                        <h2>
                            No orders yet
                        </h2>

                        <p>
                            You haven't placed any orders yet.
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

                ) : (

                    <>

                        {/* ======================================
                            SEARCH
                           ====================================== */}

                        <section className="orders-toolbar">

                            <div className="orders-search">

                                <input
                                    type="search"
                                    value={search}
                                    placeholder="Search by order number or status..."
                                    onChange={
                                        (event) =>
                                            setSearch(
                                                event.target.value
                                            )
                                    }
                                />

                                {search && (

                                    <button
                                        type="button"
                                        onClick={() =>
                                            setSearch('')
                                        }
                                    >
                                        ×
                                    </button>

                                )}

                            </div>

                            <span className="orders-result-count">

                                Showing{' '}

                                <strong>
                                    {
                                        filteredOrders.length
                                    }
                                </strong>

                                {' '}of{' '}

                                {orders.length}

                            </span>

                        </section>

                        {/* ======================================
                            ORDERS LIST
                           ====================================== */}

                        {filteredOrders.length === 0 ? (

                            <section className="orders-empty small">

                                <h2>
                                    No matching orders
                                </h2>

                                <p>
                                    Try a different search term.
                                </p>

                            </section>

                        ) : (

                            <section className="orders-list">

                                {filteredOrders.map(
                                    (order) => (

                                        <article
                                            className="order-card"
                                            key={order.id}
                                        >

                                            <div className="order-card-top">

                                                <div>

                                                    <span className="order-label">
                                                        Order Number
                                                    </span>

                                                    <h2>
                                                        {
                                                            order.orderNumber
                                                        }
                                                    </h2>

                                                </div>

                                                <button
                                                    type="button"
                                                    className="order-view-button"
                                                    onClick={() =>
                                                        navigate(
                                                            `/order-success/${order.id}`
                                                        )
                                                    }
                                                >
                                                    View Details
                                                </button>

                                            </div>

                                            <div className="order-card-grid">

                                                <div className="order-info-block">

                                                    <span>
                                                        Total
                                                    </span>

                                                    <strong>
                                                        ₹{formatPrice(
                                                            order.totalAmount
                                                        )}
                                                    </strong>

                                                </div>

                                                <div className="order-info-block">

                                                    <span>
                                                        Order Status
                                                    </span>

                                                    <strong
                                                        className={
                                                            `order-list-badge ${getOrderStatusClass(
                                                                order.status
                                                            )
                                                            }`
                                                        }
                                                    >
                                                        {
                                                            getOrderStatusLabel(
                                                                order.status
                                                            )
                                                        }
                                                    </strong>

                                                </div>

                                                <div className="order-info-block">

                                                    <span>
                                                        Payment
                                                    </span>

                                                    <strong
                                                        className={
                                                            `order-list-badge ${getPaymentStatusClass(
                                                                order.paymentStatus
                                                            )
                                                            }`
                                                        }
                                                    >
                                                        {
                                                            getPaymentStatusLabel(
                                                                order.paymentStatus
                                                            )
                                                        }
                                                    </strong>

                                                </div>

                                                <div className="order-info-block">

                                                    <span>
                                                        Created
                                                    </span>

                                                    <strong>
                                                        {
                                                            new Date(
                                                                order.createdAt
                                                            ).toLocaleString()
                                                        }
                                                    </strong>

                                                </div>

                                            </div>

                                        </article>

                                    )
                                )}

                            </section>

                        )}

                    </>

                )}

            </div>

        </main>
    )
}

export default OrdersPage