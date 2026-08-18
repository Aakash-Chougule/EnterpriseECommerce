import {
    useEffect,
    useMemo,
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

import './AdminOrdersPage.css'

// ============================================================
// ENUM LABELS
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

    return (
        orderStatusLabels[status] ??
        'Unknown'
    )
}

function getPaymentStatusLabel(status) {

    return (
        paymentStatusLabels[status] ??
        'Unknown'
    )
}

// ============================================================
// ADMIN ORDERS PAGE
// ============================================================

function AdminOrdersPage() {

    const [orders, setOrders] =
        useState([])

    const [loading, setLoading] =
        useState(true)

    const [
        processingOrderId,
        setProcessingOrderId
    ] = useState(null)

    const [error, setError] =
        useState('')

    const [message, setMessage] =
        useState('')

    const [search, setSearch] =
        useState('')

    const [
        statusFilter,
        setStatusFilter
    ] = useState('all')

    const [
        paymentFilter,
        setPaymentFilter
    ] = useState('all')

    // ========================================================
    // LOAD ORDERS
    // ========================================================

    const loadOrders = async () => {

        try {

            setLoading(true)
            setError('')

            const data =
                await getAdminOrders()

            setOrders(
                Array.isArray(data)
                    ? data
                    : []
            )
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

    // ========================================================
    // STATUS CHANGE
    // ========================================================

    const handleStatusChange =
        async (
            orderId,
            operation,
            successMessage
        ) => {

            try {

                setProcessingOrderId(
                    orderId
                )

                setError('')
                setMessage('')

                await operation(
                    orderId
                )

                setMessage(
                    successMessage
                )

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

                setProcessingOrderId(
                    null
                )
            }
        }

    // ========================================================
    // INITIAL LOAD
    // ========================================================

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

    const getOrderStatusClass =
        (status) => {

            switch (status) {

                case 1:
                    return 'pending'

                case 2:
                    return 'confirmed'

                case 3:
                    return 'processing'

                case 4:
                    return 'shipped'

                case 5:
                    return 'delivered'

                case 6:
                    return 'cancelled'

                default:
                    return ''
            }
        }

    const getPaymentStatusClass =
        (status) => {

            switch (status) {

                case 1:
                    return 'pending'

                case 2:
                    return 'success'

                case 3:
                    return 'failed'

                case 4:
                    return 'refunded'

                default:
                    return ''
            }
        }

    // ========================================================
    // SUMMARY COUNTS
    // ========================================================

    const pendingCount =
        orders.filter(
            order =>
                order.status === 1
        ).length

    const processingCount =
        orders.filter(
            order =>
                order.status === 3
        ).length

    const shippedCount =
        orders.filter(
            order =>
                order.status === 4
        ).length

    const deliveredCount =
        orders.filter(
            order =>
                order.status === 5
        ).length

    // ========================================================
    // FILTERING
    // ========================================================

    const filteredOrders =
        useMemo(() => {

            const normalizedSearch =
                search
                    .trim()
                    .toLowerCase()

            return orders.filter(
                order => {

                    const matchesSearch =
                        !normalizedSearch ||
                        `
                            ${order.orderNumber ?? ''}
                            ${order.id ?? ''}
                            ${order.userId ?? ''}
                            ${order.customerName ?? ''}
                            ${order.customerEmail ?? ''}
                            ${order.customerPhoneNumber ?? ''}
                            ${order.shippingAddress ?? ''}
                            ${getOrderStatusLabel(order.status)}
                            ${getPaymentStatusLabel(order.paymentStatus)}
                        `
                            .toLowerCase()
                            .includes(
                                normalizedSearch
                            )

                    const matchesStatus =
                        statusFilter === 'all' ||
                        String(order.status) ===
                        statusFilter

                    const matchesPayment =
                        paymentFilter === 'all' ||
                        String(
                            order.paymentStatus
                        ) ===
                        paymentFilter

                    return (
                        matchesSearch &&
                        matchesStatus &&
                        matchesPayment
                    )
                }
            )

        }, [
            orders,
            search,
            statusFilter,
            paymentFilter
        ])

    // ========================================================
    // LOADING
    // ========================================================

    if (loading) {

        return (

            <main className="admin-orders-page">

                <div className="admin-orders-container">

                    <div className="admin-orders-loading">

                        <div className="admin-orders-spinner" />

                        <h2>
                            Loading orders...
                        </h2>

                    </div>

                </div>

            </main>
        )
    }

    // ========================================================
    // MAIN UI
    // ========================================================

    return (

        <main className="admin-orders-page">

            <div className="admin-orders-container">

                {/* ==============================================
                    HEADER
                   ============================================== */}

                <header className="admin-orders-header">

                    <div>

                        <span className="admin-orders-eyebrow">
                            Order Management
                        </span>

                        <h1>
                            Admin Orders
                        </h1>

                        <p>
                            Review customer details,
                            payment status and order progress
                            before processing each order.
                        </p>

                    </div>

                    <div className="admin-orders-count">

                        <strong>
                            {orders.length}
                        </strong>

                        <span>
                            Orders
                        </span>

                    </div>

                </header>

                {/* ==============================================
                    ALERTS
                   ============================================== */}

                {error && (

                    <div className="admin-orders-alert error">

                        <span>
                            !
                        </span>

                        {error}

                    </div>

                )}

                {message && (

                    <div className="admin-orders-alert success">

                        <span>
                            ✓
                        </span>

                        {message}

                    </div>

                )}

                {/* ==============================================
                    SUMMARY
                   ============================================== */}

                <section className="admin-order-stats">

                    <div className="admin-order-stat-card">

                        <span>
                            Total Orders
                        </span>

                        <strong>
                            {orders.length}
                        </strong>

                    </div>

                    <div className="admin-order-stat-card warning">

                        <span>
                            Pending
                        </span>

                        <strong>
                            {pendingCount}
                        </strong>

                    </div>

                    <div className="admin-order-stat-card info">

                        <span>
                            Processing
                        </span>

                        <strong>
                            {processingCount}
                        </strong>

                    </div>

                    <div className="admin-order-stat-card shipped">

                        <span>
                            Shipped
                        </span>

                        <strong>
                            {shippedCount}
                        </strong>

                    </div>

                    <div className="admin-order-stat-card success">

                        <span>
                            Delivered
                        </span>

                        <strong>
                            {deliveredCount}
                        </strong>

                    </div>

                </section>

                {/* ==============================================
                    FILTERS
                   ============================================== */}

                <section className="admin-orders-toolbar">

                    <div className="admin-orders-search">

                        <input
                            type="search"
                            placeholder="Search customer, email, phone, order number or address..."
                            value={search}
                            onChange={
                                event =>
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

                    <select
                        value={statusFilter}
                        onChange={
                            event =>
                                setStatusFilter(
                                    event.target.value
                                )
                        }
                    >

                        <option value="all">
                            All Order Status
                        </option>

                        <option value="1">
                            Pending
                        </option>

                        <option value="2">
                            Confirmed
                        </option>

                        <option value="3">
                            Processing
                        </option>

                        <option value="4">
                            Shipped
                        </option>

                        <option value="5">
                            Delivered
                        </option>

                        <option value="6">
                            Cancelled
                        </option>

                    </select>

                    <select
                        value={paymentFilter}
                        onChange={
                            event =>
                                setPaymentFilter(
                                    event.target.value
                                )
                        }
                    >

                        <option value="all">
                            All Payment Status
                        </option>

                        <option value="1">
                            Pending
                        </option>

                        <option value="2">
                            Success
                        </option>

                        <option value="3">
                            Failed
                        </option>

                        <option value="4">
                            Refunded
                        </option>

                    </select>

                </section>

                <div className="admin-orders-result-count">

                    Showing{' '}

                    <strong>
                        {filteredOrders.length}
                    </strong>

                    {' '}of{' '}

                    {orders.length}

                    {' '}orders

                </div>

                {/* ==============================================
                    ORDER LIST
                   ============================================== */}

                {filteredOrders.length === 0 ? (

                    <section className="admin-orders-empty">

                        <h2>
                            No orders found
                        </h2>

                        <p>
                            Try changing your search
                            or filter options.
                        </p>

                    </section>

                ) : (

                    <section className="admin-orders-list">

                        {filteredOrders.map(
                            order => {

                                const isProcessing =
                                    processingOrderId ===
                                    order.id

                                return (

                                    <article
                                        className="admin-order-card"
                                        key={order.id}
                                    >

                                        {/* ==================================
                                            ORDER HEADER
                                           ================================== */}

                                        <div className="admin-order-card-header">

                                            <div>

                                                <span className="admin-order-number-label">
                                                    Order Number
                                                </span>

                                                <h2>
                                                    {order.orderNumber}
                                                </h2>

                                                <span className="admin-order-date">

                                                    {
                                                        new Date(
                                                            order.createdAt
                                                        ).toLocaleString()
                                                    }

                                                </span>

                                            </div>

                                            <div className="admin-order-badges">

                                                <span
                                                    className={
                                                        `admin-order-status-badge ${getOrderStatusClass(
                                                            order.status
                                                        )}`
                                                    }
                                                >
                                                    {
                                                        getOrderStatusLabel(
                                                            order.status
                                                        )
                                                    }
                                                </span>

                                                <span
                                                    className={
                                                        `admin-payment-status-badge ${getPaymentStatusClass(
                                                            order.paymentStatus
                                                        )}`
                                                    }
                                                >
                                                    Payment:{' '}
                                                    {
                                                        getPaymentStatusLabel(
                                                            order.paymentStatus
                                                        )
                                                    }
                                                </span>

                                            </div>

                                        </div>

                                        {/* ==================================
                                            CUSTOMER INFORMATION
                                           ================================== */}

                                        <div className="admin-order-customer-section">

                                            <div className="admin-order-customer-heading">

                                                <span>
                                                    Customer
                                                </span>

                                                <h3>
                                                    Customer Information
                                                </h3>

                                            </div>

                                            <div className="admin-order-customer-grid">

                                                <div className="admin-order-customer-detail">

                                                    <span>
                                                        Name
                                                    </span>

                                                    <strong>
                                                        {
                                                            order.customerName ||
                                                            'Not available'
                                                        }
                                                    </strong>

                                                </div>

                                                <div className="admin-order-customer-detail">

                                                    <span>
                                                        Email
                                                    </span>

                                                    {
                                                        order.customerEmail ? (

                                                            <a
                                                                href={
                                                                    `mailto:${order.customerEmail}`
                                                                }
                                                            >
                                                                {
                                                                    order.customerEmail
                                                                }
                                                            </a>

                                                        ) : (

                                                            <strong>
                                                                Not available
                                                            </strong>

                                                        )
                                                    }

                                                </div>

                                                <div className="admin-order-customer-detail">

                                                    <span>
                                                        Phone
                                                    </span>

                                                    {
                                                        order.customerPhoneNumber ? (

                                                            <a
                                                                href={
                                                                    `tel:${order.customerPhoneNumber}`
                                                                }
                                                            >
                                                                {
                                                                    order.customerPhoneNumber
                                                                }
                                                            </a>

                                                        ) : (

                                                            <strong>
                                                                Not provided
                                                            </strong>

                                                        )
                                                    }

                                                </div>

                                                <div className="admin-order-customer-detail">

                                                    <span>
                                                        Customer ID
                                                    </span>

                                                    <strong className="admin-order-code">
                                                        {order.userId}
                                                    </strong>

                                                </div>

                                            </div>

                                        </div>

                                        {/* ==================================
                                            ORDER DETAILS
                                           ================================== */}

                                        <div className="admin-order-details-grid">

                                            <div className="admin-order-detail">

                                                <span>
                                                    Total Amount
                                                </span>

                                                <strong>
                                                    ₹{formatPrice(
                                                        order.totalAmount
                                                    )}
                                                </strong>

                                            </div>

                                            <div className="admin-order-detail">

                                                <span>
                                                    Items
                                                </span>

                                                <strong>
                                                    {
                                                        order.orderItems
                                                            ?.length ?? 0
                                                    }
                                                </strong>

                                            </div>

                                            <div className="admin-order-detail">

                                                <span>
                                                    Payment
                                                </span>

                                                <strong>
                                                    {
                                                        getPaymentStatusLabel(
                                                            order.paymentStatus
                                                        )
                                                    }
                                                </strong>

                                            </div>

                                            <div className="admin-order-detail">

                                                <span>
                                                    Order ID
                                                </span>

                                                <strong className="admin-order-code">
                                                    {order.id}
                                                </strong>

                                            </div>

                                        </div>

                                        {/* ==================================
                                            SHIPPING
                                           ================================== */}

                                        <div className="admin-order-shipping">

                                            <span>
                                                Shipping Address
                                            </span>

                                            <p>
                                                {
                                                    order.shippingAddress ||
                                                    'Shipping address not available.'
                                                }
                                            </p>

                                        </div>

                                        {/* ==================================
                                            ITEMS
                                           ================================== */}

                                        <div className="admin-order-items-section">

                                            <h3>
                                                Order Items
                                            </h3>

                                            <div className="admin-order-items">

                                                {order.orderItems?.map(
                                                    item => (

                                                        <div
                                                            className="admin-order-item"
                                                            key={item.id}
                                                        >

                                                            <div className="admin-order-item-icon">

                                                                {item.productName
                                                                    ?.charAt(0)
                                                                    .toUpperCase()
                                                                    || 'P'}

                                                            </div>

                                                            <div className="admin-order-item-info">

                                                                <strong>
                                                                    {
                                                                        item.productName
                                                                    }
                                                                </strong>

                                                                <span>
                                                                    ₹{formatPrice(
                                                                        item.unitPrice
                                                                    )}
                                                                    {' × '}
                                                                    {
                                                                        item.quantity
                                                                    }
                                                                </span>

                                                            </div>

                                                            <strong className="admin-order-item-total">

                                                                ₹{formatPrice(
                                                                    item.totalPrice
                                                                )}

                                                            </strong>

                                                        </div>

                                                    )
                                                )}

                                            </div>

                                        </div>

                                        {/* ==================================
                                            ACTIONS
                                           ================================== */}

                                        <div className="admin-order-actions">

                                            {order.status === 1 && (

                                                <button
                                                    type="button"
                                                    className="admin-order-action primary"
                                                    disabled={
                                                        isProcessing
                                                    }
                                                    onClick={() =>
                                                        handleStatusChange(
                                                            order.id,
                                                            confirmOrder,
                                                            'Order confirmed successfully.'
                                                        )
                                                    }
                                                >
                                                    {
                                                        isProcessing
                                                            ? 'Processing...'
                                                            : 'Confirm Order'
                                                    }
                                                </button>

                                            )}

                                            {order.status === 2 && (

                                                <button
                                                    type="button"
                                                    className="admin-order-action primary"
                                                    disabled={
                                                        isProcessing
                                                    }
                                                    onClick={() =>
                                                        handleStatusChange(
                                                            order.id,
                                                            startProcessingOrder,
                                                            'Order moved to processing.'
                                                        )
                                                    }
                                                >
                                                    {
                                                        isProcessing
                                                            ? 'Processing...'
                                                            : 'Start Processing'
                                                    }
                                                </button>

                                            )}

                                            {order.status === 3 && (

                                                <button
                                                    type="button"
                                                    className="admin-order-action primary"
                                                    disabled={
                                                        isProcessing
                                                    }
                                                    onClick={() =>
                                                        handleStatusChange(
                                                            order.id,
                                                            shipOrder,
                                                            'Order shipped successfully.'
                                                        )
                                                    }
                                                >
                                                    {
                                                        isProcessing
                                                            ? 'Processing...'
                                                            : 'Ship Order'
                                                    }
                                                </button>

                                            )}

                                            {order.status === 4 && (

                                                <button
                                                    type="button"
                                                    className="admin-order-action success"
                                                    disabled={
                                                        isProcessing
                                                    }
                                                    onClick={() =>
                                                        handleStatusChange(
                                                            order.id,
                                                            deliverOrder,
                                                            'Order delivered successfully.'
                                                        )
                                                    }
                                                >
                                                    {
                                                        isProcessing
                                                            ? 'Processing...'
                                                            : 'Mark Delivered'
                                                    }
                                                </button>

                                            )}

                                            {order.status !== 4 &&
                                                order.status !== 5 &&
                                                order.status !== 6 && (

                                                    <button
                                                        type="button"
                                                        className="admin-order-action danger"
                                                        disabled={
                                                            isProcessing
                                                        }
                                                        onClick={() =>
                                                            handleStatusChange(
                                                                order.id,
                                                                cancelOrder,
                                                                'Order cancelled successfully.'
                                                            )
                                                        }
                                                    >
                                                        Cancel Order
                                                    </button>

                                                )}

                                            {order.status === 5 && (

                                                <span className="admin-order-complete-message">
                                                    ✓ Order completed
                                                </span>

                                            )}

                                            {order.status === 6 && (

                                                <span className="admin-order-cancelled-message">
                                                    Order cancelled
                                                </span>

                                            )}

                                        </div>

                                    </article>

                                )
                            }
                        )}

                    </section>

                )}

            </div>

        </main>
    )
}

export default AdminOrdersPage