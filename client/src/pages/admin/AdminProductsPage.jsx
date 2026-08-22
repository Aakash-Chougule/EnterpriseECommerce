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
// GST RATES
// ============================================================

const GST_RATES = [
    0,
    5,
    12,
    18,
    28
]

// ============================================================
// ADMIN PRODUCTS PAGE
// ============================================================

function AdminProductsPage() {

    // ========================================================
    // DATA
    // ========================================================

    const [
        products,
        setProducts
    ] = useState([])

    const [
        categories,
        setCategories
    ] = useState([])

    // ========================================================
    // EDIT MODE
    // ========================================================

    const [
        editingProductId,
        setEditingProductId
    ] = useState(null)

    // ========================================================
    // FORM
    // ========================================================

    const [
        formData,
        setFormData
    ] = useState({
        categoryId: '',
        name: '',
        description: '',
        sku: '',
        hsnCode: '',
        gstRate: '18',
        price: '',
        stockQuantity: ''
    })

    // ========================================================
    // UI STATE
    // ========================================================

    const [
        loading,
        setLoading
    ] = useState(true)

    const [
        saving,
        setSaving
    ] = useState(false)

    const [
        error,
        setError
    ] = useState('')

    const [
        message,
        setMessage
    ] = useState('')

    const [
        search,
        setSearch
    ] = useState('')

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
    // FORM CHANGE
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
                    [name]:
                        value
                })
            )
        }

    // ========================================================
    // RESET FORM
    // ========================================================

    const resetForm = () => {

        setEditingProductId(
            null
        )

        setFormData({
            categoryId: '',
            name: '',
            description: '',
            sku: '',
            hsnCode: '',
            gstRate: '18',
            price: '',
            stockQuantity: ''
        })
    }

    // ========================================================
    // EDIT PRODUCT
    // ========================================================

    const handleEdit =
        (product) => {

            setEditingProductId(
                product.id
            )

            setFormData({
                categoryId:
                    product.categoryId ?? '',

                name:
                    product.name ?? '',

                description:
                    product.description ?? '',

                sku:
                    product.sku ?? '',

                hsnCode:
                    product.hsnCode ?? '',

                gstRate:
                    String(
                        product.gstRate ?? 0
                    ),

                price:
                    String(
                        product.price ?? ''
                    ),

                stockQuantity:
                    String(
                        product.stockQuantity ?? ''
                    )
            })

            setMessage('')
            setError('')

            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            })
        }

    // ========================================================
    // VALIDATE
    // ========================================================

    const validateProductForm = () => {

        if (!formData.name.trim()) {

            setError(
                'Product name is required.'
            )

            return false
        }

        if (!editingProductId) {

            if (!formData.categoryId) {

                setError(
                    'Please select a category.'
                )

                return false
            }

            if (!formData.sku.trim()) {

                setError(
                    'SKU is required.'
                )

                return false
            }
        }

        if (
            formData.price === '' ||
            Number.isNaN(
                Number(formData.price)
            )
        ) {

            setError(
                'Valid selling price is required.'
            )

            return false
        }

        if (
            Number(
                formData.price
            ) < 0
        ) {

            setError(
                'Price cannot be negative.'
            )

            return false
        }

        if (
            formData.stockQuantity === '' ||
            Number.isNaN(
                Number(
                    formData.stockQuantity
                )
            )
        ) {

            setError(
                'Valid stock quantity is required.'
            )

            return false
        }

        if (
            Number(
                formData.stockQuantity
            ) < 0
        ) {

            setError(
                'Stock quantity cannot be negative.'
            )

            return false
        }

        const gstRate =
            Number(
                formData.gstRate
            )

        if (
            Number.isNaN(gstRate) ||
            gstRate < 0 ||
            gstRate > 100
        ) {

            setError(
                'GST rate must be between 0 and 100.'
            )

            return false
        }

        return true
    }

    // ========================================================
    // SAVE PRODUCT
    // ========================================================

    const handleSubmit =
        async (event) => {

            event.preventDefault()

            setError('')
            setMessage('')

            if (!validateProductForm()) {

                return
            }

            try {

                setSaving(true)

                // =============================================
                // UPDATE
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
                            ),

                        hsnCode:
                            formData.hsnCode.trim(),

                        gstRate:
                            Number(
                                formData.gstRate
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
                // CREATE
                // =============================================

                else {

                    const createData = {

                        categoryId:
                            formData.categoryId,

                        name:
                            formData.name.trim(),

                        description:
                            formData.description.trim(),

                        sku:
                            formData.sku
                                .trim()
                                .toUpperCase(),

                        hsnCode:
                            formData.hsnCode.trim(),

                        gstRate:
                            Number(
                                formData.gstRate
                            ),

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
    // DEACTIVATE
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
    // CATEGORY NAME
    // ========================================================

    const getCategoryName =
        (categoryId) => {

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
    // PRICE
    // ========================================================

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

    // ========================================================
    // GST EXTRACTED FROM INCLUSIVE PRICE
    // ========================================================

    const calculateIncludedGst =
        (price, gstRate) => {

            const sellingPrice =
                Number(
                    price ?? 0
                )

            const taxRate =
                Number(
                    gstRate ?? 0
                )

            if (
                sellingPrice <= 0 ||
                taxRate <= 0
            ) {

                return 0
            }

            const taxableValue =
                sellingPrice *
                100 /
                (100 + taxRate)

            return (
                sellingPrice -
                taxableValue
            )
        }

    // ========================================================
    // TAXABLE VALUE
    // ========================================================

    const calculateTaxableValue =
        (price, gstRate) => {

            const sellingPrice =
                Number(
                    price ?? 0
                )

            const taxRate =
                Number(
                    gstRate ?? 0
                )

            if (
                sellingPrice <= 0
            ) {

                return 0
            }

            if (
                taxRate <= 0
            ) {

                return sellingPrice
            }

            return (
                sellingPrice *
                100 /
                (100 + taxRate)
            )
        }

    // ========================================================
    // STOCK STATUS
    // ========================================================

    const getStockStatus =
        (stockQuantity) => {

            if (
                stockQuantity === 0
            ) {

                return {
                    label:
                        'Out of Stock',

                    className:
                        'danger'
                }
            }

            if (
                stockQuantity <= 5
            ) {

                return {
                    label:
                        'Low Stock',

                    className:
                        'warning'
                }
            }

            return {
                label:
                    'In Stock',

                className:
                    'success'
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
                        ${product.hsnCode ?? ''}
                        ${product.gstRate ?? ''}
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

                {/* =============================================
                    HEADER
                   ============================================= */}

                <header className="admin-products-header">

                    <div>

                        <span className="admin-products-eyebrow">
                            Product Management
                        </span>

                        <h1>
                            Admin Products
                        </h1>

                        <p>
                            Manage catalog information,
                            GST, HSN codes, prices and inventory.
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

                {/* =============================================
                    ALERTS
                   ============================================= */}

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

                {/* =============================================
                    PRODUCT FORM
                   ============================================= */}

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
                                        ? 'Update product pricing, tax and inventory information.'
                                        : 'Add a GST-ready product to your catalog.'
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

                            {/* =================================
                                CATEGORY
                               ================================= */}

                            <div className="admin-form-group">

                                <label htmlFor="categoryId">
                                    Category
                                </label>

                                {editingProductId ? (

                                    <div className="admin-readonly-field">

                                        {
                                            getCategoryName(
                                                formData.categoryId
                                            )
                                        }

                                        <span>
                                            Category cannot currently
                                            be changed while editing.
                                        </span>

                                    </div>

                                ) : (

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

                                )}

                            </div>

                            {/* =================================
                                PRODUCT NAME
                               ================================= */}

                            <div className="admin-form-group">

                                <label htmlFor="name">
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
                                    required
                                />

                            </div>

                            {/* =================================
                                SKU
                               ================================= */}

                            <div className="admin-form-group">

                                <label htmlFor="sku">
                                    SKU
                                </label>

                                {editingProductId ? (

                                    <div className="admin-readonly-field code">

                                        {
                                            formData.sku ||
                                            '-'
                                        }

                                        <span>
                                            SKU remains fixed after creation.
                                        </span>

                                    </div>

                                ) : (

                                    <input
                                        id="sku"
                                        name="sku"
                                        type="text"
                                        value={
                                            formData.sku
                                        }
                                        placeholder="Example: MOUSE-001"
                                        onChange={
                                            handleChange
                                        }
                                        required
                                    />

                                )}

                            </div>

                            {/* =================================
                                HSN CODE
                               ================================= */}

                            <div className="admin-form-group">

                                <label htmlFor="hsnCode">
                                    HSN Code
                                </label>

                                <input
                                    id="hsnCode"
                                    name="hsnCode"
                                    type="text"
                                    value={
                                        formData.hsnCode
                                    }
                                    placeholder="Example: 8471"
                                    maxLength="20"
                                    onChange={
                                        handleChange
                                    }
                                />

                                <small className="admin-field-help">
                                    Used for GST invoices and tax reports.
                                </small>

                            </div>

                            {/* =================================
                                GST RATE
                               ================================= */}

                            <div className="admin-form-group">

                                <label htmlFor="gstRate">
                                    GST Rate
                                </label>

                                <select
                                    id="gstRate"
                                    name="gstRate"
                                    value={
                                        formData.gstRate
                                    }
                                    onChange={
                                        handleChange
                                    }
                                >

                                    {GST_RATES.map(
                                        rate => (

                                            <option
                                                key={rate}
                                                value={rate}
                                            >
                                                {rate}% GST
                                            </option>

                                        )
                                    )}

                                </select>

                                <small className="admin-field-help">
                                    GST is included in the selling price.
                                </small>

                            </div>

                            {/* =================================
                                STOCK
                               ================================= */}

                            <div className="admin-form-group">

                                <label htmlFor="stockQuantity">
                                    Stock Quantity
                                </label>

                                <input
                                    id="stockQuantity"
                                    name="stockQuantity"
                                    type="number"
                                    min="0"
                                    step="1"
                                    value={
                                        formData.stockQuantity
                                    }
                                    placeholder="0"
                                    onChange={
                                        handleChange
                                    }
                                    required
                                />

                            </div>

                            {/* =================================
                                SELLING PRICE
                               ================================= */}

                            <div className="admin-form-group full-width">

                                <label htmlFor="price">
                                    Selling Price
                                    {' '}
                                    <span className="admin-gst-inclusive-label">
                                        GST Inclusive
                                    </span>
                                </label>

                                <div className="admin-price-input-wrap">

                                    <span>
                                        ₹
                                    </span>

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
                                        required
                                    />

                                </div>

                                <small className="admin-field-help">
                                    Enter the final customer-facing price.
                                    GST will not be added again at checkout.
                                </small>

                                {
                                    formData.price !== '' &&
                                    Number(
                                        formData.price
                                    ) >= 0 &&
                                    (

                                        <div className="admin-tax-preview">

                                            <div>

                                                <span>
                                                    Selling Price
                                                </span>

                                                <strong>
                                                    ₹{
                                                        formatPrice(
                                                            formData.price
                                                        )
                                                    }
                                                </strong>

                                            </div>

                                            <div>

                                                <span>
                                                    Taxable Value
                                                </span>

                                                <strong>
                                                    ₹{
                                                        formatPrice(
                                                            calculateTaxableValue(
                                                                formData.price,
                                                                formData.gstRate
                                                            )
                                                        )
                                                    }
                                                </strong>

                                            </div>

                                            <div>

                                                <span>
                                                    Included GST
                                                </span>

                                                <strong>
                                                    ₹{
                                                        formatPrice(
                                                            calculateIncludedGst(
                                                                formData.price,
                                                                formData.gstRate
                                                            )
                                                        )
                                                    }
                                                </strong>

                                            </div>

                                            <div>

                                                <span>
                                                    GST Rate
                                                </span>

                                                <strong>
                                                    {
                                                        Number(
                                                            formData.gstRate
                                                        )
                                                    }%
                                                </strong>

                                            </div>

                                        </div>

                                    )
                                }

                            </div>

                            {/* =================================
                                DESCRIPTION
                               ================================= */}

                            <div className="admin-form-group full-width">

                                <label htmlFor="description">
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

                        {/* =====================================
                            ACTIONS
                           ===================================== */}

                        <div className="admin-product-form-actions">

                            {editingProductId && (

                                <button
                                    type="button"
                                    className="admin-product-cancel-button"
                                    onClick={
                                        resetForm
                                    }
                                    disabled={
                                        saving
                                    }
                                >
                                    Cancel Edit
                                </button>

                            )}

                            <button
                                type="submit"
                                className="admin-product-save-button"
                                disabled={
                                    saving
                                }
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

                {/* =============================================
                    PRODUCT LIST
                   ============================================= */}

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

                    {/* =========================================
                        SEARCH
                       ========================================= */}

                    <div className="admin-products-toolbar">

                        <div className="admin-products-search">

                            <input
                                type="search"
                                value={
                                    search
                                }
                                placeholder="Search product, SKU, HSN, GST or category..."
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

                    {/* =========================================
                        LOADING
                       ========================================= */}

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
                                            HSN
                                        </th>

                                        <th>
                                            GST
                                        </th>

                                        <th>
                                            Selling Price
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

                                            const includedGst =
                                                calculateIncludedGst(
                                                    product.price,
                                                    product.gstRate
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

                                                                {
                                                                    product.name
                                                                        ?.charAt(0)
                                                                        .toUpperCase()
                                                                    || 'P'
                                                                }

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
                                                        {
                                                            product.sku
                                                        }
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
                                                        data-label="HSN"
                                                    >

                                                        <span className="admin-hsn-code">

                                                            {
                                                                product.hsnCode ||
                                                                '—'
                                                            }

                                                        </span>

                                                    </td>

                                                    <td
                                                        data-label="GST"
                                                    >

                                                        <span
                                                            className="admin-gst-badge"
                                                        >
                                                            {
                                                                Number(
                                                                    product.gstRate ?? 0
                                                                )
                                                            }%
                                                        </span>

                                                    </td>

                                                    <td
                                                        data-label="Selling Price"
                                                    >

                                                        <div className="admin-price-cell">

                                                            <strong>
                                                                ₹{
                                                                    formatPrice(
                                                                        product.price
                                                                    )
                                                                }
                                                            </strong>

                                                            <span>
                                                                Includes ₹{
                                                                    formatPrice(
                                                                        includedGst
                                                                    )
                                                                } GST
                                                            </span>

                                                        </div>

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