import {
    useEffect,
    useState
} from 'react'

import {
    getCart,
    removeItemFromCart,
    clearCart
} from '../services/cartService'

// ============================================================
// CART PAGE
// ============================================================

function CartPage() {
    const [cart, setCart] =
        useState(null)

    const [loading, setLoading] =
        useState(true)

    const [error, setError] =
        useState('')

    // ----------------------------------------------------------
    // Load current user's cart
    // ----------------------------------------------------------

    const loadCart = async () => {
        try {
            setLoading(true)
            setError('')

            const data =
                await getCart()

            setCart(data)
        }
        catch (err) {
            console.error(
                'Failed to load cart:',
                err
            )

            setError(
                'Unable to load cart.'
            )
        }
        finally {
            setLoading(false)
        }
    }

    // ----------------------------------------------------------
    // Remove one product completely from cart
    // ----------------------------------------------------------

    const handleRemove =
        async (productId) => {

            try {
                const updatedCart =
                    await removeItemFromCart(
                        productId
                    )

                setCart(updatedCart)
            }
            catch (err) {
                console.error(
                    'Failed to remove item:',
                    err
                )

                setError(
                    'Unable to remove item.'
                )
            }
        }

    // ----------------------------------------------------------
    // Clear full cart
    // ----------------------------------------------------------

    const handleClear =
        async () => {

            try {
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
                    'Unable to clear cart.'
                )
            }
        }

    useEffect(() => {
        loadCart()
    }, [])

    if (loading) {
        return (
            <div>
                <h1>Cart</h1>
                <p>Loading cart...</p>
            </div>
        )
    }

    if (error) {
        return (
            <div>
                <h1>Cart</h1>
                <p>{error}</p>

                <button
                    type="button"
                    onClick={loadCart}
                >
                    Try Again
                </button>
            </div>
        )
    }

    const items =
        cart?.items ?? []

    return (
        <div>

            <h1>Shopping Cart</h1>

            {items.length === 0 ? (
                <p>
                    Your cart is empty.
                </p>
            ) : (
                <>
                    {items.map((item) => (
                        <div key={item.id}>

                            <p>
                                <strong>
                                    Product ID:
                                </strong>
                                {' '}
                                {item.productId}
                            </p>

                            <p>
                                <strong>
                                    Quantity:
                                </strong>
                                {' '}
                                {item.quantity}
                            </p>

                            <button
                                type="button"
                                onClick={() =>
                                    handleRemove(
                                        item.productId
                                    )
                                }
                            >
                                Remove
                            </button>

                            <hr />

                        </div>
                    ))}

                    <button
                        type="button"
                        onClick={handleClear}
                    >
                        Clear Cart
                    </button>
                </>
            )}

        </div>
    )
}

export default CartPage