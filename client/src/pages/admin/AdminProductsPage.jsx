import {
    useEffect,
    useMemo,
    useState
} from 'react'

import {
    getProducts,
    createProduct,
    updateProduct,
    deactivateProduct
} from '../../services/productService'

import {
    getCategories
} from '../../services/categoryService'

import './AdminProductsPage.css'

// ============================================================
// ADMIN PRODUCTS PAGE
// ============================================================

function AdminProductsPage() {

    // ========================================================
    // DATA
    // ========================================================

    const [products, setProducts] =
        useState([])

    const [categories, setCategories] =
        useState([])

    // ========================================================
    // FORM MODE
    // ========================================================

    const [
        editingProductId,
        setEditingProductId
    ] = useState(null)

    // ========================================================
    // FORM STATE
    // ========================================================

    const [formData, setFormData] =
        useState({
            categoryId: '',
            name: '',
            description: '',
            sku: '',
            price: '',
            stockQuantity: ''
        })

    // ========================================================
    // UI STATE
    // ========================================================

    const [loading, setLoading] =
        useState(true)

    const [saving, setSaving] =
        useState(false)

    const [error, setError] =
        useState('')

    const [message, setMessage] =
        useState('')

    const [search, setSearch] =
        useState('')

    // ========================================================
    // LOAD PRODUCTS
    // ========================================================

    const loadProducts = async () => {

        try {

            setLoading(true)
            setError('')

            const data =
                await getProducts()

            setProducts(
                Array.isArray(data)
                    ? data
                    : []
            )
        }
        catch (err) {

            console.error(
                'Failed to load products:',
                err
            )

            setError(
                err.response?.data?.message ||
                'Unable to load products.'
            )
        }
        finally {

            setLoading(false)
        }
    }

    // ========================================================
    // LOAD CATEGORIES
    // ========================================================

    const loadCategories = async () => {

        try {

            const data =
                await getCategories()

            setCategories(
                Array.isArray(data)
                    ? data
                    : []
            )
        }
        catch (err) {

            console.error(
                'Failed to load categories:',
                err
            )

            setError(
                err.response?.data?.message ||
                'Unable to load categories.'
            )
        }
    }

    // ========================================================
    // INPUT CHANGE
    // ========================================================

    const handleChange =
        (event) => {

            const {
                name,
                value
            } = event.target

            setFormData(
                current => ({
                    ...current,
                    [name]: value
                })
            )
        }

    // ========================================================
    // RESET FORM
    // ========================================================

    const resetForm = () => {

        setEditingProductId(null)

        setFormData({
            categoryId: '',
            name: '',
            description: '',
            sku: '',
            price: '',
            stockQuantity: ''
        })
    }

    // ========================================================
    // START EDIT
    // ========================================================

    const handleEdit =
        (product) => {

            setEditingProductId(
                product.id
            )

            setFormData({
                categoryId:
                    product.categoryId,

                name:
                    product.name,

                description:
                    product.description,

                sku:
                    product.sku,

                price:
                    product.price,

                stockQuantity:
                    product.stockQuantity
            })

            setMessage('')
            setError('')

            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            })
        }

    // ========================================================
    // SAVE PRODUCT
    // ========================================================

    const handleSubmit =
        async (event) => {

            event.preventDefault()

            setError('')
            setMessage('')

            if (!formData.name.trim()) {

                setError(
                    'Product name is required.'
                )

                return
            }

            if (
                Number(
                    formData.price
                ) < 0
            ) {

                setError(
                    'Price cannot be negative.'
                )

                return
            }

            if (
                Number(
                    formData.stockQuantity
                ) < 0
            ) {

                setError(
                    'Stock quantity cannot be negative.'
                )

                return
            }

            try {

                setSaving(true)

                // =============================================
                // EDIT MODE
                // =============================================

                if (editingProductId) {

                    const updateData = {

                        name:
                            formData.name.trim(),

                        description:
                            formData.description.trim(),

                        price:
                            Number(
                                formData.price
                            ),

                        stockQuantity:
                            Number(
                                formData.stockQuantity
                            )
                    }

                    await updateProduct(
                        editingProductId,
                        updateData
                    )

                    setMessage(
                        'Product updated successfully.'
                    )
                }

                // =============================================
                // CREATE MODE
                // =============================================

                else {

                    if (!formData.categoryId) {

                        setError(
                            'Please select a category.'
                        )

                        return
                    }

                    if (!formData.sku.trim()) {

                        setError(
                            'SKU is required.'
                        )

                        return
                    }

                    const createData = {

                        categoryId:
                            formData.categoryId,

                        name:
                            formData.name.trim(),

                        description:
                            formData.description.trim(),

                        sku:
                            formData.sku.trim(),

                        price:
                            Number(
                                formData.price
                            ),

                        stockQuantity:
                            Number(
                                formData.stockQuantity
                            )
                    }

                    await createProduct(
                        createData
                    )

                    setMessage(
                        'Product created successfully.'
                    )
                }

                resetForm()

                await loadProducts()
            }
            catch (err) {

                console.error(
                    'Product save failed:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Unable to save product.'
                )
            }
            finally {

                setSaving(false)
            }
        }

    // ========================================================
    // DEACTIVATE PRODUCT
    // ========================================================

    const handleDeactivate =
        async (productId) => {

            const confirmed =
                window.confirm(
                    'Are you sure you want to deactivate this product?'
                )

            if (!confirmed) {
                return
            }

            try {

                setError('')
                setMessage('')

                await deactivateProduct(
                    productId
                )

                setMessage(
                    'Product deactivated successfully.'
                )

                await loadProducts()
            }
            catch (err) {

                console.error(
                    'Failed to deactivate product:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Unable to deactivate product.'
                )
            }
        }

    // ========================================================
    // INITIAL LOAD
    // ========================================================

    useEffect(() => {

        loadProducts()
        loadCategories()

    }, [])

    // ========================================================
    // HELPERS
    // ========================================================

    const getCategoryName =
        (categoryId) => {

            const category =
                categories.find(
                    item =>
                        item.id === categoryId
                )

            return category?.name ||
                'Unknown Category'
        }

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

    const getStockStatus =
        (stockQuantity) => {

            if (stockQuantity === 0) {
                return {
                    label: 'Out of Stock',
                    className: 'danger'
                }
            }

            if (stockQuantity <= 5) {
                return {
                    label: 'Low Stock',
                    className: 'warning'
                }
            }

            return {
                label: 'In Stock',
                className: 'success'
            }
        }

    // ========================================================
    // SEARCH
    // ========================================================

    const filteredProducts =
        useMemo(() => {

            const value =
                search
                    .trim()
                    .toLowerCase()

            if (!value) {
                return products
            }

            return products.filter(
                product => {

                    const categoryName =
                        getCategoryName(
                            product.categoryId
                        )

                    const searchable = `
                        ${product.name ?? ''}
                        ${product.sku ?? ''}
                        ${product.description ?? ''}
                        ${categoryName}
                    `.toLowerCase()

                    return searchable.includes(
                        value
                    )
                }
            )

        }, [
            products,
            categories,
            search
        ])

    // ========================================================
    // UI
    // ========================================================

    return (

        <main className="admin-products-page">

            <div className="admin-products-container">

                {/* ==============================================
                    HEADER
                   ============================================== */}

                <header className="admin-products-header">

                    <div>

                        <span className="admin-products-eyebrow">
                            Product Management
                        </span>

                        <h1>
                            Admin Products
                        </h1>

                        <p>
                            Create, update and manage
                            products in your catalog.
                        </p>

                    </div>

                    <div className="admin-products-count">

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

                    <div className="admin-products-alert error">

                        <span>
                            !
                        </span>

                        {error}

                    </div>

                )}

                {message && (

                    <div className="admin-products-alert success">

                        <span>
                            ✓
                        </span>

                        {message}

                    </div>

                )}

                {/* ==============================================
                    PRODUCT FORM
                   ============================================== */}

                <section className="admin-product-form-card">

                    <div className="admin-product-form-header">

                        <div>

                            <span>
                                {
                                    editingProductId
                                        ? 'Editing'
                                        : 'New Product'
                                }
                            </span>

                            <h2>
                                {
                                    editingProductId
                                        ? 'Edit Product'
                                        : 'Create Product'
                                }
                            </h2>

                            <p>
                                {
                                    editingProductId
                                        ? 'Update the selected product information.'
                                        : 'Add a new product to your catalog.'
                                }
                            </p>

                        </div>

                        {editingProductId && (

                            <div className="editing-badge">
                                Edit Mode
                            </div>

                        )}

                    </div>

                    <form
                        className="admin-product-form"
                        onSubmit={handleSubmit}
                    >

                        <div className="admin-product-form-grid">

                            {/* ==============================
                                CATEGORY
                               ============================== */}

                            {!editingProductId && (

                                <div className="admin-form-group">

                                    <label
                                        htmlFor="categoryId"
                                    >
                                        Category
                                    </label>

                                    <select
                                        id="categoryId"
                                        name="categoryId"
                                        value={
                                            formData.categoryId
                                        }
                                        onChange={
                                            handleChange
                                        }
                                        required
                                    >

                                        <option value="">
                                            Select Category
                                        </option>

                                        {categories.map(
                                            category => (

                                                <option
                                                    key={
                                                        category.id
                                                    }
                                                    value={
                                                        category.id
                                                    }
                                                >
                                                    {
                                                        category.name
                                                    }
                                                </option>

                                            )
                                        )}

                                    </select>

                                </div>

                            )}

                            {/* ==============================
                                NAME
                               ============================== */}

                            <div className="admin-form-group">

                                <label
                                    htmlFor="name"
                                >
                                    Product Name
                                </label>

                                <input
                                    id="name"
                                    name="name"
                                    type="text"
                                    value={
                                        formData.name
                                    }
                                    placeholder="Enter product name"
                                    onChange={
                                        handleChange
                                    }
                                />

                            </div>

                            {/* ==============================
                                SKU
                               ============================== */}

                            {!editingProductId && (

                                <div className="admin-form-group">

                                    <label
                                        htmlFor="sku"
                                    >
                                        SKU
                                    </label>

                                    <input
                                        id="sku"
                                        name="sku"
                                        type="text"
                                        value={
                                            formData.sku
                                        }
                                        placeholder="Example: LAP-001"
                                        onChange={
                                            handleChange
                                        }
                                    />

                                </div>

                            )}

                            {/* ==============================
                                PRICE
                               ============================== */}

                            <div className="admin-form-group">

                                <label
                                    htmlFor="price"
                                >
                                    Price
                                </label>

                                <input
                                    id="price"
                                    name="price"
                                    type="number"
                                    min="0"
                                    step="0.01"
                                    value={
                                        formData.price
                                    }
                                    placeholder="0.00"
                                    onChange={
                                        handleChange
                                    }
                                />

                            </div>

                            {/* ==============================
                                STOCK
                               ============================== */}

                            <div className="admin-form-group">

                                <label
                                    htmlFor="stockQuantity"
                                >
                                    Stock Quantity
                                </label>

                                <input
                                    id="stockQuantity"
                                    name="stockQuantity"
                                    type="number"
                                    min="0"
                                    value={
                                        formData.stockQuantity
                                    }
                                    placeholder="0"
                                    onChange={
                                        handleChange
                                    }
                                />

                            </div>

                            {/* ==============================
                                DESCRIPTION
                               ============================== */}

                            <div className="admin-form-group full-width">

                                <label
                                    htmlFor="description"
                                >
                                    Description
                                </label>

                                <textarea
                                    id="description"
                                    name="description"
                                    value={
                                        formData.description
                                    }
                                    placeholder="Enter product description"
                                    rows="4"
                                    onChange={
                                        handleChange
                                    }
                                />

                            </div>

                        </div>

                        {/* ==================================
                            FORM ACTIONS
                           ================================== */}

                        <div className="admin-product-form-actions">

                            {editingProductId && (

                                <button
                                    type="button"
                                    className="admin-product-cancel-button"
                                    onClick={
                                        resetForm
                                    }
                                >
                                    Cancel Edit
                                </button>

                            )}

                            <button
                                type="submit"
                                className="admin-product-save-button"
                                disabled={saving}
                            >
                                {
                                    saving
                                        ? 'Saving...'
                                        : editingProductId
                                            ? 'Update Product'
                                            : 'Create Product'
                                }
                            </button>

                        </div>

                    </form>

                </section>

                {/* ==============================================
                    PRODUCTS LIST
                   ============================================== */}

                <section className="admin-products-list-section">

                    <div className="admin-products-list-header">

                        <div>

                            <span>
                                Catalog
                            </span>

                            <h2>
                                Products
                            </h2>

                        </div>

                        <span className="admin-products-total">
                            {products.length}
                            {' '}
                            {
                                products.length === 1
                                    ? 'product'
                                    : 'products'
                            }
                        </span>

                    </div>

                    {/* ==========================================
                        SEARCH
                       ========================================== */}

                    <div className="admin-products-toolbar">

                        <div className="admin-products-search">

                            <input
                                type="search"
                                value={search}
                                placeholder="Search by product, SKU or category..."
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

                        <span className="admin-products-showing">

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
                        LOADING
                       ========================================== */}

                    {loading ? (

                        <div className="admin-products-loading">

                            <div className="admin-products-spinner" />

                            <p>
                                Loading products...
                            </p>

                        </div>

                    ) : filteredProducts.length === 0 ? (

                        <div className="admin-products-empty">

                            <h3>
                                No products found
                            </h3>

                            <p>
                                Try another search term
                                or create a new product.
                            </p>

                        </div>

                    ) : (

                        <div className="admin-products-table-wrapper">

                            <table className="admin-products-table">

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
                                            Actions
                                        </th>

                                    </tr>

                                </thead>

                                <tbody>

                                    {filteredProducts.map(
                                        product => {

                                            const stockStatus =
                                                getStockStatus(
                                                    product.stockQuantity
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

                                                        <div className="admin-product-name-cell">

                                                            <div className="admin-product-avatar">

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
                                                        className="admin-product-sku"
                                                    >
                                                        {product.sku}
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
                                                        ₹{formatPrice(
                                                            product.price
                                                        )}
                                                    </td>

                                                    <td
                                                        data-label="Stock"
                                                    >
                                                        <strong>
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
                                                                `admin-product-status ${stockStatus.className}`
                                                            }
                                                        >
                                                            {
                                                                stockStatus.label
                                                            }
                                                        </span>

                                                    </td>

                                                    <td
                                                        data-label="Actions"
                                                    >

                                                        <div className="admin-product-actions">

                                                            <button
                                                                type="button"
                                                                className="admin-product-edit-button"
                                                                onClick={() =>
                                                                    handleEdit(
                                                                        product
                                                                    )
                                                                }
                                                            >
                                                                Edit
                                                            </button>

                                                            <button
                                                                type="button"
                                                                className="admin-product-deactivate-button"
                                                                onClick={() =>
                                                                    handleDeactivate(
                                                                        product.id
                                                                    )
                                                                }
                                                            >
                                                                Deactivate
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

export default AdminProductsPage