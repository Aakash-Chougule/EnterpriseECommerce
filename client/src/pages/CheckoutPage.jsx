import {
    useEffect,
    useState
} from 'react'

import {
    useNavigate
} from 'react-router-dom'

import {
    getCart
} from '../services/cartService'

import {
    createOrder
} from '../services/orderService'

import {
    createPayment
} from '../services/paymentService'

// ============================================================
// CHECKOUT PAGE
// ============================================================
//
// Checkout flow:
//
// 1. Load user's current cart.
// 2. User enters shipping address.
// 3. Create order.
// 4. Backend clears cart and reduces stock.
// 5. Create payment record.
// 6. Navigate to order confirmation page later.
// ============================================================

function CheckoutPage() {

    // ----------------------------------------------------------
    // ROUTER
    // ----------------------------------------------------------

    const navigate =
        useNavigate()

    // ----------------------------------------------------------
    // CART
    // ----------------------------------------------------------

    const [cart, setCart] =
        useState(null)

    // ----------------------------------------------------------
    // SHIPPING ADDRESS
    // ----------------------------------------------------------

    const [
        shippingAddress,
        setShippingAddress
    ] = useState('')

    // ----------------------------------------------------------
    // PAYMENT METHOD
    // ----------------------------------------------------------
    //
    // For now this is only stored in our backend.
    //
    // Later Razorpay / Stripe will replace this simple test flow.
    // ----------------------------------------------------------

    const [
        paymentMethod,
        setPaymentMethod
    ] = useState('UPI')

    // ----------------------------------------------------------
    // UI STATE
    // ----------------------------------------------------------

    const [loading, setLoading] =
        useState(true)

    const [processing, setProcessing] =
        useState(false)

    const [error, setError] =
        useState('')

    // ==========================================================
    // LOAD CART
    // ==========================================================

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
                'Failed to load checkout cart:',
                err
            )

            setError(
                err.response?.data?.message ||
                'Unable to load checkout information.'
            )
        }
        finally {

            setLoading(false)
        }
    }

    // ==========================================================
    // PLACE ORDER
    // ==========================================================

    const handlePlaceOrder =
        async (event) => {

            // Prevent normal HTML form submission.
            event.preventDefault()

            // ------------------------------------------------------
            // Frontend validation
            // ------------------------------------------------------

            if (!shippingAddress.trim()) {

                setError(
                    'Shipping address is required.'
                )

                return
            }

            if (!paymentMethod) {

                setError(
                    'Please select a payment method.'
                )

                return
            }

            try {

                setProcessing(true)
                setError('')

                // ====================================================
                // STEP 1 — CREATE ORDER
                // ====================================================
                //
                // ASP.NET:
                //
                // POST /api/Orders
                //
                // Backend will:
                // - Validate stock
                // - Reduce stock
                // - Create Order
                // - Create OrderItems
                // - Clear cart
                // - Publish Kafka OrderCreatedEvent
                // ====================================================

                const order =
                    await createOrder(
                        shippingAddress.trim()
                    )

                console.log(
                    'Order created:',
                    order
                )

                // ====================================================
                // STEP 2 — CREATE PAYMENT
                // ====================================================

                const payment =
                    await createPayment(
                        order.id,
                        paymentMethod
                    )

                console.log(
                    'Payment created:',
                    payment
                )

                // ====================================================
                // STEP 3 — NAVIGATE
                // ====================================================
                //
                // Pass IDs through URL.
                //
                // Later our OrderSuccessPage will load complete details
                // from the API.
                // ====================================================

                navigate(
                    `/payment/${order.id}`
                )
            }
            catch (err) {

                console.error(
                    'Checkout failed:',
                    err
                )

                const message =
                    err.response?.data?.message ||
                    'Checkout failed. Please try again.'

                setError(message)
            }
            finally {

                setProcessing(false)
            }
        }

    // ==========================================================
    // INITIAL LOAD
    // ==========================================================

    useEffect(() => {

        loadCart()

    }, [])

    // ==========================================================
    // LOADING
    // ==========================================================

    if (loading) {

        return (
            <div>

                <h1>
                    Checkout
                </h1>

                <p>
                    Loading checkout...
                </p>

            </div>
        )
    }

    const items =
        cart?.items ?? []

    // ==========================================================
    // EMPTY CART
    // ==========================================================

    if (items.length === 0) {

        return (
            <div>

                <h1>
                    Checkout
                </h1>

                <p>
                    Your cart is empty.
                </p>

                <button
                    type="button"
                    onClick={() =>
                        navigate('/products')
                    }
                >
                    Browse Products
                </button>

            </div>
        )
    }

    // ==========================================================
    // MAIN UI
    // ==========================================================

    return (
        <div>

            <h1>
                Checkout
            </h1>

            {/* ====================================================
          ORDER SUMMARY
         ==================================================== */}

            <section>

                <h2>
                    Order Summary
                </h2>

                {items.map((item) => (

                    <div key={item.id}>

                        <h3>
                            {item.productName}
                        </h3>

                        <p>
                            Price:
                            {' '}
                            ₹{Number(
                                item.unitPrice
                            ).toLocaleString(
                                'en-IN'
                            )}
                        </p>

                        <p>
                            Quantity:
                            {' '}
                            {item.quantity}
                        </p>

                        <p>
                            Subtotal:
                            {' '}
                            ₹{Number(
                                item.totalPrice
                            ).toLocaleString(
                                'en-IN'
                            )}
                        </p>

                        <hr />

                    </div>

                ))}

                <h2>

                    Total:

                    {' '}

                    ₹{Number(
                        cart?.totalAmount ?? 0
                    ).toLocaleString(
                        'en-IN'
                    )}

                </h2>

            </section>

            {/* ====================================================
          CHECKOUT FORM
         ==================================================== */}

            <form
                onSubmit={handlePlaceOrder}
            >

                {/* --------------------------------------------------
            SHIPPING ADDRESS
           -------------------------------------------------- */}

                <div>

                    <label
                        htmlFor="shippingAddress"
                    >
                        Shipping Address
                    </label>

                    <br />

                    <textarea
                        id="shippingAddress"
                        rows="5"

                        value={
                            shippingAddress
                        }

                        placeholder={
                            'Enter complete delivery address'
                        }

                        onChange={(event) =>
                            setShippingAddress(
                                event.target.value
                            )
                        }
                    />

                </div>

                <br />

                {/* --------------------------------------------------
            PAYMENT METHOD
           -------------------------------------------------- */}

                <div>

                    <label
                        htmlFor="paymentMethod"
                    >
                        Payment Method
                    </label>

                    <br />

                    <select
                        id="paymentMethod"

                        value={
                            paymentMethod
                        }

                        onChange={(event) =>
                            setPaymentMethod(
                                event.target.value
                            )
                        }
                    >

                        <option value="UPI">
                            UPI
                        </option>

                        <option value="Card">
                            Debit / Credit Card
                        </option>

                        <option value="NetBanking">
                            Net Banking
                        </option>

                        <option value="COD">
                            Cash on Delivery
                        </option>

                    </select>

                </div>

                <br />

                {/* --------------------------------------------------
            ERROR
           -------------------------------------------------- */}

                {error && (
                    <p>
                        {error}
                    </p>
                )}

                {/* --------------------------------------------------
            SUBMIT
           -------------------------------------------------- */}

                <button
                    type="submit"
                    disabled={processing}
                >
                    {
                        processing
                            ? 'Placing Order...'
                            : 'Place Order'
                    }
                </button>

            </form>

        </div>
    )
}

export default CheckoutPage