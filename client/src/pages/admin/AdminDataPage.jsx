import {
    useEffect,
    useMemo,
    useState
} from 'react'

import {
    getAllProductsForAdmin
} from '../../services/productService'

import {
    getAllCategoriesForAdmin
} from '../../services/categoryService'

import './AdminDataPage.css'

// ============================================================
// ADMIN DATA / ARCHIVE PAGE
// ============================================================
//
// Displays:
//
// 1. Categories
// 2. Products
//
// Includes active and inactive records.
//
// Categories and products are loaded independently so that
// one section can still work if the other API fails.
// ============================================================

function AdminDataPage() {

    // ========================================================
    // DATA
    // ========================================================

    const [categories, setCategories] =
        useState([])

    const [products, setProducts] =
        useState([])

    // ========================================================
    // SEARCH
    // ========================================================

    const [
        categorySearch,
        setCategorySearch
    ] = useState('')

    const [
        productSearch,
        setProductSearch
    ] = useState('')

    // ========================================================
    // STATUS FILTER
    // ========================================================

    const [
        categoryStatus,
        setCategoryStatus
    ] = useState('all')

    const [
        productStatus,
        setProductStatus
    ] = useState('all')

    // ========================================================
    // LOADING
    // ========================================================

    const [
        categoriesLoading,
        setCategoriesLoading
    ] = useState(true)

    const [
        productsLoading,
        setProductsLoading
    ] = useState(true)

    // ========================================================
    // ERRORS
    // ========================================================

    const [
        categoryError,
        setCategoryError
    ] = useState('')

    const [
        productError,
        setProductError
    ] = useState('')

    // ========================================================
    // LOAD CATEGORIES
    // ========================================================

    const loadCategories = async () => {

        try {

            setCategoriesLoading(true)

            setCategoryError('')

            const data =
                await getAllCategoriesForAdmin()

            console.log(
                'Admin categories:',
                data
            )

            setCategories(
                Array.isArray(data)
                    ? data
                    : []
            )
        }
        catch (error) {

            console.error(
                'Category API error:',
                error
            )

            console.error(
                'Category status:',
                error.response?.status
            )

            console.error(
                'Category response:',
                error.response?.data
            )

            setCategoryError(
                error.response?.data?.message ||
                `Unable to load categories. ${error.response?.status
                    ? `HTTP ${error.response.status}`
                    : ''
                }`
            )
        }
        finally {

            setCategoriesLoading(false)
        }
    }

    // ========================================================
    // LOAD PRODUCTS
    // ========================================================

    const loadProducts = async () => {

        try {

            setProductsLoading(true)

            setProductError('')

            const data =
                await getAllProductsForAdmin()

            console.log(
                'Admin products:',
                data
            )

            setProducts(
                Array.isArray(data)
                    ? data
                    : []
            )
        }
        catch (error) {

            console.error(
                'Product API error:',
                error
            )

            console.error(
                'Product status:',
                error.response?.status
            )

            console.error(
                'Product response:',
                error.response?.data
            )

            setProductError(
                error.response?.data?.message ||
                `Unable to load products. ${error.response?.status
                    ? `HTTP ${error.response.status}`
                    : ''
                }`
            )
        }
        finally {

            setProductsLoading(false)
        }
    }

    // ========================================================
    // INITIAL LOAD
    // ========================================================

    useEffect(() => {

        loadCategories()

        loadProducts()

    }, [])

    // ========================================================
    // GET CATEGORY NAME
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

    const activeCategoriesCount =
        categories.filter(
            category =>
                category.isActive
        ).length

    const inactiveCategoriesCount =
        categories.filter(
            category =>
                !category.isActive
        ).length

    const activeProductsCount =
        products.filter(
            product =>
                product.isActive
        ).length

    const inactiveProductsCount =
        products.filter(
            product =>
                !product.isActive
        ).length

    // ========================================================
    // CATEGORY SEARCH + FILTER
    // ========================================================

    const filteredCategories =
        useMemo(() => {

            const search =
                categorySearch
                    .trim()
                    .toLowerCase()

            return categories.filter(
                category => {

                    const searchableText = `
                        ${category.name ?? ''}
                        ${category.description ?? ''}
                        ${category.id ?? ''}
                        ${category.isActive
                            ? 'active'
                            : 'inactive'
                        }
                    `.toLowerCase()

                    const matchesSearch =
                        !search ||
                        searchableText.includes(
                            search
                        )

                    let matchesStatus =
                        true

                    if (
                        categoryStatus ===
                        'active'
                    ) {
                        matchesStatus =
                            category.isActive ===
                            true
                    }

                    if (
                        categoryStatus ===
                        'inactive'
                    ) {
                        matchesStatus =
                            category.isActive ===
                            false
                    }

                    return (
                        matchesSearch &&
                        matchesStatus
                    )
                }
            )

        }, [
            categories,
            categorySearch,
            categoryStatus
        ])

    // ========================================================
    // PRODUCT SEARCH + FILTER
    // ========================================================

    const filteredProducts =
        useMemo(() => {

            const search =
                productSearch
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
                        ${product.categoryId ?? ''}
                        ${categoryName}
                        ${product.price ?? ''}
                        ${product.stockQuantity ?? ''}
                        ${product.isActive
                            ? 'active'
                            : 'inactive'
                        }
                    `.toLowerCase()

                    const matchesSearch =
                        !search ||
                        searchableText.includes(
                            search
                        )

                    let matchesStatus =
                        true

                    if (
                        productStatus ===
                        'active'
                    ) {
                        matchesStatus =
                            product.isActive ===
                            true
                    }

                    if (
                        productStatus ===
                        'inactive'
                    ) {
                        matchesStatus =
                            product.isActive ===
                            false
                    }

                    return (
                        matchesSearch &&
                        matchesStatus
                    )
                }
            )

        }, [
            products,
            categories,
            productSearch,
            productStatus
        ])

    // ========================================================
    // COPY VALUE
    // ========================================================

    const copyValue = async (value) => {

        try {

            await navigator.clipboard.writeText(
                String(value)
            )
        }
        catch (error) {

            console.error(
                'Unable to copy value:',
                error
            )
        }
    }

    // ========================================================
    // UI
    // ========================================================

    return (

        <main className="admin-data-page">

            <div className="admin-data-container">

                {/* ==============================================
                    PAGE HEADER
                   ============================================== */}

                <header className="admin-data-header">

                    <div>

                        <span className="admin-data-eyebrow">
                            Database Viewer
                        </span>

                        <h1>
                            Admin Data / Archive
                        </h1>

                        <p>
                            View and search active and inactive
                            category and product records.
                        </p>

                    </div>

                    <div className="admin-data-record-count">

                        <strong>
                            {
                                categories.length +
                                products.length
                            }
                        </strong>

                        <span>
                            Total Records
                        </span>

                    </div>

                </header>

                {/* ==============================================
                    SUMMARY CARDS
                   ============================================== */}

                <section className="admin-data-summary">

                    <article className="admin-data-summary-card">

                        <span>
                            Categories
                        </span>

                        <strong>
                            {categories.length}
                        </strong>

                        <small>
                            Total category records
                        </small>

                    </article>

                    <article className="admin-data-summary-card active">

                        <span>
                            Active Categories
                        </span>

                        <strong>
                            {activeCategoriesCount}
                        </strong>

                        <small>
                            Currently available
                        </small>

                    </article>

                    <article className="admin-data-summary-card inactive">

                        <span>
                            Inactive Categories
                        </span>

                        <strong>
                            {inactiveCategoriesCount}
                        </strong>

                        <small>
                            Archived records
                        </small>

                    </article>

                    <article className="admin-data-summary-card">

                        <span>
                            Products
                        </span>

                        <strong>
                            {products.length}
                        </strong>

                        <small>
                            Total product records
                        </small>

                    </article>

                    <article className="admin-data-summary-card active">

                        <span>
                            Active Products
                        </span>

                        <strong>
                            {activeProductsCount}
                        </strong>

                        <small>
                            Currently available
                        </small>

                    </article>

                    <article className="admin-data-summary-card inactive">

                        <span>
                            Inactive Products
                        </span>

                        <strong>
                            {inactiveProductsCount}
                        </strong>

                        <small>
                            Archived records
                        </small>

                    </article>

                </section>

                {/* ==============================================
                    CATEGORY SECTION
                   ============================================== */}

                <section className="admin-data-section">

                    <div className="admin-data-section-header">

                        <div>

                            <span className="admin-data-section-label">
                                Section 01
                            </span>

                            <h2>
                                Categories
                            </h2>

                            <p>
                                Search category names,
                                descriptions, IDs and status.
                            </p>

                        </div>

                        <span className="admin-data-showing">

                            Showing{' '}

                            <strong>
                                {
                                    filteredCategories.length
                                }
                            </strong>

                            {' '}of{' '}

                            {categories.length}

                        </span>

                    </div>

                    {/* ==========================================
                        CATEGORY TOOLBAR
                       ========================================== */}

                    <div className="admin-data-toolbar">

                        <div className="admin-data-search">

                            <span className="admin-data-search-icon">
                                ⌕
                            </span>

                            <input
                                type="search"
                                placeholder="Search categories..."
                                value={categorySearch}
                                onChange={
                                    event =>
                                        setCategorySearch(
                                            event.target.value
                                        )
                                }
                            />

                            {categorySearch && (

                                <button
                                    type="button"
                                    title="Clear search"
                                    onClick={() =>
                                        setCategorySearch('')
                                    }
                                >
                                    ×
                                </button>

                            )}

                        </div>

                        <select
                            value={categoryStatus}
                            onChange={
                                event =>
                                    setCategoryStatus(
                                        event.target.value
                                    )
                            }
                        >

                            <option value="all">
                                All Status
                            </option>

                            <option value="active">
                                Active
                            </option>

                            <option value="inactive">
                                Inactive
                            </option>

                        </select>

                    </div>

                    {/* ==========================================
                        CATEGORY ERROR
                       ========================================== */}

                    {categoryError && (

                        <div className="admin-data-error">

                            <div>

                                <strong>
                                    Unable to load categories
                                </strong>

                                <span>
                                    {categoryError}
                                </span>

                            </div>

                            <button
                                type="button"
                                onClick={loadCategories}
                            >
                                Retry
                            </button>

                        </div>

                    )}

                    {/* ==========================================
                        CATEGORY CONTENT
                       ========================================== */}

                    {categoriesLoading ? (

                        <div className="admin-data-loading">

                            <div className="admin-data-spinner" />

                            <span>
                                Loading categories...
                            </span>

                        </div>

                    ) : filteredCategories.length === 0 ? (

                        <div className="admin-data-empty">

                            <h3>
                                No categories found
                            </h3>

                            <p>
                                Try changing your search
                                or status filter.
                            </p>

                        </div>

                    ) : (

                        <div className="admin-data-table-wrapper">

                            <table className="admin-data-table">

                                <thead>

                                    <tr>

                                        <th>
                                            Name
                                        </th>

                                        <th>
                                            Description
                                        </th>

                                        <th>
                                            Category ID
                                        </th>

                                        <th>
                                            Status
                                        </th>

                                    </tr>

                                </thead>

                                <tbody>

                                    {
                                        filteredCategories.map(
                                            category => (

                                                <tr
                                                    key={
                                                        category.id
                                                    }
                                                >

                                                    <td
                                                        data-label="Name"
                                                    >

                                                        <div className="admin-data-name">

                                                            <div className="admin-data-avatar category">

                                                                {
                                                                    category.name
                                                                        ?.charAt(0)
                                                                        .toUpperCase() ||
                                                                    'C'
                                                                }

                                                            </div>

                                                            <strong>
                                                                {
                                                                    category.name
                                                                }
                                                            </strong>

                                                        </div>

                                                    </td>

                                                    <td
                                                        data-label="Description"
                                                    >

                                                        <span className="admin-data-description">

                                                            {
                                                                category.description ||
                                                                'No description'
                                                            }

                                                        </span>

                                                    </td>

                                                    <td
                                                        data-label="Category ID"
                                                    >

                                                        <div className="admin-data-id">

                                                            <code>
                                                                {
                                                                    category.id
                                                                }
                                                            </code>

                                                            <button
                                                                type="button"
                                                                title="Copy Category ID"
                                                                onClick={() =>
                                                                    copyValue(
                                                                        category.id
                                                                    )
                                                                }
                                                            >
                                                                Copy
                                                            </button>

                                                        </div>

                                                    </td>

                                                    <td
                                                        data-label="Status"
                                                    >

                                                        <span
                                                            className={
                                                                `admin-data-status ${category.isActive
                                                                    ? 'active'
                                                                    : 'inactive'
                                                                }`
                                                            }
                                                        >

                                                            <span />

                                                            {
                                                                category.isActive
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

                    )}

                </section>

                {/* ==============================================
                    PRODUCT SECTION
                   ============================================== */}

                <section className="admin-data-section">

                    <div className="admin-data-section-header">

                        <div>

                            <span className="admin-data-section-label">
                                Section 02
                            </span>

                            <h2>
                                Products
                            </h2>

                            <p>
                                Search products by name,
                                SKU, category, ID, price,
                                stock or status.
                            </p>

                        </div>

                        <span className="admin-data-showing">

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
                        PRODUCT TOOLBAR
                       ========================================== */}

                    <div className="admin-data-toolbar">

                        <div className="admin-data-search">

                            <span className="admin-data-search-icon">
                                ⌕
                            </span>

                            <input
                                type="search"
                                placeholder="Search name, SKU, category..."
                                value={productSearch}
                                onChange={
                                    event =>
                                        setProductSearch(
                                            event.target.value
                                        )
                                }
                            />

                            {productSearch && (

                                <button
                                    type="button"
                                    title="Clear search"
                                    onClick={() =>
                                        setProductSearch('')
                                    }
                                >
                                    ×
                                </button>

                            )}

                        </div>

                        <select
                            value={productStatus}
                            onChange={
                                event =>
                                    setProductStatus(
                                        event.target.value
                                    )
                            }
                        >

                            <option value="all">
                                All Status
                            </option>

                            <option value="active">
                                Active
                            </option>

                            <option value="inactive">
                                Inactive
                            </option>

                        </select>

                    </div>

                    {/* ==========================================
                        PRODUCT ERROR
                       ========================================== */}

                    {productError && (

                        <div className="admin-data-error">

                            <div>

                                <strong>
                                    Unable to load products
                                </strong>

                                <span>
                                    {productError}
                                </span>

                            </div>

                            <button
                                type="button"
                                onClick={loadProducts}
                            >
                                Retry
                            </button>

                        </div>

                    )}

                    {/* ==========================================
                        PRODUCT CONTENT
                       ========================================== */}

                    {productsLoading ? (

                        <div className="admin-data-loading">

                            <div className="admin-data-spinner" />

                            <span>
                                Loading products...
                            </span>

                        </div>

                    ) : filteredProducts.length === 0 ? (

                        <div className="admin-data-empty">

                            <h3>
                                No products found
                            </h3>

                            <p>
                                Try changing your search
                                or status filter.
                            </p>

                        </div>

                    ) : (

                        <div className="admin-data-table-wrapper">

                            <table className="admin-data-table product-table">

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
                                            Category ID
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

                                    </tr>

                                </thead>

                                <tbody>

                                    {
                                        filteredProducts.map(
                                            product => (

                                                <tr
                                                    key={
                                                        product.id
                                                    }
                                                >

                                                    <td
                                                        data-label="Product"
                                                    >

                                                        <div className="admin-data-name">

                                                            <div className="admin-data-avatar product">

                                                                {
                                                                    product.name
                                                                        ?.charAt(0)
                                                                        .toUpperCase() ||
                                                                    'P'
                                                                }

                                                            </div>

                                                            <div>

                                                                <strong>
                                                                    {
                                                                        product.name
                                                                    }
                                                                </strong>

                                                                <span className="admin-data-product-description">

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

                                                        <div className="admin-data-sku">

                                                            <code>
                                                                {
                                                                    product.sku
                                                                }
                                                            </code>

                                                            <button
                                                                type="button"
                                                                title="Copy SKU"
                                                                onClick={() =>
                                                                    copyValue(
                                                                        product.sku
                                                                    )
                                                                }
                                                            >
                                                                Copy
                                                            </button>

                                                        </div>

                                                    </td>

                                                    <td
                                                        data-label="Category"
                                                    >

                                                        <span className="admin-data-category-pill">

                                                            {
                                                                getCategoryName(
                                                                    product.categoryId
                                                                )
                                                            }

                                                        </span>

                                                    </td>

                                                    <td
                                                        data-label="Category ID"
                                                    >

                                                        <div className="admin-data-id">

                                                            <code>
                                                                {
                                                                    product.categoryId
                                                                }
                                                            </code>

                                                            <button
                                                                type="button"
                                                                title="Copy Category ID"
                                                                onClick={() =>
                                                                    copyValue(
                                                                        product.categoryId
                                                                    )
                                                                }
                                                            >
                                                                Copy
                                                            </button>

                                                        </div>

                                                    </td>

                                                    <td
                                                        data-label="Price"
                                                    >

                                                        <strong className="admin-data-price">

                                                            ₹{
                                                                Number(
                                                                    product.price
                                                                ).toLocaleString(
                                                                    'en-IN'
                                                                )
                                                            }

                                                        </strong>

                                                    </td>

                                                    <td
                                                        data-label="Stock"
                                                    >

                                                        <span
                                                            className={
                                                                `admin-data-stock ${product.stockQuantity === 0
                                                                    ? 'empty'
                                                                    : product.stockQuantity <= 5
                                                                        ? 'low'
                                                                        : ''
                                                                }`
                                                            }
                                                        >

                                                            {
                                                                product.stockQuantity
                                                            }

                                                        </span>

                                                    </td>

                                                    <td
                                                        data-label="Status"
                                                    >

                                                        <span
                                                            className={
                                                                `admin-data-status ${product.isActive
                                                                    ? 'active'
                                                                    : 'inactive'
                                                                }`
                                                            }
                                                        >

                                                            <span />

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

                    )}

                </section>

            </div>

        </main>
    )
}

export default AdminDataPage