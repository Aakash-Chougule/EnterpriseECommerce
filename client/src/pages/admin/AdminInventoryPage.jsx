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

    const [threshold, setThreshold] =
        useState(5)

    const [search, setSearch] =
        useState('')

    const [
        stockFilter,
        setStockFilter
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

    const [error, setError] =
        useState('')

    const [message, setMessage] =
        useState('')

    // ========================================================
    // LOAD INVENTORY
    // ========================================================

    const loadInventory = async () => {

        try {

            setLoading(true)
            setError('')

            const [
                productData,
                categoryData,
                lowStockData
            ] = await Promise.all([
                getProducts(),
                getCategories(),
                getLowStockProducts(
                    threshold
                )
            ])

            setProducts(
                Array.isArray(productData)
                    ? productData
                    : []
            )

            setCategories(
                Array.isArray(categoryData)
                    ? categoryData
                    : []
            )

            setLowStockProducts(
                Array.isArray(lowStockData)
                    ? lowStockData
                    : []
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

            setLoading(false)
        }
    }

    // ========================================================
    // INITIAL LOAD / THRESHOLD CHANGE
    // ========================================================

    useEffect(() => {

        loadInventory()

    }, [threshold])

    // ========================================================
    // CATEGORY NAME
    // ========================================================

    const getCategoryName =
        (categoryId) => {

            const category =
                categories.find(
                    item =>
                        item.id === categoryId
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
                product.stockQuantity === 0
        ).length

    const lowStockCount =
        lowStockProducts.filter(
            product =>
                product.stockQuantity > 0
        ).length

    const inStockCount =
        products.filter(
            product =>
                product.stockQuantity >
                threshold
        ).length

    const totalStock =
        products.reduce(
            (total, product) =>
                total +
                Number(
                    product.stockQuantity ?? 0
                ),
            0
        )

    // ========================================================
    // STOCK STATUS
    // ========================================================

    const getStockStatus =
        (stockQuantity) => {

            if (stockQuantity === 0) {
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
        (stockQuantity) => {

            if (stockQuantity === 0) {
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
    // SEARCH + FILTER
    // ========================================================

    const filteredProducts =
        useMemo(() => {

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

                    const searchableText = `
                        ${product.name ?? ''}
                        ${product.sku ?? ''}
                        ${product.description ?? ''}
                        ${categoryName}
                        ${product.stockQuantity ?? ''}
                    `.toLowerCase()

                    const matchesSearch =
                        !normalizedSearch ||
                        searchableText.includes(
                            normalizedSearch
                        )

                    let matchesFilter = true

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
                            product.stockQuantity > 0 &&
                            product.stockQuantity <=
                            threshold
                    }

                    if (
                        stockFilter ===
                        'out-of-stock'
                    ) {
                        matchesFilter =
                            product.stockQuantity === 0
                    }

                    return (
                        matchesSearch &&
                        matchesFilter
                    )
                }
            )

        }, [
            products,
            categories,
            search,
            stockFilter,
            threshold
        ])

    // ========================================================
    // ADJUSTMENT QUANTITY
    // ========================================================

    const getAdjustmentQuantity =
        (productId) => {

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

            const quantity =
                Number(value)

            setAdjustmentQuantities(
                current => ({
                    ...current,

                    [productId]:
                        quantity
                })
            )
        }

    // ========================================================
    // INCREASE STOCK
    // ========================================================

    const handleIncreaseStock =
        async (product) => {

            const quantity =
                getAdjustmentQuantity(
                    product.id
                )

            if (
                !Number.isInteger(quantity) ||
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

                console.error(
                    'Failed to increase stock:',
                    err
                )

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
    // DECREASE STOCK
    // ========================================================

    const handleDecreaseStock =
        async (product) => {

            const quantity =
                getAdjustmentQuantity(
                    product.id
                )

            if (
                !Number.isInteger(quantity) ||
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

                console.error(
                    'Failed to decrease stock:',
                    err
                )

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
                            Monitor product stock,
                            identify low-stock items and
                            adjust inventory quantities.
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

                {/* ==============================================
                    ALERTS
                   ============================================== */}

                {error && (

                    <div className="admin-inventory-alert error">

                        <span>
                            !
                        </span>

                        {error}

                    </div>

                )}

                {message && (

                    <div className="admin-inventory-alert success">

                        <span>
                            ✓
                        </span>

                        {message}

                    </div>

                )}

                {/* ==============================================
                    SUMMARY
                   ============================================== */}

                <section className="inventory-summary-grid">

                    <article className="inventory-summary-card">

                        <span>
                            Active Products
                        </span>

                        <strong>
                            {products.length}
                        </strong>

                        <small>
                            Products currently available
                        </small>

                    </article>

                    <article className="inventory-summary-card stock">

                        <span>
                            In Stock
                        </span>

                        <strong>
                            {inStockCount}
                        </strong>

                        <small>
                            Above low-stock threshold
                        </small>

                    </article>

                    <article className="inventory-summary-card warning">

                        <span>
                            Low Stock
                        </span>

                        <strong>
                            {lowStockCount}
                        </strong>

                        <small>
                            Needs attention soon
                        </small>

                    </article>

                    <article className="inventory-summary-card danger">

                        <span>
                            Out of Stock
                        </span>

                        <strong>
                            {outOfStockCount}
                        </strong>

                        <small>
                            Requires restocking
                        </small>

                    </article>

                    <article className="inventory-summary-card units">

                        <span>
                            Total Units
                        </span>

                        <strong>
                            {totalStock}
                        </strong>

                        <small>
                            Units across inventory
                        </small>

                    </article>

                </section>

                {/* ==============================================
                    LOW STOCK SETTING
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
                            or equal to the selected value
                            will be marked as low stock.
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
                            value={threshold}
                            onChange={
                                event => {

                                    const value =
                                        Number(
                                            event.target.value
                                        )

                                    setThreshold(
                                        value >= 0
                                            ? value
                                            : 0
                                    )
                                }
                            }
                        />

                        <span>
                            ≤ {threshold} units
                        </span>

                    </div>

                </section>

                {/* ==============================================
                    INVENTORY TABLE
                   ============================================== */}

                <section className="inventory-table-card">

                    <div className="inventory-table-heading">

                        <div>

                            <span>
                                Inventory
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
                                {
                                    filteredProducts.length
                                }
                            </strong>

                            {' '}of{' '}

                            {products.length}

                        </span>

                    </div>

                    {/* ==========================================
                        SEARCH / FILTER
                       ========================================== */}

                    <div className="inventory-toolbar">

                        <div className="inventory-search">

                            <input
                                type="search"
                                placeholder="Search name, SKU, category..."
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
                            value={stockFilter}
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

                    {/* ==========================================
                        EMPTY
                       ========================================== */}

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

                                        <th>
                                            Product
                                        </th>

                                        <th>
                                            SKU
                                        </th>

                                        <th>
                                            Category
                                        </th>

                                        <th>
                                            Price
                                        </th>

                                        <th>
                                            Stock
                                        </th>

                                        <th>
                                            Status
                                        </th>

                                        <th>
                                            Quantity
                                        </th>

                                        <th>
                                            Actions
                                        </th>

                                    </tr>

                                </thead>

                                <tbody>

                                    {filteredProducts.map(
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

                                                    <td
                                                        data-label="Product"
                                                    >

                                                        <div className="inventory-product">

                                                            <div className="inventory-product-icon">

                                                                {product.name
                                                                    ?.charAt(0)
                                                                    .toUpperCase()
                                                                    || 'P'}

                                                            </div>

                                                            <div>

                                                                <strong>
                                                                    {
                                                                        product.name
                                                                    }
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

                                                    <td
                                                        data-label="SKU"
                                                    >

                                                        <span className="inventory-sku">
                                                            {
                                                                product.sku
                                                            }
                                                        </span>

                                                    </td>

                                                    <td
                                                        data-label="Category"
                                                    >
                                                        {
                                                            getCategoryName(
                                                                product.categoryId
                                                            )
                                                        }
                                                    </td>

                                                    <td
                                                        data-label="Price"
                                                    >

                                                        ₹{Number(
                                                            product.price
                                                        ).toLocaleString(
                                                            'en-IN'
                                                        )}

                                                    </td>

                                                    <td
                                                        data-label="Stock"
                                                    >

                                                        <strong className="inventory-stock-number">
                                                            {
                                                                product.stockQuantity
                                                            }
                                                        </strong>

                                                    </td>

                                                    <td
                                                        data-label="Status"
                                                    >

                                                        <span
                                                            className={
                                                                `inventory-stock-badge ${getStockStatusClass(
                                                                    product.stockQuantity
                                                                )
                                                                }`
                                                            }
                                                        >
                                                            {
                                                                getStockStatus(
                                                                    product.stockQuantity
                                                                )
                                                            }
                                                        </span>

                                                    </td>

                                                    <td
                                                        data-label="Quantity"
                                                    >

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

                                                    <td
                                                        data-label="Actions"
                                                    >

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
                                    )}

                                </tbody>

                            </table>

                        </div>

                    )}

                </section>

            </div>

        </main>
    )
}

export default AdminInventoryPage