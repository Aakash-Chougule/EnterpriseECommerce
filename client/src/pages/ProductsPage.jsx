import {
    useEffect,
    useMemo,
    useState
} from 'react'

import {
    getProducts
} from '../services/productService'

import {
    addItemToCart
} from '../services/cartService'

import ProductCard from '../components/ProductCard'

import './ProductsPage.css'

// ============================================================
// PRODUCTS PAGE
// ============================================================

function ProductsPage() {

    const [products, setProducts] =
        useState([])

    const [loading, setLoading] =
        useState(true)

    const [error, setError] =
        useState('')

    const [message, setMessage] =
        useState('')

    const [
        addingProductId,
        setAddingProductId
    ] = useState(null)

    // ========================================================
    // SEARCH
    // ========================================================

    const [searchTerm, setSearchTerm] =
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

            console.log(
                'Products received:',
                data
            )

            setProducts(data)
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
    // ADD TO CART
    // ========================================================

    const handleAddToCart =
        async (product) => {

            try {

                setMessage('')
                setError('')

                setAddingProductId(
                    product.id
                )

                await addItemToCart(
                    product.id,
                    1
                )

                setMessage(
                    `${product.name} added to cart successfully.`
                )
            }
            catch (err) {

                console.error(
                    'Failed to add product to cart:',
                    err
                )

                const errorMessage =
                    err.response?.data?.message ||
                    'Unable to add product to cart.'

                setError(errorMessage)
            }
            finally {

                setAddingProductId(null)
            }
        }

    // ========================================================
    // FILTER PRODUCTS
    // ========================================================

    const filteredProducts =
        useMemo(() => {

            const search =
                searchTerm
                    .trim()
                    .toLowerCase()

            if (!search) {
                return products
            }

            return products.filter(
                (product) => {

                    const name =
                        product.name
                            ?.toLowerCase() ||
                        ''

                    const description =
                        product.description
                            ?.toLowerCase() ||
                        ''

                    const sku =
                        product.sku
                            ?.toLowerCase() ||
                        ''

                    return (
                        name.includes(search) ||
                        description.includes(search) ||
                        sku.includes(search)
                    )
                }
            )

        }, [products, searchTerm])

    // ========================================================
    // LOAD ON PAGE START
    // ========================================================

    useEffect(() => {

        loadProducts()

    }, [])

    // ========================================================
    // LOADING
    // ========================================================

    if (loading) {

        return (

            <main className="products-page">

                <div className="products-container">

                    <div className="products-loading">

                        <div className="products-spinner" />

                        <h2>
                            Loading products
                        </h2>

                        <p>
                            Please wait while we load
                            the latest products.
                        </p>

                    </div>

                </div>

            </main>
        )
    }

    // ========================================================
    // UI
    // ========================================================

    return (

        <main className="products-page">

            <div className="products-container">

                {/* ==================================================
                    PAGE HEADER
                   ================================================== */}

                <section className="products-header">

                    <div>

                        <span className="products-eyebrow">
                            Our Collection
                        </span>

                        <h1>
                            Explore Products
                        </h1>

                        <p>
                            Browse our available products
                            and add your favorites to the cart.
                        </p>

                    </div>

                    <div className="products-count">

                        <strong>
                            {products.length}
                        </strong>

                        <span>
                            {
                                products.length === 1
                                    ? 'Product'
                                    : 'Products'
                            }
                        </span>

                    </div>

                </section>

                {/* ==================================================
                    MESSAGES
                   ================================================== */}

                {message && (

                    <div className="products-alert products-alert-success">

                        <span className="products-alert-icon">
                            ✓
                        </span>

                        <span>
                            {message}
                        </span>

                        <button
                            type="button"
                            onClick={() =>
                                setMessage('')
                            }
                            aria-label="Close message"
                        >
                            ×
                        </button>

                    </div>

                )}

                {error && (

                    <div className="products-alert products-alert-error">

                        <span className="products-alert-icon">
                            !
                        </span>

                        <span>
                            {error}
                        </span>

                        <button
                            type="button"
                            onClick={() =>
                                setError('')
                            }
                            aria-label="Close error"
                        >
                            ×
                        </button>

                    </div>

                )}

                {/* ==================================================
                    TOOLBAR
                   ================================================== */}

                <section className="products-toolbar">

                    <div className="products-search">

                        <span className="products-search-icon">
                            ⌕
                        </span>

                        <input
                            type="search"
                            value={searchTerm}
                            placeholder="Search products by name, SKU..."
                            onChange={
                                (event) =>
                                    setSearchTerm(
                                        event.target.value
                                    )
                            }
                        />

                        {searchTerm && (

                            <button
                                type="button"
                                className="products-clear-search"
                                onClick={() =>
                                    setSearchTerm('')
                                }
                                aria-label="Clear search"
                            >
                                ×
                            </button>

                        )}

                    </div>

                    <div className="products-result-count">

                        Showing

                        {' '}

                        <strong>
                            {filteredProducts.length}
                        </strong>

                        {' '}

                        of

                        {' '}

                        {products.length}

                    </div>

                </section>

                {/* ==================================================
                    PRODUCTS
                   ================================================== */}

                {products.length === 0 ? (

                    <section className="products-empty">

                        <div className="products-empty-icon">
                            □
                        </div>

                        <h2>
                            No products available
                        </h2>

                        <p>
                            There are currently no products
                            available in the store.
                        </p>

                    </section>

                ) : filteredProducts.length === 0 ? (

                    <section className="products-empty">

                        <div className="products-empty-icon">
                            ⌕
                        </div>

                        <h2>
                            No matching products
                        </h2>

                        <p>
                            We couldn't find any products
                            matching "{searchTerm}".
                        </p>

                        <button
                            type="button"
                            className="products-reset-button"
                            onClick={() =>
                                setSearchTerm('')
                            }
                        >
                            Clear Search
                        </button>

                    </section>

                ) : (

                    <section className="products-grid">

                        {filteredProducts.map(
                            (product) => (

                                <ProductCard
                                    key={product.id}
                                    product={product}
                                    onAddToCart={
                                        handleAddToCart
                                    }
                                    adding={
                                        addingProductId ===
                                        product.id
                                    }
                                />

                            )
                        )}

                    </section>

                )}

            </div>

        </main>
    )
}

export default ProductsPage