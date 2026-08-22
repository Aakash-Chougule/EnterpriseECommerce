import {
    useEffect,
    useState
} from 'react'

import {
    exportReportCsv,
    exportReportExcel,
    exportReportPdf,
    getReportDashboard
} from '../../services/reportService'

import './AdminReportsPage.css'

// ============================================================
// ADMIN REPORTS
// ============================================================

function AdminReportsPage() {

    const [report, setReport] =
        useState(null)

    const [loading, setLoading] =
        useState(true)

    const [exporting, setExporting] =
        useState('')

    const [error, setError] =
        useState('')

    const [fromDate, setFromDate] =
        useState('')

    const [toDate, setToDate] =
        useState('')

    // ========================================================
    // FORMAT MONEY
    // ========================================================

    const formatMoney =
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
    // DATE
    // ========================================================

    const formatDate =
        value => {

            if (!value) {

                return '-'
            }

            return new Date(
                value
            ).toLocaleString(
                'en-IN'
            )
        }

    // ========================================================
    // LOAD REPORT
    // ========================================================

    const loadReport =
        async (
            from = fromDate,
            to = toDate
        ) => {

            try {

                setLoading(
                    true
                )

                setError('')

                const data =
                    await getReportDashboard(
                        from || null,
                        to || null
                    )

                setReport(
                    data
                )
            }
            catch (err) {

                console.error(
                    'Unable to load reports:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Unable to load reports.'
                )
            }
            finally {

                setLoading(
                    false
                )
            }
        }

    useEffect(
        () => {

            loadReport(
                null,
                null
            )

        },
        []
    )

    // ========================================================
    // QUICK FILTER
    // ========================================================

    const applyQuickFilter =
        days => {

            const end =
                new Date()

            const start =
                new Date()

            start.setDate(
                end.getDate() -
                days +
                1
            )

            const from =
                start
                    .toISOString()
                    .slice(
                        0,
                        10
                    )

            const to =
                end
                    .toISOString()
                    .slice(
                        0,
                        10
                    )

            setFromDate(
                from
            )

            setToDate(
                to
            )

            loadReport(
                from,
                to
            )
        }

    // ========================================================
    // ALL TIME
    // ========================================================

    const loadAllTime =
        () => {

            setFromDate('')
            setToDate('')

            loadReport(
                null,
                null
            )
        }

    // ========================================================
    // EXPORT
    // ========================================================

    const handleExport =
        async type => {

            try {

                setExporting(
                    type
                )

                setError('')

                if (type === 'excel') {

                    await exportReportExcel(
                        fromDate || null,
                        toDate || null
                    )
                }

                if (type === 'csv') {

                    await exportReportCsv(
                        fromDate || null,
                        toDate || null
                    )
                }

                if (type === 'pdf') {

                    await exportReportPdf(
                        fromDate || null,
                        toDate || null
                    )
                }
            }
            catch (err) {

                console.error(
                    'Export failed:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Unable to export report.'
                )
            }
            finally {

                setExporting('')
            }
        }

    // ========================================================
    // INITIAL LOADING
    // ========================================================

    if (
        loading &&
        !report
    ) {

        return (

            <main className="admin-reports-page">

                <div className="admin-reports-container">

                    <div className="reports-loading">
                        Loading reports...
                    </div>

                </div>

            </main>
        )
    }

    return (

        <main className="admin-reports-page">

            <div className="admin-reports-container">

                {/* ==================================================
                    HEADER
                   ================================================== */}

                <header className="admin-reports-header">

                    <div>

                        <span className="reports-eyebrow">
                            Analytics & Reporting
                        </span>

                        <h1>
                            Business Reports
                        </h1>

                        <p>
                            Monitor sales, payments,
                            order performance and product
                            activity across the platform.
                        </p>

                    </div>

                    {/* ==============================================
                        EXPORT
                       ============================================== */}

                    <div className="reports-export-buttons">

                        <button
                            type="button"
                            className="report-export-button excel"
                            disabled={
                                Boolean(
                                    exporting
                                )
                            }
                            onClick={() =>
                                handleExport(
                                    'excel'
                                )
                            }
                        >
                            {
                                exporting === 'excel'
                                    ? 'Exporting...'
                                    : 'Export Excel'
                            }
                        </button>

                        <button
                            type="button"
                            className="report-export-button csv"
                            disabled={
                                Boolean(
                                    exporting
                                )
                            }
                            onClick={() =>
                                handleExport(
                                    'csv'
                                )
                            }
                        >
                            {
                                exporting === 'csv'
                                    ? 'Exporting...'
                                    : 'Export CSV'
                            }
                        </button>

                        <button
                            type="button"
                            className="report-export-button pdf"
                            disabled={
                                Boolean(
                                    exporting
                                )
                            }
                            onClick={() =>
                                handleExport(
                                    'pdf'
                                )
                            }
                        >
                            {
                                exporting === 'pdf'
                                    ? 'Exporting...'
                                    : 'Export PDF'
                            }
                        </button>

                    </div>

                </header>

                {/* ==================================================
                    FILTERS
                   ================================================== */}

                <section className="reports-filter-card">

                    <div className="reports-quick-filters">

                        <button
                            type="button"
                            onClick={
                                loadAllTime
                            }
                        >
                            All Time
                        </button>

                        <button
                            type="button"
                            onClick={() =>
                                applyQuickFilter(1)
                            }
                        >
                            Today
                        </button>

                        <button
                            type="button"
                            onClick={() =>
                                applyQuickFilter(7)
                            }
                        >
                            Last 7 Days
                        </button>

                        <button
                            type="button"
                            onClick={() =>
                                applyQuickFilter(30)
                            }
                        >
                            Last 30 Days
                        </button>

                    </div>

                    <div className="reports-date-filter">

                        <label>

                            From

                            <input
                                type="date"
                                value={
                                    fromDate
                                }
                                onChange={
                                    event =>
                                        setFromDate(
                                            event.target.value
                                        )
                                }
                            />

                        </label>

                        <label>

                            To

                            <input
                                type="date"
                                value={
                                    toDate
                                }
                                onChange={
                                    event =>
                                        setToDate(
                                            event.target.value
                                        )
                                }
                            />

                        </label>

                        <button
                            type="button"
                            className="reports-apply-button"
                            disabled={
                                loading
                            }
                            onClick={() =>
                                loadReport()
                            }
                        >
                            {
                                loading
                                    ? 'Loading...'
                                    : 'Apply'
                            }
                        </button>

                    </div>

                </section>

                {error && (

                    <div className="reports-error">
                        {error}
                    </div>

                )}

                {report && (

                    <>

                        {/* ==================================================
                            SUMMARY
                           ================================================== */}

                        <section className="reports-summary-grid">

                            <ReportCard
                                label="Total Revenue"
                                value={
                                    `₹${formatMoney(
                                        report.totalRevenue
                                    )}`
                                }
                                description="Successful payments"
                            />

                            <ReportCard
                                label="Average Order Value"
                                value={
                                    `₹${formatMoney(
                                        report.averageOrderValue
                                    )}`
                                }
                                description="Average order amount"
                            />

                            <ReportCard
                                label="Total Orders"
                                value={
                                    report.totalOrders
                                }
                                description="Orders in selected period"
                            />

                            <ReportCard
                                label="Successful Payments"
                                value={
                                    report.successfulPayments
                                }
                                description="Completed payments"
                            />

                        </section>

                        {/* ==================================================
                            ORDER STATUS
                           ================================================== */}

                        <section className="reports-section">

                            <div className="reports-section-header">

                                <span>
                                    Orders
                                </span>

                                <h2>
                                    Order Status
                                </h2>

                            </div>

                            <div className="reports-status-grid">

                                <StatusCard
                                    label="Pending"
                                    value={
                                        report.pendingOrders
                                    }
                                />

                                <StatusCard
                                    label="Confirmed"
                                    value={
                                        report.confirmedOrders
                                    }
                                />

                                <StatusCard
                                    label="Processing"
                                    value={
                                        report.processingOrders
                                    }
                                />

                                <StatusCard
                                    label="Shipped"
                                    value={
                                        report.shippedOrders
                                    }
                                />

                                <StatusCard
                                    label="Delivered"
                                    value={
                                        report.deliveredOrders
                                    }
                                />

                                <StatusCard
                                    label="Cancelled"
                                    value={
                                        report.cancelledOrders
                                    }
                                />

                            </div>

                        </section>

                        {/* ==================================================
                            PAYMENT STATUS
                           ================================================== */}

                        <section className="reports-section">

                            <div className="reports-section-header">

                                <span>
                                    Payments
                                </span>

                                <h2>
                                    Payment Status
                                </h2>

                            </div>

                            <div className="reports-status-grid payment">

                                <StatusCard
                                    label="Total Payments"
                                    value={
                                        report.totalPayments
                                    }
                                />

                                <StatusCard
                                    label="Successful"
                                    value={
                                        report.successfulPayments
                                    }
                                />

                                <StatusCard
                                    label="Pending"
                                    value={
                                        report.pendingPayments
                                    }
                                />

                                <StatusCard
                                    label="Failed"
                                    value={
                                        report.failedPayments
                                    }
                                />

                                <StatusCard
                                    label="Refunded"
                                    value={
                                        report.refundedPayments
                                    }
                                />

                            </div>

                        </section>

                        {/* ==================================================
                            PRODUCTS + PAYMENT METHODS
                           ================================================== */}

                        <div className="reports-two-column">

                            <section className="reports-table-card">

                                <div className="reports-section-header">

                                    <span>
                                        Products
                                    </span>

                                    <h2>
                                        Top Selling Products
                                    </h2>

                                </div>

                                <div className="reports-table-wrapper">

                                    <table>

                                        <thead>

                                            <tr>

                                                <th>
                                                    Product
                                                </th>

                                                <th>
                                                    Qty
                                                </th>

                                                <th>
                                                    Revenue
                                                </th>

                                            </tr>

                                        </thead>

                                        <tbody>

                                            {
                                                report
                                                    .topProducts
                                                    ?.length >
                                                    0
                                                    ? report
                                                        .topProducts
                                                        .map(
                                                            product => (

                                                                <tr
                                                                    key={
                                                                        product.productId
                                                                    }
                                                                >

                                                                    <td>
                                                                        {product.productName}
                                                                    </td>

                                                                    <td>
                                                                        {product.quantitySold}
                                                                    </td>

                                                                    <td>
                                                                        ₹{formatMoney(
                                                                            product.revenue
                                                                        )}
                                                                    </td>

                                                                </tr>

                                                            )
                                                        )

                                                    : (

                                                        <tr>

                                                            <td
                                                                colSpan="3"
                                                                className="reports-empty"
                                                            >
                                                                No sales data.
                                                            </td>

                                                        </tr>
                                                    )
                                            }

                                        </tbody>

                                    </table>

                                </div>

                            </section>

                            <section className="reports-table-card">

                                <div className="reports-section-header">

                                    <span>
                                        Payments
                                    </span>

                                    <h2>
                                        Payment Methods
                                    </h2>

                                </div>

                                <div className="reports-table-wrapper">

                                    <table>

                                        <thead>

                                            <tr>

                                                <th>
                                                    Method
                                                </th>

                                                <th>
                                                    Count
                                                </th>

                                                <th>
                                                    Revenue
                                                </th>

                                            </tr>

                                        </thead>

                                        <tbody>

                                            {
                                                report
                                                    .paymentMethods
                                                    ?.length >
                                                    0
                                                    ? report
                                                        .paymentMethods
                                                        .map(
                                                            item => (

                                                                <tr
                                                                    key={
                                                                        item.paymentMethod
                                                                    }
                                                                >

                                                                    <td>
                                                                        {item.paymentMethod}
                                                                    </td>

                                                                    <td>
                                                                        {item.count}
                                                                    </td>

                                                                    <td>
                                                                        ₹{formatMoney(
                                                                            item.amount
                                                                        )}
                                                                    </td>

                                                                </tr>

                                                            )
                                                        )

                                                    : (

                                                        <tr>

                                                            <td
                                                                colSpan="3"
                                                                className="reports-empty"
                                                            >
                                                                No payment data.
                                                            </td>

                                                        </tr>
                                                    )
                                            }

                                        </tbody>

                                    </table>

                                </div>

                            </section>

                        </div>

                        {/* ==================================================
                            RECENT ORDERS
                           ================================================== */}

                        <section className="reports-table-card recent-orders">

                            <div className="reports-section-header">

                                <span>
                                    Activity
                                </span>

                                <h2>
                                    Recent Orders
                                </h2>

                            </div>

                            <div className="reports-table-wrapper">

                                <table>

                                    <thead>

                                        <tr>

                                            <th>
                                                Order
                                            </th>

                                            <th>
                                                Product
                                            </th>

                                            <th>
                                                Qty
                                            </th>

                                            <th>
                                                Date
                                            </th>

                                            <th>
                                                Order Status
                                            </th>

                                            <th>
                                                Payment
                                            </th>

                                            <th>
                                                Amount
                                            </th>

                                        </tr>

                                    </thead>

                                    <tbody>

                                        {
                                            report
                                                .recentOrders
                                                ?.length >
                                                0
                                                ? report
                                                    .recentOrders
                                                    .map(
                                                        order => (

                                                            <tr
                                                                key={
                                                                    order.orderId
                                                                }
                                                            >

                                                                <td>

                                                                    <strong>
                                                                        {order.orderNumber}
                                                                    </strong>

                                                                </td>

                                                                <td className="report-product-cell">

                                                                    {
                                                                        order.productNames ||
                                                                        '-'
                                                                    }

                                                                </td>

                                                                <td>

                                                                    {
                                                                        order.totalQuantity ??
                                                                        0
                                                                    }

                                                                </td>

                                                                <td>

                                                                    {formatDate(
                                                                        order.createdAt
                                                                    )}

                                                                </td>

                                                                <td>
                                                                    {order.status}
                                                                </td>

                                                                <td>
                                                                    {order.paymentStatus}
                                                                </td>

                                                                <td>

                                                                    <strong>

                                                                        ₹{formatMoney(
                                                                            order.totalAmount
                                                                        )}

                                                                    </strong>

                                                                </td>

                                                            </tr>

                                                        )
                                                    )

                                                : (

                                                    <tr>

                                                        <td
                                                            colSpan="7"
                                                            className="reports-empty"
                                                        >
                                                            No orders available.
                                                        </td>

                                                    </tr>
                                                )
                                        }

                                    </tbody>

                                </table>

                            </div>

                        </section>

                    </>

                )}

            </div>

        </main>
    )
}

// ============================================================
// SUMMARY CARD
// ============================================================

function ReportCard({
    label,
    value,
    description
}) {

    return (

        <article className="report-summary-card">

            <span>
                {label}
            </span>

            <strong>
                {value}
            </strong>

            <p>
                {description}
            </p>

        </article>
    )
}

// ============================================================
// STATUS CARD
// ============================================================

function StatusCard({
    label,
    value
}) {

    return (

        <article className="report-status-card">

            <span>
                {label}
            </span>

            <strong>
                {value ?? 0}
            </strong>

        </article>
    )
}

export default AdminReportsPage