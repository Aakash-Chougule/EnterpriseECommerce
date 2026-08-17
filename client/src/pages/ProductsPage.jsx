import {
    useEffect,
    useState
} from 'react'

import {
    getProducts
} from '../services/productService'

import {
    addItemToCart
} from '../services/cartService'

import ProductCard from '../components/ProductCard'

// ============================================================
// PRODUCTS PAGE
// ============================================================
//
// This page:
// 1. Loads products from the ASP.NET API.
// 2. Displays them using ProductCard.
// 3. Allows the authenticated user to add products to the cart.
// ============================================================

function ProductsPage() {

    // ----------------------------------------------------------
    // PRODUCTS STATE
    // ----------------------------------------------------------
    //
    // Stores the list of products returned by the backend.
    // ----------------------------------------------------------

    const [products, setProducts] =
        useState([])

    // ----------------------------------------------------------
    // LOADING STATE
    // ----------------------------------------------------------
    //
    // Used while products are being loaded from the API.
    // ----------------------------------------------------------

    const [loading, setLoading] =
        useState(true)

    // ----------------------------------------------------------
    // ERROR STATE
    // ----------------------------------------------------------
    //
    // Stores an error message if an API request fails.
    // ----------------------------------------------------------

    const [error, setError] =
        useState('')

    // ----------------------------------------------------------
    // SUCCESS MESSAGE
    // ----------------------------------------------------------
    //
    // Example:
    // "Mechanical Keyboard added to cart."
    // ----------------------------------------------------------

    const [message, setMessage] =
        useState('')

    // ----------------------------------------------------------
    // ADDING PRODUCT STATE
    // ----------------------------------------------------------
    //
    // Stores the ID of the product currently being added.
    //
    // This allows only that product's button to show:
    //
    // Adding...
    // ----------------------------------------------------------

    const [
        addingProductId,
        setAddingProductId
    ] = useState(null)

    // ==========================================================
    // LOAD PRODUCTS
    // ==========================================================

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
                'Unable to load products.'
            )
        }
        finally {

            setLoading(false)
        }
    }

    // ==========================================================
    // ADD PRODUCT TO CART
    // ==========================================================
    //
    // This function is passed to ProductCard as a prop.
    //
    // When the user clicks "Add to Cart", ProductCard sends
    // the selected product back to this function.
    // ==========================================================

    const handleAddToCart =
        async (product) => {

            try {

                // Clear old messages.
                setMessage('')
                setError('')

                // Remember which product is being added.
                setAddingProductId(
                    product.id
                )

                // ------------------------------------------------------
                // Call ASP.NET Cart API
                // ------------------------------------------------------
                //
                // For now every click adds quantity = 1.
                //
                // Later we can add:
                // - Quantity selector
                // - Plus/minus buttons
                // - Stock validation in the UI
                // ------------------------------------------------------

                await addItemToCart(
                    product.id,
                    1
                )

                // Show success message.

                setMessage(
                    `${product.name} added to cart.`
                )
            }
            catch (err) {

                console.error(
                    'Failed to add product to cart:',
                    err
                )

                // Try to use the backend error message first.
                // If the backend does not return one, use our
                // frontend fallback message.

                const errorMessage =
                    err.response?.data?.message ||
                    'Unable to add product to cart.'

                setError(
                    errorMessage
                )
            }
            finally {

                // Re-enable the Add to Cart button.

                setAddingProductId(null)
            }
        }

    // ==========================================================
    // USE EFFECT
    // ==========================================================
    //
    // The empty [] means:
    //
    // Run loadProducts() once when ProductsPage first appears.
    // ==========================================================

    useEffect(() => {

        loadProducts()

    }, [])

    // ==========================================================
    // LOADING UI
    // ==========================================================

    if (loading) {

        return (
            <div>

                <h1>
                    Products
                </h1>

                <p>
                    Loading products...
                </p>

            </div>
        )
    }

    // ==========================================================
    // MAIN UI
    // ==========================================================

    return (
        <div>

            <h1>
                Products
            </h1>

            {/* ------------------------------------------------------
          Success message
         ------------------------------------------------------ */}

            {message && (
                <p>
                    {message}
                </p>
            )}

            {/* ------------------------------------------------------
          Error message
         ------------------------------------------------------ */}

            {error && (
                <p>
                    {error}
                </p>
            )}

            {/* ------------------------------------------------------
          Product list
         ------------------------------------------------------ */}

            {products.length === 0 ? (

                <p>
                    No products are currently available.
                </p>

            ) : (

                <div>

                    {products.map((product) => (

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

                    ))}

                </div>

            )}

        </div>
    )
}

export default ProductsPage