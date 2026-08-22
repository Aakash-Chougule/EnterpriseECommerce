import {
    useEffect,
    useMemo,
    useState
} from 'react'

import {
    getProducts,
    getLowStockProducts,
    increaseProductStock,
    decreaseProductStock
} from '../../services/productService'

import {
    getCategories
} from '../../services/categoryService'

import {
    exportInventoryCsv,
    exportInventoryExcel,
    exportInventoryPdf,
    getInventoryReport
} from '../../services/inventoryReportService'

import './AdminInventoryPage.css'

// ============================================================
// ADMIN INVENTORY PAGE
// ============================================================

function AdminInventoryPage() {

    const [products, setProducts] =
        useState([])

    const [categories, setCategories] =
        useState([])

    const [
        lowStockProducts,
        setLowStockProducts
    ] = useState([])

    const [
        inventoryReport,
        setInventoryReport
    ] = useState(null)

    const [threshold, setThreshold] =
        useState(5)

    const [search, setSearch] =
        useState('')

    const [
        stockFilter,
        setStockFilter
    ] = useState('all')

    const [
        reportSearch,
        setReportSearch
    ] = useState('')

    const [
        reportCategory,
        setReportCategory
    ] = useState('all')

    const [
        reportStockFilter,
        setReportStockFilter
    ] = useState('all')

    const [
        adjustmentQuantities,
        setAdjustmentQuantities
    ] = useState({})

    const [
        processingProductId,
        setProcessingProductId
    ] = useState(null)

    const [loading, setLoading] =
        useState(true)

    const [exporting, setExporting] =
        useState('')

    const [error, setError] =
        useState('')

    const [message, setMessage] =
        useState('')

    // ========================================================
    // LOAD
    // ========================================================

    const loadInventory =
        async () => {

            try {

                setLoading(
                    true
                )

                setError('')

                const [
                    productData,
                    categoryData,
                    lowStockData,
                    reportData
                ] =
                    await Promise.all([
                        getProducts(),

                        getCategories(),

                        getLowStockProducts(
                            threshold
                        ),

                        getInventoryReport(
                            threshold
                        )
                    ])

                setProducts(
                    Array.isArray(
                        productData
                    )
                        ? productData
                        : []
                )

                setCategories(
                    Array.isArray(
                        categoryData
                    )
                        ? categoryData
                        : []
                )

                setLowStockProducts(
                    Array.isArray(
                        lowStockData
                    )
                        ? lowStockData
                        : []
                )

                setInventoryReport(
                    reportData
                )
            }
            catch (err) {

                console.error(
                    'Failed to load inventory:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Unable to load inventory.'
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

            loadInventory()

        },
        [threshold]
    )

    // ========================================================
    // CATEGORY
    // ========================================================

    const getCategoryName =
        categoryId => {

            const category =
                categories.find(
                    item =>
                        item.id ===
                        categoryId
                )

            return (
                category?.name ||
                'Unknown Category'
            )
        }

    // ========================================================
    // COUNTS
    // ========================================================

    const outOfStockCount =
        products.filter(
            product =>
                product.stockQuantity ===
                0
        ).length

    const lowStockCount =
        lowStockProducts.filter(
            product =>
                product.stockQuantity >
                0
        ).length

    const inStockCount =
        products.filter(
            product =>
                product.stockQuantity >
                threshold
        ).length

    const totalStock =
        products.reduce(
            (
                total,
                product
            ) =>
                total +
                Number(
                    product.stockQuantity ??
                    0
                ),
            0
        )

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
                    minimumFractionDigits:
                        2,

                    maximumFractionDigits:
                        2
                }
            )

    // ========================================================
    // STOCK STATUS
    // ========================================================

    const getStockStatus =
        stockQuantity => {

            if (
                stockQuantity ===
                0
            ) {

                return 'Out of Stock'
            }

            if (
                stockQuantity <=
                threshold
            ) {

                return 'Low Stock'
            }

            return 'In Stock'
        }

    const getStockStatusClass =
        stockQuantity => {

            if (
                stockQuantity ===
                0
            ) {

                return 'out'
            }

            if (
                stockQuantity <=
                threshold
            ) {

                return 'low'
            }

            return 'available'
        }

    // ========================================================
    // MANAGEMENT FILTER
    // ========================================================

    const filteredProducts =
        useMemo(
            () => {

                const normalizedSearch =
                    search
                        .trim()
                        .toLowerCase()

                return products.filter(
                    product => {

                        const categoryName =
                            getCategoryName(
                                product.categoryId
                            )

                        const searchableText =
                            `
                            ${product.name ?? ''}
                            ${product.sku ?? ''}
                            ${product.description ?? ''}
                            ${categoryName}
                            ${product.stockQuantity ?? ''}
                            `
                                .toLowerCase()

                        const matchesSearch =
                            !normalizedSearch ||
                            searchableText.includes(
                                normalizedSearch
                            )

                        let matchesFilter =
                            true

                        if (
                            stockFilter ===
                            'in-stock'
                        ) {

                            matchesFilter =
                                product.stockQuantity >
                                threshold
                        }

                        if (
                            stockFilter ===
                            'low-stock'
                        ) {

                            matchesFilter =
                                product.stockQuantity >
                                0 &&
                                product.stockQuantity <=
                                threshold
                        }

                        if (
                            stockFilter ===
                            'out-of-stock'
                        ) {

                            matchesFilter =
                                product.stockQuantity ===
                                0
                        }

                        return (
                            matchesSearch &&
                            matchesFilter
                        )
                    }
                )
            },
            [
                products,
                categories,
                search,
                stockFilter,
                threshold
            ]
        )

    // ========================================================
    // REPORT FILTER
    // ========================================================

    const filteredReportProducts =
        useMemo(
            () => {

                const rows =
                    inventoryReport
                        ?.products ??
                    []

                const normalized =
                    reportSearch
                        .trim()
                        .toLowerCase()

                return rows.filter(
                    product => {

                        const searchable =
                            `
                            ${product.productName}
                            ${product.sku}
                            ${product.categoryName}
                            ${product.stockStatus}
                            `
                                .toLowerCase()

                        const matchesSearch =
                            !normalized ||
                            searchable.includes(
                                normalized
                            )

                        const matchesCategory =
                            reportCategory ===
                            'all' ||
                            product.categoryId ===
                            reportCategory

                        const status =
                            product.stockStatus
                                ?.toLowerCase()

                        const matchesStock =
                            reportStockFilter ===
                            'all' ||
                            (
                                reportStockFilter ===
                                'in-stock' &&
                                status ===
                                'in stock'
                            ) ||
                            (
                                reportStockFilter ===
                                'low-stock' &&
                                status ===
                                'low stock'
                            ) ||
                            (
                                reportStockFilter ===
                                'out-of-stock' &&
                                status ===
                                'out of stock'
                            )

                        return (
                            matchesSearch &&
                            matchesCategory &&
                            matchesStock
                        )
                    }
                )
            },
            [
                inventoryReport,
                reportSearch,
                reportCategory,
                reportStockFilter
            ]
        )

    // ========================================================
    // ADJUSTMENT
    // ========================================================

    const getAdjustmentQuantity =
        productId => {

            return (
                adjustmentQuantities[
                productId
                ] ?? 1
            )
        }

    const handleAdjustmentQuantityChange =
        (
            productId,
            value
        ) => {

            setAdjustmentQuantities(
                current => ({
                    ...current,

                    [productId]:
                        Number(
                            value
                        )
                })
            )
        }

    // ========================================================
    // INCREASE
    // ========================================================

    const handleIncreaseStock =
        async product => {

            const quantity =
                getAdjustmentQuantity(
                    product.id
                )

            if (
                !Number.isInteger(
                    quantity
                ) ||
                quantity <= 0
            ) {

                setError(
                    'Quantity must be a positive whole number.'
                )

                return
            }

            try {

                setProcessingProductId(
                    product.id
                )

                setError('')
                setMessage('')

                await increaseProductStock(
                    product.id,
                    quantity
                )

                setMessage(
                    `${quantity} unit(s) added to ${product.name}.`
                )

                await loadInventory()
            }
            catch (err) {

                setError(
                    err.response?.data?.message ||
                    'Unable to increase stock.'
                )
            }
            finally {

                setProcessingProductId(
                    null
                )
            }
        }

    // ========================================================
    // DECREASE
    // ========================================================

    const handleDecreaseStock =
        async product => {

            const quantity =
                getAdjustmentQuantity(
                    product.id
                )

            if (
                !Number.isInteger(
                    quantity
                ) ||
                quantity <= 0
            ) {

                setError(
                    'Quantity must be a positive whole number.'
                )

                return
            }

            try {

                setProcessingProductId(
                    product.id
                )

                setError('')
                setMessage('')

                await decreaseProductStock(
                    product.id,
                    quantity
                )

                setMessage(
                    `${quantity} unit(s) removed from ${product.name}.`
                )

                await loadInventory()
            }
            catch (err) {

                setError(
                    err.response?.data?.message ||
                    'Unable to decrease stock.'
                )
            }
            finally {

                setProcessingProductId(
                    null
                )
            }
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

                if (
                    type ===
                    'excel'
                ) {

                    await exportInventoryExcel(
                        threshold
                    )
                }

                if (
                    type ===
                    'csv'
                ) {

                    await exportInventoryCsv(
                        threshold
                    )
                }

                if (
                    type ===
                    'pdf'
                ) {

                    await exportInventoryPdf(
                        threshold
                    )
                }
            }
            catch (err) {

                console.error(
                    'Inventory export failed:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Unable to export inventory report.'
                )
            }
            finally {

                setExporting('')
            }
        }

    // ========================================================
    // LOADING
    // ========================================================

    if (loading) {

        return (

            <main className="admin-inventory-page">

                <div className="admin-inventory-loading">

                    <div className="admin-inventory-spinner" />

                    <h2>
                        Loading inventory...
                    </h2>

                    <p>
                        Checking product stock levels.
                    </p>

                </div>

            </main>
        )
    }

    // ========================================================
    // UI
    // ========================================================

    return (

        <main className="admin-inventory-page">

            <div className="admin-inventory-container">

                {/* ==============================================
                    HEADER
                   ============================================== */}

                <header className="admin-inventory-header">

                    <div>

                        <span className="admin-inventory-eyebrow">
                            Stock Management
                        </span>

                        <h1>
                            Inventory Management
                        </h1>

                        <p>
                            Monitor stock, manage quantities
                            and review product-wise inventory
                            reports by category.
                        </p>

                    </div>

                    <div className="admin-inventory-header-count">

                        <strong>
                            {products.length}
                        </strong>

                        <span>
                            Products
                        </span>

                    </div>

                </header>

                {error && (

                    <div className="admin-inventory-alert error">
                        <span>!</span>
                        {error}
                    </div>

                )}

                {message && (

                    <div className="admin-inventory-alert success">
                        <span>✓</span>
                        {message}
                    </div>

                )}

                {/* ==============================================
                    SUMMARY
                   ============================================== */}

                <section className="inventory-summary-grid">

                    <InventorySummaryCard
                        label="Active Products"
                        value={
                            inventoryReport
                                ?.activeProducts ??
                            products.length
                        }
                        description="Products currently available"
                    />

                    <InventorySummaryCard
                        className="stock"
                        label="In Stock"
                        value={
                            inventoryReport
                                ?.inStockProducts ??
                            inStockCount
                        }
                        description="Above low-stock threshold"
                    />

                    <InventorySummaryCard
                        className="warning"
                        label="Low Stock"
                        value={
                            inventoryReport
                                ?.lowStockProducts ??
                            lowStockCount
                        }
                        description="Needs attention soon"
                    />

                    <InventorySummaryCard
                        className="danger"
                        label="Out of Stock"
                        value={
                            inventoryReport
                                ?.outOfStockProducts ??
                            outOfStockCount
                        }
                        description="Requires restocking"
                    />

                    <InventorySummaryCard
                        className="units"
                        label="Total Units"
                        value={
                            inventoryReport
                                ?.totalUnits ??
                            totalStock
                        }
                        description="Units across inventory"
                    />

                    <InventorySummaryCard
                        className="value"
                        label="Inventory Value"
                        value={
                            `₹${formatMoney(
                                inventoryReport
                                    ?.totalInventoryValue
                            )}`
                        }
                        description="Current stock valuation"
                    />

                </section>

                {/* ==============================================
                    LOW STOCK THRESHOLD
                   ============================================== */}

                <section className="inventory-settings-card">

                    <div className="inventory-settings-info">

                        <span>
                            Inventory Alert
                        </span>

                        <h2>
                            Low Stock Threshold
                        </h2>

                        <p>
                            Products with stock less than
                            or equal to this value are
                            marked as low stock.
                        </p>

                    </div>

                    <div className="inventory-threshold-control">

                        <label htmlFor="stockThreshold">
                            Threshold
                        </label>

                        <input
                            id="stockThreshold"
                            type="number"
                            min="0"
                            step="1"
                            value={
                                threshold
                            }
                            onChange={
                                event =>
                                    setThreshold(
                                        Math.max(
                                            0,
                                            Number(
                                                event.target.value
                                            )
                                        )
                                    )
                            }
                        />

                        <span>
                            ≤ {threshold} units
                        </span>

                    </div>

                </section>

                {/* ==============================================
                    CATEGORY SUMMARY
                   ============================================== */}

                <section className="inventory-report-card">

                    <div className="inventory-report-heading">

                        <div>

                            <span>
                                Inventory Report
                            </span>

                            <h2>
                                Category Summary
                            </h2>

                            <p>
                                Inventory totals grouped by
                                product category.
                            </p>

                        </div>

                    </div>

                    <div className="inventory-report-table-wrapper">

                        <table className="inventory-report-table">

                            <thead>

                                <tr>

                                    <th>
                                        Category
                                    </th>

                                    <th>
                                        Products
                                    </th>

                                    <th>
                                        Units
                                    </th>

                                    <th>
                                        Inventory Value
                                    </th>

                                    <th>
                                        In Stock
                                    </th>

                                    <th>
                                        Low Stock
                                    </th>

                                    <th>
                                        Out of Stock
                                    </th>

                                </tr>

                            </thead>

                            <tbody>

                                {
                                    inventoryReport
                                        ?.categories
                                        ?.map(
                                            category => (

                                                <tr
                                                    key={
                                                        category.categoryId
                                                    }
                                                >

                                                    <td>
                                                        <strong>
                                                            {category.categoryName}
                                                        </strong>
                                                    </td>

                                                    <td>
                                                        {category.productCount}
                                                    </td>

                                                    <td>
                                                        {category.totalUnits}
                                                    </td>

                                                    <td>
                                                        ₹{formatMoney(
                                                            category.inventoryValue
                                                        )}
                                                    </td>

                                                    <td>
                                                        {category.inStockProducts}
                                                    </td>

                                                    <td>
                                                        {category.lowStockProducts}
                                                    </td>

                                                    <td>
                                                        {category.outOfStockProducts}
                                                    </td>

                                                </tr>

                                            )
                                        )
                                }

                            </tbody>

                        </table>

                    </div>

                </section>

                {/* ==============================================
                    PRODUCT-WISE INVENTORY REPORT
                   ============================================== */}

                <section className="inventory-report-card">

                    <div className="inventory-report-heading">

                        <div>

                            <span>
                                Detailed Report
                            </span>

                            <h2>
                                Product-wise Inventory Report
                            </h2>

                            <p>
                                View each product with its
                                category, stock and valuation.
                            </p>

                        </div>

                        <div className="inventory-export-buttons">

                            <button
                                type="button"
                                className="inventory-export-button excel"
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
                                    exporting ===
                                        'excel'
                                        ? 'Exporting...'
                                        : 'Excel'
                                }
                            </button>

                            <button
                                type="button"
                                className="inventory-export-button csv"
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
                                    exporting ===
                                        'csv'
                                        ? 'Exporting...'
                                        : 'CSV'
                                }
                            </button>

                            <button
                                type="button"
                                className="inventory-export-button pdf"
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
                                    exporting ===
                                        'pdf'
                                        ? 'Exporting...'
                                        : 'PDF'
                                }
                            </button>

                        </div>

                    </div>

                    {/* REPORT FILTER */}

                    <div className="inventory-report-toolbar">

                        <input
                            type="search"
                            placeholder="Search product, SKU, category..."
                            value={
                                reportSearch
                            }
                            onChange={
                                event =>
                                    setReportSearch(
                                        event.target.value
                                    )
                            }
                        />

                        <select
                            value={
                                reportCategory
                            }
                            onChange={
                                event =>
                                    setReportCategory(
                                        event.target.value
                                    )
                            }
                        >

                            <option value="all">
                                All Categories
                            </option>

                            {
                                inventoryReport
                                    ?.categories
                                    ?.map(
                                        category => (

                                            <option
                                                key={
                                                    category.categoryId
                                                }
                                                value={
                                                    category.categoryId
                                                }
                                            >
                                                {category.categoryName}
                                            </option>

                                        )
                                    )
                            }

                        </select>

                        <select
                            value={
                                reportStockFilter
                            }
                            onChange={
                                event =>
                                    setReportStockFilter(
                                        event.target.value
                                    )
                            }
                        >

                            <option value="all">
                                All Stock
                            </option>

                            <option value="in-stock">
                                In Stock
                            </option>

                            <option value="low-stock">
                                Low Stock
                            </option>

                            <option value="out-of-stock">
                                Out of Stock
                            </option>

                        </select>

                    </div>

                    <div className="inventory-report-table-wrapper">

                        <table className="inventory-report-table product-report">

                            <thead>

                                <tr>

                                    <th>Product</th>

                                    <th>SKU</th>

                                    <th>Category</th>

                                    <th>Unit Price</th>

                                    <th>Current Stock</th>

                                    <th>Stock Value</th>

                                    <th>Stock Status</th>

                                    <th>Product Status</th>

                                </tr>

                            </thead>

                            <tbody>

                                {
                                    filteredReportProducts
                                        .length ===
                                        0
                                        ? (

                                            <tr>

                                                <td
                                                    colSpan="8"
                                                    className="inventory-report-empty"
                                                >
                                                    No inventory records found.
                                                </td>

                                            </tr>

                                        )
                                        : filteredReportProducts
                                            .map(
                                                product => (

                                                    <tr
                                                        key={
                                                            product.productId
                                                        }
                                                    >

                                                        <td>

                                                            <strong>
                                                                {product.productName}
                                                            </strong>

                                                        </td>

                                                        <td>

                                                            <span className="inventory-sku">
                                                                {product.sku}
                                                            </span>

                                                        </td>

                                                        <td>
                                                            {product.categoryName}
                                                        </td>

                                                        <td>
                                                            ₹{formatMoney(
                                                                product.unitPrice
                                                            )}
                                                        </td>

                                                        <td>

                                                            <strong className="inventory-stock-number">
                                                                {product.stockQuantity}
                                                            </strong>

                                                        </td>

                                                        <td>

                                                            <strong>
                                                                ₹{formatMoney(
                                                                    product.stockValue
                                                                )}
                                                            </strong>

                                                        </td>

                                                        <td>

                                                            <span
                                                                className={
                                                                    `inventory-stock-badge ${product.stockStatus ===
                                                                        'Out of Stock'
                                                                        ? 'out'
                                                                        : product.stockStatus ===
                                                                            'Low Stock'
                                                                            ? 'low'
                                                                            : 'available'
                                                                    }`
                                                                }
                                                            >
                                                                {product.stockStatus}
                                                            </span>

                                                        </td>

                                                        <td>

                                                            <span
                                                                className={
                                                                    product.isActive
                                                                        ? 'inventory-product-status active'
                                                                        : 'inventory-product-status inactive'
                                                                }
                                                            >
                                                                {
                                                                    product.isActive
                                                                        ? 'Active'
                                                                        : 'Inactive'
                                                                }
                                                            </span>

                                                        </td>

                                                    </tr>

                                                )
                                            )
                                }

                            </tbody>

                        </table>

                    </div>

                </section>

                {/* ==============================================
                    STOCK MANAGEMENT TABLE
                   ============================================== */}

                <section className="inventory-table-card">

                    <div className="inventory-table-heading">

                        <div>

                            <span>
                                Inventory Management
                            </span>

                            <h2>
                                Product Stock
                            </h2>

                            <p>
                                Search products and make
                                stock adjustments.
                            </p>

                        </div>

                        <span className="inventory-results-count">

                            Showing{' '}

                            <strong>
                                {filteredProducts.length}
                            </strong>

                            {' '}of{' '}

                            {products.length}

                        </span>

                    </div>

                    <div className="inventory-toolbar">

                        <div className="inventory-search">

                            <input
                                type="search"
                                placeholder="Search name, SKU, category..."
                                value={
                                    search
                                }
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
                            value={
                                stockFilter
                            }
                            onChange={
                                event =>
                                    setStockFilter(
                                        event.target.value
                                    )
                            }
                        >

                            <option value="all">
                                All Stock
                            </option>

                            <option value="in-stock">
                                In Stock
                            </option>

                            <option value="low-stock">
                                Low Stock
                            </option>

                            <option value="out-of-stock">
                                Out of Stock
                            </option>

                        </select>

                    </div>

                    {filteredProducts.length === 0 ? (

                        <div className="inventory-empty">

                            <h3>
                                No products found
                            </h3>

                            <p>
                                Try changing your search
                                or stock filter.
                            </p>

                        </div>

                    ) : (

                        <div className="inventory-table-wrapper">

                            <table className="inventory-table">

                                <thead>

                                    <tr>

                                        <th>Product</th>

                                        <th>SKU</th>

                                        <th>Category</th>

                                        <th>Price</th>

                                        <th>Stock</th>

                                        <th>Status</th>

                                        <th>Quantity</th>

                                        <th>Actions</th>

                                    </tr>

                                </thead>

                                <tbody>

                                    {
                                        filteredProducts
                                            .map(
                                                product => {

                                                    const isProcessing =
                                                        processingProductId ===
                                                        product.id

                                                    const quantity =
                                                        getAdjustmentQuantity(
                                                            product.id
                                                        )

                                                    return (

                                                        <tr
                                                            key={
                                                                product.id
                                                            }
                                                        >

                                                            <td data-label="Product">

                                                                <div className="inventory-product">

                                                                    <div className="inventory-product-icon">
                                                                        {
                                                                            product.name
                                                                                ?.charAt(0)
                                                                                .toUpperCase() ||
                                                                            'P'
                                                                        }
                                                                    </div>

                                                                    <div>

                                                                        <strong>
                                                                            {product.name}
                                                                        </strong>

                                                                        <span>
                                                                            {
                                                                                product.description ||
                                                                                'No description'
                                                                            }
                                                                        </span>

                                                                    </div>

                                                                </div>

                                                            </td>

                                                            <td data-label="SKU">

                                                                <span className="inventory-sku">
                                                                    {product.sku}
                                                                </span>

                                                            </td>

                                                            <td data-label="Category">

                                                                {
                                                                    getCategoryName(
                                                                        product.categoryId
                                                                    )
                                                                }

                                                            </td>

                                                            <td data-label="Price">

                                                                ₹{Number(
                                                                    product.price
                                                                ).toLocaleString(
                                                                    'en-IN'
                                                                )}

                                                            </td>

                                                            <td data-label="Stock">

                                                                <strong className="inventory-stock-number">
                                                                    {product.stockQuantity}
                                                                </strong>

                                                            </td>

                                                            <td data-label="Status">

                                                                <span
                                                                    className={
                                                                        `inventory-stock-badge ${getStockStatusClass(
                                                                            product.stockQuantity
                                                                        )}`
                                                                    }
                                                                >
                                                                    {
                                                                        getStockStatus(
                                                                            product.stockQuantity
                                                                        )
                                                                    }
                                                                </span>

                                                            </td>

                                                            <td data-label="Quantity">

                                                                <input
                                                                    className="inventory-quantity-input"
                                                                    type="number"
                                                                    min="1"
                                                                    step="1"
                                                                    value={
                                                                        quantity
                                                                    }
                                                                    disabled={
                                                                        isProcessing
                                                                    }
                                                                    onChange={
                                                                        event =>
                                                                            handleAdjustmentQuantityChange(
                                                                                product.id,
                                                                                event.target.value
                                                                            )
                                                                    }
                                                                />

                                                            </td>

                                                            <td data-label="Actions">

                                                                <div className="inventory-actions">

                                                                    <button
                                                                        type="button"
                                                                        className="inventory-add-button"
                                                                        disabled={
                                                                            isProcessing
                                                                        }
                                                                        onClick={() =>
                                                                            handleIncreaseStock(
                                                                                product
                                                                            )
                                                                        }
                                                                    >
                                                                        {
                                                                            isProcessing
                                                                                ? 'Working...'
                                                                                : '+ Add'
                                                                        }
                                                                    </button>

                                                                    <button
                                                                        type="button"
                                                                        className="inventory-remove-button"
                                                                        disabled={
                                                                            isProcessing ||
                                                                            product.stockQuantity ===
                                                                            0
                                                                        }
                                                                        onClick={() =>
                                                                            handleDecreaseStock(
                                                                                product
                                                                            )
                                                                        }
                                                                    >
                                                                        - Remove
                                                                    </button>

                                                                </div>

                                                            </td>

                                                        </tr>

                                                    )
                                                }
                                            )
                                    }

                                </tbody>

                            </table>

                        </div>

                    )}

                </section>

            </div>

        </main>
    )
}

// ============================================================
// SUMMARY CARD
// ============================================================

function InventorySummaryCard({
    label,
    value,
    description,
    className = ''
}) {

    return (

        <article
            className={
                `inventory-summary-card ${className}`
            }
        >

            <span>
                {label}
            </span>

            <strong>
                {value}
            </strong>

            <small>
                {description}
            </small>

        </article>
    )
}

export default AdminInventoryPage