import {
    useEffect,
    useState
} from 'react'

import {
    getCart,
    updateCartItemQuantity,
    removeItemFromCart,
    clearCart
} from '../services/cartService'

import {
    useNavigate
} from 'react-router-dom'

// ============================================================
// CART PAGE
// ============================================================
//
// Displays the authenticated user's shopping cart.
//
// Backend provides:
// - Product name
// - Unit price
// - Quantity
// - Line total
// - Complete cart total
//
// This page also allows:
// - Increasing quantity
// - Decreasing quantity
// - Removing an item
// - Clearing the complete cart
// ============================================================

function CartPage() {

    // ----------------------------------------------------------
    // CART STATE
    // ----------------------------------------------------------

    const [cart, setCart] =
        useState(null)

    const navigate =
        useNavigate()

    // ----------------------------------------------------------
    // LOADING STATE
    // ----------------------------------------------------------

    const [loading, setLoading] =
        useState(true)

    // ----------------------------------------------------------
    // ERROR STATE
    // ----------------------------------------------------------

    const [error, setError] =
        useState('')

    // ----------------------------------------------------------
    // UPDATING ITEM STATE
    // ----------------------------------------------------------
    //
    // Stores the ProductId currently being updated.
    //
    // This prevents repeated button clicks while the request
    // is being processed.
    // ----------------------------------------------------------

    const [
        updatingProductId,
        setUpdatingProductId
    ] = useState(null)

    // ==========================================================
    // LOAD CART
    // ==========================================================

    const loadCart = async () => {

        try {

            setLoading(true)

            setError('')

            const data =
                await getCart()

            console.log(
                'Cart received:',
                data
            )

            setCart(data)
        }
        catch (err) {

            console.error(
                'Failed to load cart:',
                err
            )

            const message =
                err.response?.data?.message ||
                'Unable to load cart.'

            setError(message)
        }
        finally {

            setLoading(false)
        }
    }

    // ==========================================================
    // UPDATE PRODUCT QUANTITY
    // ==========================================================
    //
    // Used by both the + and - buttons.
    //
    // Example:
    //
    // Current quantity = 2
    //
    // + button
    //     ↓
    // newQuantity = 3
    //
    // PUT /api/Cart/items/{productId}
    //
    // {
    //   quantity: 3
    // }
    //
    // The backend returns the updated cart.
    // ==========================================================

    const handleQuantityChange =
        async (
            productId,
            newQuantity
        ) => {

            // ------------------------------------------------------
            // Quantity cannot go below 1.
            //
            // If the user wants quantity 0, they should use
            // the Remove button instead.
            // ------------------------------------------------------

            if (newQuantity < 1) {
                return
            }

            try {

                setError('')

                setUpdatingProductId(
                    productId
                )

                const updatedCart =
                    await updateCartItemQuantity(
                        productId,
                        newQuantity
                    )

                // Replace current cart state with the latest
                // cart returned by the backend.

                setCart(updatedCart)
            }
            catch (err) {

                console.error(
                    'Failed to update quantity:',
                    err
                )

                const message =
                    err.response?.data?.message ||
                    'Unable to update quantity.'

                setError(message)
            }
            finally {

                setUpdatingProductId(null)
            }
        }

    // ==========================================================
    // REMOVE ITEM
    // ==========================================================

    const handleRemove =
        async (productId) => {

            try {

                setError('')

                setUpdatingProductId(
                    productId
                )

                const updatedCart =
                    await removeItemFromCart(
                        productId
                    )

                setCart(updatedCart)
            }
            catch (err) {

                console.error(
                    'Failed to remove product:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Unable to remove product.'
                )
            }
            finally {

                setUpdatingProductId(null)
            }
        }

    // ==========================================================
    // CLEAR CART
    // ==========================================================

    const handleClear =
        async () => {

            try {

                setError('')

                const updatedCart =
                    await clearCart()

                setCart(updatedCart)
            }
            catch (err) {

                console.error(
                    'Failed to clear cart:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Unable to clear cart.'
                )
            }
        }

    // ==========================================================
    // LOAD CART WHEN PAGE OPENS
    // ==========================================================
    //
    // [] means this runs once when CartPage first appears.
    // ==========================================================

    useEffect(() => {

        loadCart()

    }, [])

    // ==========================================================
    // LOADING UI
    // ==========================================================

    if (loading) {

        return (
            <div>

                <h1>
                    Shopping Cart
                </h1>

                <p>
                    Loading cart...
                </p>

            </div>
        )
    }

    // ----------------------------------------------------------
    // Safely read cart items.
    // ----------------------------------------------------------

    const items =
        cart?.items ?? []

    // ==========================================================
    // MAIN UI
    // ==========================================================

    return (
        <div>

            <h1>
                Shopping Cart
            </h1>

            {/* ======================================================
          ERROR MESSAGE
         ====================================================== */}

            {error && (
                <p>
                    {error}
                </p>
            )}

            {/* ======================================================
          EMPTY CART
         ====================================================== */}

            {items.length === 0 ? (

                <p>
                    Your cart is empty.
                </p>

            ) : (

                <>

                    {/* ==================================================
              CART ITEMS
             ================================================== */}

                    {items.map((item) => {

                        const isUpdating =
                            updatingProductId ===
                            item.productId

                        return (

                            <div key={item.id}>

                                {/* --------------------------------------------
                    Product Name
                   -------------------------------------------- */}

                                <h2>
                                    {item.productName}
                                </h2>

                                {/* --------------------------------------------
                    Unit Price
                   -------------------------------------------- */}

                                <p>

                                    <strong>
                                        Unit Price:
                                    </strong>

                                    {' '}

                                    ₹{Number(
                                        item.unitPrice
                                    ).toLocaleString(
                                        'en-IN'
                                    )}

                                </p>

                                {/* ============================================
                    QUANTITY CONTROLS
                   ============================================ */}

                                <div>

                                    <strong>
                                        Quantity:
                                    </strong>

                                    {' '}

                                    {/* ------------------------------------------
                      DECREASE QUANTITY
                     ------------------------------------------ */}

                                    <button
                                        type="button"

                                        disabled={
                                            item.quantity <= 1 ||
                                            isUpdating
                                        }

                                        onClick={() =>
                                            handleQuantityChange(
                                                item.productId,
                                                item.quantity - 1
                                            )
                                        }
                                    >
                                        -
                                    </button>

                                    {' '}

                                    <span>
                                        {item.quantity}
                                    </span>

                                    {' '}

                                    {/* ------------------------------------------
                      INCREASE QUANTITY
                     ------------------------------------------ */}

                                    <button
                                        type="button"

                                        disabled={
                                            isUpdating
                                        }

                                        onClick={() =>
                                            handleQuantityChange(
                                                item.productId,
                                                item.quantity + 1
                                            )
                                        }
                                    >
                                        +
                                    </button>

                                </div>

                                {/* --------------------------------------------
                    Subtotal
                   -------------------------------------------- */}

                                <p>

                                    <strong>
                                        Subtotal:
                                    </strong>

                                    {' '}

                                    ₹{Number(
                                        item.totalPrice
                                    ).toLocaleString(
                                        'en-IN'
                                    )}

                                </p>

                                {/* --------------------------------------------
                    Remove Product
                   -------------------------------------------- */}

                                <button
                                    type="button"

                                    disabled={
                                        isUpdating
                                    }

                                    onClick={() =>
                                        handleRemove(
                                            item.productId
                                        )
                                    }
                                >
                                    {isUpdating
                                        ? 'Updating...'
                                        : 'Remove'}
                                </button>

                                <hr />

                            </div>
                        )
                    })}

                    {/* ==================================================
              CART TOTAL
             ================================================== */}

                    <h2>

                        Total:

                        {' '}

                        ₹{Number(
                            cart?.totalAmount ?? 0
                        ).toLocaleString(
                            'en-IN'
                        )}

                    </h2>

                    {/* ==================================================
              CART ACTIONS
             ================================================== */}

                    <button
                        type="button"
                        onClick={handleClear}
                    >
                        Clear Cart
                        </button>

                        {' '}

                        <button
                            type="button"
                            onClick={() =>
                                navigate('/checkout')
                            }
                        >
                            Proceed to Checkout
                        </button>

                </>

            )}

        </div>
    )
}

export default CartPage