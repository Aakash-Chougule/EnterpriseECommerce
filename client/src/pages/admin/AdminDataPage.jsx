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

// ============================================================
// ADMIN DATA PAGE
// ============================================================
//
// Displays:
// 1. Categories table
// 2. Products table
//
// Each section has a simple Ctrl+F-style search.
//
// IMPORTANT:
// Categories and Products are loaded independently.
// If one API fails, the other table can still work.
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

    const [categorySearch, setCategorySearch] =
        useState('')

    const [productSearch, setProductSearch] =
        useState('')

    // ========================================================
    // LOADING
    // ========================================================

    const [categoriesLoading, setCategoriesLoading] =
        useState(true)

    const [productsLoading, setProductsLoading] =
        useState(true)

    // ========================================================
    // ERRORS
    // ========================================================

    const [categoryError, setCategoryError] =
        useState('')

    const [productError, setProductError] =
        useState('')

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

    const getCategoryName = (categoryId) => {

        const category =
            categories.find(
                category =>
                    category.id === categoryId
            )

        return category?.name ||
            'Unknown Category'
    }

    // ========================================================
    // CATEGORY SEARCH
    // ========================================================

    const filteredCategories =
        useMemo(() => {

            const search =
                categorySearch
                    .trim()
                    .toLowerCase()

            if (!search) {
                return categories
            }

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

                    return searchableText.includes(
                        search
                    )
                }
            )

        }, [
            categories,
            categorySearch
        ])

    // ========================================================
    // PRODUCT SEARCH
    // ========================================================

    const filteredProducts =
        useMemo(() => {

            const search =
                productSearch
                    .trim()
                    .toLowerCase()

            if (!search) {
                return products
            }

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

                    return searchableText.includes(
                        search
                    )
                }
            )

        }, [
            products,
            categories,
            productSearch
        ])

    return (

        <div>

            <h1>
                Admin Data / Archive
            </h1>

            <p>
                View active and inactive categories
                and products.
            </p>

            {/* ==================================================
                CATEGORY SECTION
               ================================================== */}

            <section>

                <h2>
                    Categories
                </h2>

                {/* Ctrl+F style search */}

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

                {' '}

                {categorySearch && (
                    <button
                        type="button"
                        onClick={
                            () =>
                                setCategorySearch('')
                        }
                    >
                        Clear
                    </button>
                )}

                <p>
                    Showing{' '}
                    <strong>
                        {filteredCategories.length}
                    </strong>
                    {' '}of{' '}
                    <strong>
                        {categories.length}
                    </strong>
                    {' '}categories
                </p>

                {categoryError && (

                    <div>

                        <p>
                            {categoryError}
                        </p>

                        <button
                            type="button"
                            onClick={loadCategories}
                        >
                            Retry
                        </button>

                    </div>

                )}

                {categoriesLoading ? (

                    <p>
                        Loading categories...
                    </p>

                ) : filteredCategories.length === 0 ? (

                    <p>
                        No categories found.
                    </p>

                ) : (

                    <table
                        border="1"
                        cellPadding="8"
                        cellSpacing="0"
                    >

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

                            {filteredCategories.map(
                                category => (

                                    <tr
                                        key={category.id}
                                    >

                                        <td>
                                            {category.name}
                                        </td>

                                        <td>
                                            {
                                                category.description ||
                                                'No description'
                                            }
                                        </td>

                                        <td>
                                            {category.id}
                                        </td>

                                        <td>

                                            <strong>
                                                {
                                                    category.isActive
                                                        ? 'Active'
                                                        : 'Inactive'
                                                }
                                            </strong>

                                        </td>

                                    </tr>

                                )
                            )}

                        </tbody>

                    </table>

                )}

            </section>

            <br />

            <hr />

            <br />

            {/* ==================================================
                PRODUCT SECTION
               ================================================== */}

            <section>

                <h2>
                    Products
                </h2>

                {/* Ctrl+F style search */}

                <input
                    type="search"
                    placeholder="Search products..."
                    value={productSearch}
                    onChange={
                        event =>
                            setProductSearch(
                                event.target.value
                            )
                    }
                />

                {' '}

                {productSearch && (

                    <button
                        type="button"
                        onClick={
                            () =>
                                setProductSearch('')
                        }
                    >
                        Clear
                    </button>

                )}

                <p>
                    Showing{' '}
                    <strong>
                        {filteredProducts.length}
                    </strong>
                    {' '}of{' '}
                    <strong>
                        {products.length}
                    </strong>
                    {' '}products
                </p>

                {productError && (

                    <div>

                        <p>
                            {productError}
                        </p>

                        <button
                            type="button"
                            onClick={loadProducts}
                        >
                            Retry
                        </button>

                    </div>

                )}

                {productsLoading ? (

                    <p>
                        Loading products...
                    </p>

                ) : filteredProducts.length === 0 ? (

                    <p>
                        No products found.
                    </p>

                ) : (

                    <table
                        border="1"
                        cellPadding="8"
                        cellSpacing="0"
                    >

                        <thead>

                            <tr>

                                <th>
                                    Name
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

                            {filteredProducts.map(
                                product => (

                                    <tr
                                        key={product.id}
                                    >

                                        <td>
                                            {product.name}
                                        </td>

                                        <td>
                                            <strong>
                                                {product.sku}
                                            </strong>
                                        </td>

                                        <td>
                                            {
                                                getCategoryName(
                                                    product.categoryId
                                                )
                                            }
                                        </td>

                                        <td>
                                            {product.categoryId}
                                        </td>

                                        <td>
                                            ₹{
                                                Number(
                                                    product.price
                                                ).toLocaleString(
                                                    'en-IN'
                                                )
                                            }
                                        </td>

                                        <td>
                                            {
                                                product.stockQuantity
                                            }
                                        </td>

                                        <td>

                                            <strong>
                                                {
                                                    product.isActive
                                                        ? 'Active'
                                                        : 'Inactive'
                                                }
                                            </strong>

                                        </td>

                                    </tr>

                                )
                            )}

                        </tbody>

                    </table>

                )}

            </section>

        </div>
    )
}

export default AdminDataPage