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
    createOrder,
    getCheckoutPreview
} from '../services/orderService'

import {
    createPayment
} from '../services/paymentService'

import './CheckoutPage.css'

// ============================================================
// GST STATE CODES
// ============================================================

const INDIAN_STATES = [
    { name: 'Jammu and Kashmir', code: '01' },
    { name: 'Himachal Pradesh', code: '02' },
    { name: 'Punjab', code: '03' },
    { name: 'Chandigarh', code: '04' },
    { name: 'Uttarakhand', code: '05' },
    { name: 'Haryana', code: '06' },
    { name: 'Delhi', code: '07' },
    { name: 'Rajasthan', code: '08' },
    { name: 'Uttar Pradesh', code: '09' },
    { name: 'Bihar', code: '10' },
    { name: 'Sikkim', code: '11' },
    { name: 'Arunachal Pradesh', code: '12' },
    { name: 'Nagaland', code: '13' },
    { name: 'Manipur', code: '14' },
    { name: 'Mizoram', code: '15' },
    { name: 'Tripura', code: '16' },
    { name: 'Meghalaya', code: '17' },
    { name: 'Assam', code: '18' },
    { name: 'West Bengal', code: '19' },
    { name: 'Jharkhand', code: '20' },
    { name: 'Odisha', code: '21' },
    { name: 'Chhattisgarh', code: '22' },
    { name: 'Madhya Pradesh', code: '23' },
    { name: 'Gujarat', code: '24' },
    {
        name:
            'Dadra and Nagar Haveli and Daman and Diu',
        code:
            '26'
    },
    { name: 'Maharashtra', code: '27' },
    { name: 'Karnataka', code: '29' },
    { name: 'Goa', code: '30' },
    { name: 'Lakshadweep', code: '31' },
    { name: 'Kerala', code: '32' },
    { name: 'Tamil Nadu', code: '33' },
    { name: 'Puducherry', code: '34' },
    {
        name:
            'Andaman and Nicobar Islands',
        code:
            '35'
    },
    { name: 'Telangana', code: '36' },
    { name: 'Andhra Pradesh', code: '37' },
    { name: 'Ladakh', code: '38' }
]

// ============================================================
// PAGE
// ============================================================

function CheckoutPage() {

    const navigate =
        useNavigate()

    // ========================================================
    // CART
    // ========================================================

    const [
        cart,
        setCart
    ] = useState(null)

    // ========================================================
    // DELIVERY
    // ========================================================

    const [
        shippingAddress,
        setShippingAddress
    ] = useState('')

    const [
        shippingCity,
        setShippingCity
    ] = useState('')

    const [
        shippingState,
        setShippingState
    ] = useState('')

    const [
        shippingStateCode,
        setShippingStateCode
    ] = useState('')

    const [
        shippingPostalCode,
        setShippingPostalCode
    ] = useState('')

    // ========================================================
    // PAYMENT
    // ========================================================

    const [
        paymentMethod,
        setPaymentMethod
    ] = useState('UPI')

    // ========================================================
    // PRICE PREVIEW
    // ========================================================

    const [
        preview,
        setPreview
    ] = useState(null)

    const [
        previewLoading,
        setPreviewLoading
    ] = useState(false)

    // ========================================================
    // UI
    // ========================================================

    const [
        loading,
        setLoading
    ] = useState(true)

    const [
        processing,
        setProcessing
    ] = useState(false)

    const [
        error,
        setError
    ] = useState('')

    // ========================================================
    // LOAD CART
    // ========================================================

    const loadCart =
        async () => {

            try {

                setLoading(true)
                setError('')

                const data =
                    await getCart()

                setCart(
                    data
                )
            }
            catch (err) {

                console.error(
                    'Failed to load cart:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Unable to load checkout.'
                )
            }
            finally {

                setLoading(false)
            }
        }

    // ========================================================
    // LOAD CHECKOUT PREVIEW
    // ========================================================

    const loadCheckoutPreview =
        async (
            stateName,
            stateCode,
            postalCode
        ) => {

            if (
                !stateName ||
                !stateCode
            ) {
                setPreview(
                    null
                )

                return
            }

            try {

                setPreviewLoading(
                    true
                )

                setError('')

                const data =
                    await getCheckoutPreview(
                        stateName,
                        stateCode,
                        postalCode || null
                    )

                setPreview(
                    data
                )
            }
            catch (err) {

                console.error(
                    'Checkout preview failed:',
                    err
                )

                setPreview(
                    null
                )

                setError(
                    err.response?.data?.message ||
                    err.message ||
                    'Unable to calculate order total.'
                )
            }
            finally {

                setPreviewLoading(
                    false
                )
            }
        }

    // ========================================================
    // STATE CHANGE
    // ========================================================

    const handleStateChange =
        async (event) => {

            const selectedCode =
                event.target.value

            const selectedState =
                INDIAN_STATES.find(
                    state =>
                        state.code ===
                        selectedCode
                )

            const stateName =
                selectedState?.name ?? ''

            setShippingStateCode(
                selectedCode
            )

            setShippingState(
                stateName
            )

            // Remove the old price preview immediately
            // so a previous state's total is never shown.
            setPreview(
                null
            )

            if (
                stateName &&
                selectedCode
            ) {

                await loadCheckoutPreview(
                    stateName,
                    selectedCode,
                    shippingPostalCode
                )
            }
        }

    // ========================================================
    // POSTAL CODE CHANGE
    // ========================================================

    const handlePostalCodeChange =
        async (event) => {

            const value =
                event.target.value
                    .replace(
                        /\D/g,
                        ''
                    )

            setShippingPostalCode(
                value
            )

            // If the state is already selected and a full PIN
            // has been entered, refresh the server-side preview.
            if (
                value.length === 6 &&
                shippingState &&
                shippingStateCode
            ) {

                await loadCheckoutPreview(
                    shippingState,
                    shippingStateCode,
                    value
                )
            }
        }

    // ========================================================
    // VALIDATION
    // ========================================================

    const validateCheckout =
        () => {

            if (!shippingAddress.trim()) {

                setError(
                    'Shipping address is required.'
                )

                return false
            }

            if (!shippingCity.trim()) {

                setError(
                    'Shipping city is required.'
                )

                return false
            }

            if (!shippingPostalCode.trim()) {

                setError(
                    'PIN code is required.'
                )

                return false
            }

            if (
                !/^[1-9][0-9]{5}$/.test(
                    shippingPostalCode.trim()
                )
            ) {

                setError(
                    'Enter a valid 6-digit PIN code.'
                )

                return false
            }

            if (!shippingState) {

                setError(
                    'Shipping state is required.'
                )

                return false
            }

            if (!shippingStateCode) {

                setError(
                    'Shipping state code is required.'
                )

                return false
            }

            if (!paymentMethod) {

                setError(
                    'Payment method is required.'
                )

                return false
            }

            if (!preview) {

                setError(
                    'Please wait for GST, delivery charges and the final amount to be calculated.'
                )

                return false
            }

            return true
        }

    // ========================================================
    // PLACE ORDER
    // ========================================================

    const handlePlaceOrder =
        async (event) => {

            event.preventDefault()

            if (!validateCheckout()) {
                return
            }

            try {

                setProcessing(
                    true
                )

                setError('')

                // =============================================
                // CREATE ORDER
                // =============================================

                const order =
                    await createOrder(
                        {
                            shippingAddress:
                                shippingAddress.trim(),

                            shippingCity:
                                shippingCity.trim(),

                            shippingState,

                            shippingStateCode,

                            shippingPostalCode:
                                shippingPostalCode.trim(),

                            paymentMethod
                        }
                    )

                // =============================================
                // PRICE SAFETY CHECK
                // =============================================

                if (
                    Number(
                        order.totalAmount
                    ) !==
                    Number(
                        preview.totalAmount
                    )
                ) {

                    console.warn(
                        'Order amount changed after checkout preview.',
                        {
                            previewAmount:
                                preview.totalAmount,

                            orderAmount:
                                order.totalAmount
                        }
                    )
                }

                // =============================================
                // CREATE PAYMENT
                // =============================================

                await createPayment(
                    order.id,
                    paymentMethod
                )

                // =============================================
                // COD
                // =============================================

                if (
                    paymentMethod ===
                    'COD'
                ) {

                    navigate(
                        `/order-success/${order.id}`
                    )

                    return
                }

                // =============================================
                // ONLINE PAYMENT
                // =============================================

                navigate(
                    `/payment/${order.id}`
                )
            }
            catch (err) {

                console.error(
                    'Checkout failed:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    err.message ||
                    'Checkout failed.'
                )
            }
            finally {

                setProcessing(
                    false
                )
            }
        }

    // ========================================================
    // INITIAL LOAD
    // ========================================================

    useEffect(
        () => {

            loadCart()

        },
        []
    )

    // ========================================================
    // FORMAT PRICE
    // ========================================================

    const formatPrice =
        (price) =>
            Number(
                price ?? 0
            ).toLocaleString(
                'en-IN',
                {
                    minimumFractionDigits:
                        2,

                    maximumFractionDigits:
                        2
                }
            )

    // ========================================================
    // LOADING
    // ========================================================

    if (loading) {

        return (

            <main className="checkout-page">

                <div className="checkout-container">

                    <div className="checkout-loading">

                        <div className="checkout-spinner" />

                        <h2>
                            Loading checkout...
                        </h2>

                        <p>
                            Preparing your cart and pricing.
                        </p>

                    </div>

                </div>

            </main>
        )
    }

    const items =
        cart?.items ?? []

    // ========================================================
    // EMPTY CART
    // ========================================================

    if (
        items.length ===
        0
    ) {

        return (

            <main className="checkout-page">

                <div className="checkout-container">

                    <section className="checkout-empty">

                        <div className="checkout-empty-icon">
                            🛒
                        </div>

                        <h1>
                            Your cart is empty
                        </h1>

                        <p>
                            Add products before checkout.
                        </p>

                        <button
                            type="button"
                            onClick={() =>
                                navigate(
                                    '/products'
                                )
                            }
                        >
                            Browse Products
                        </button>

                    </section>

                </div>

            </main>
        )
    }

    const totalItems =
        items.reduce(
            (
                total,
                item
            ) =>
                total +
                item.quantity,
            0
        )

    // ========================================================
    // UI
    // ========================================================

    return (

        <main className="checkout-page">

            <div className="checkout-container">

                {/* =================================================
                    HEADER
                   ================================================= */}

                <header className="checkout-header">

                    <span className="checkout-eyebrow">
                        Secure Checkout
                    </span>

                    <h1>
                        Checkout
                    </h1>

                    <p>
                        Review product price,
                        included GST,
                        delivery charges and the
                        exact payable amount before payment.
                    </p>

                </header>

                {/* =================================================
                    ERROR
                   ================================================= */}

                {error && (

                    <div
                        className="checkout-alert"
                        role="alert"
                    >
                        {error}
                    </div>

                )}

                <div className="checkout-layout">

                    {/* =============================================
                        DELIVERY FORM
                       ============================================= */}

                    <section className="checkout-form-section">

                        <div className="checkout-card">

                            <div className="checkout-section-header">

                                <div className="checkout-step">
                                    1
                                </div>

                                <div>

                                    <h2>
                                        Delivery Information
                                    </h2>

                                    <p>
                                        Enter your complete
                                        delivery address.
                                    </p>

                                </div>

                            </div>

                            <form
                                className="checkout-form"
                                onSubmit={
                                    handlePlaceOrder
                                }
                            >

                                {/* =================================
                                    ADDRESS
                                   ================================= */}

                                <div className="checkout-field">

                                    <label htmlFor="shippingAddress">
                                        Address
                                    </label>

                                    <textarea
                                        id="shippingAddress"
                                        rows="4"
                                        value={
                                            shippingAddress
                                        }
                                        placeholder="House / Flat No., Building, Street, Area"
                                        onChange={
                                            event =>
                                                setShippingAddress(
                                                    event.target.value
                                                )
                                        }
                                    />

                                </div>

                                {/* =================================
                                    CITY + PIN
                                   ================================= */}

                                <div className="checkout-address-grid">

                                    <div className="checkout-field">

                                        <label htmlFor="shippingCity">
                                            City
                                        </label>

                                        <input
                                            id="shippingCity"
                                            type="text"
                                            value={
                                                shippingCity
                                            }
                                            placeholder="e.g. Kagal"
                                            onChange={
                                                event =>
                                                    setShippingCity(
                                                        event.target.value
                                                    )
                                            }
                                        />

                                    </div>

                                    <div className="checkout-field">

                                        <label htmlFor="shippingPostalCode">
                                            PIN Code
                                        </label>

                                        <input
                                            id="shippingPostalCode"
                                            type="text"
                                            inputMode="numeric"
                                            maxLength="6"
                                            value={
                                                shippingPostalCode
                                            }
                                            placeholder="416216"
                                            onChange={
                                                handlePostalCodeChange
                                            }
                                        />

                                    </div>

                                </div>

                                {/* =================================
                                    STATE
                                   ================================= */}

                                <div className="checkout-field">

                                    <label htmlFor="shippingState">
                                        State
                                    </label>

                                    <select
                                        id="shippingState"
                                        value={
                                            shippingStateCode
                                        }
                                        onChange={
                                            handleStateChange
                                        }
                                    >

                                        <option value="">
                                            Select State
                                        </option>

                                        {INDIAN_STATES.map(
                                            state => (

                                                <option
                                                    key={
                                                        state.code
                                                    }
                                                    value={
                                                        state.code
                                                    }
                                                >
                                                    {
                                                        state.name
                                                    }
                                                </option>

                                            )
                                        )}

                                    </select>

                                    <small>

                                        {shippingState
                                            ? `${shippingState} (${shippingStateCode}) selected for GST calculation.`
                                            : 'Select the delivery state to calculate GST and final pricing.'}

                                    </small>

                                </div>

                                {/* =================================
                                    PAYMENT
                                   ================================= */}

                                <div className="checkout-payment-section">

                                    <div className="checkout-section-header">

                                        <div className="checkout-step">
                                            2
                                        </div>

                                        <div>

                                            <h2>
                                                Payment Method
                                            </h2>

                                            <p>
                                                Choose your payment method.
                                            </p>

                                        </div>

                                    </div>

                                    <div className="checkout-field">

                                        <label htmlFor="paymentMethod">
                                            Payment Method
                                        </label>

                                        <select
                                            id="paymentMethod"
                                            value={
                                                paymentMethod
                                            }
                                            onChange={
                                                event =>
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

                                </div>

                                {/* =================================
                                    ACTIONS
                                   ================================= */}

                                <div className="checkout-form-actions">

                                    <button
                                        type="button"
                                        className="checkout-back-button"
                                        disabled={
                                            processing
                                        }
                                        onClick={() =>
                                            navigate(
                                                '/cart'
                                            )
                                        }
                                    >
                                        ← Back to Cart
                                    </button>

                                    <button
                                        type="submit"
                                        className="place-order-button"
                                        disabled={
                                            processing ||
                                            previewLoading ||
                                            !preview
                                        }
                                    >

                                        {processing
                                            ? 'Placing Order...'
                                            : previewLoading
                                                ? 'Calculating Total...'
                                                : preview
                                                    ? `Place Order • ₹${formatPrice(
                                                        preview.totalAmount
                                                    )}`
                                                    : 'Select State to Continue'}

                                    </button>

                                </div>

                            </form>

                        </div>

                    </section>

                    {/* =============================================
                        ORDER SUMMARY
                       ============================================= */}

                    <aside className="checkout-summary">

                        <div className="checkout-summary-header">

                            <span>
                                Price Details
                            </span>

                            <h2>
                                Order Summary
                            </h2>

                        </div>

                        {/* =========================================
                            PRODUCTS
                           ========================================= */}

                        <div className="checkout-products">

                            {items.map(
                                item => (

                                    <div
                                        className="checkout-product"
                                        key={
                                            item.id
                                        }
                                    >

                                        <div className="checkout-product-icon">

                                            {
                                                item.productName
                                                    ?.charAt(0)
                                                    .toUpperCase()
                                                || 'P'
                                            }

                                        </div>

                                        <div className="checkout-product-info">

                                            <h3>
                                                {
                                                    item.productName
                                                }
                                            </h3>

                                            <p>

                                                ₹{
                                                    formatPrice(
                                                        item.unitPrice
                                                    )
                                                }

                                                {' × '}

                                                {
                                                    item.quantity
                                                }

                                            </p>

                                        </div>

                                        <strong className="checkout-product-total">

                                            ₹{
                                                formatPrice(
                                                    item.totalPrice
                                                )
                                            }

                                        </strong>

                                    </div>

                                )
                            )}

                        </div>

                        <div className="checkout-summary-divider" />

                        {/* =========================================
                            PRODUCT PRICE
                           ========================================= */}

                        <div className="checkout-summary-row">

                            <span>

                                Price ({totalItems} {
                                    totalItems === 1
                                        ? 'item'
                                        : 'items'
                                })

                            </span>

                            <strong>

                                ₹{
                                    formatPrice(
                                        cart?.totalAmount
                                    )
                                }

                            </strong>

                        </div>

                        {/* =========================================
                            WAITING FOR STATE
                           ========================================= */}

                        {!shippingState &&
                            !previewLoading && (

                                <div className="checkout-select-state-note">

                                    <strong>
                                        Final amount not calculated yet
                                    </strong>

                                    <br />

                                    Select your delivery state
                                    to calculate GST,
                                    delivery charges and the
                                    exact payable amount.

                                </div>

                            )}

                        {/* =========================================
                            PREVIEW LOADING
                           ========================================= */}

                        {previewLoading && (

                            <div className="checkout-preview-loading">

                                <div className="checkout-mini-spinner" />

                                <span>
                                    Calculating GST,
                                    delivery and final total...
                                </span>

                            </div>

                        )}

                        {/* =========================================
                            FINANCIAL PREVIEW
                           ========================================= */}

                        {!previewLoading &&
                            preview && (
                                <>

                                    {/* =================================
                                    TAXABLE
                                   ================================= */}

                                    <div className="checkout-summary-row muted">

                                        <span>
                                            Taxable Value
                                        </span>

                                        <span>
                                            ₹{
                                                formatPrice(
                                                    preview.taxableAmount
                                                )
                                            }
                                        </span>

                                    </div>

                                    {/* =================================
                                    TOTAL GST
                                   ================================= */}

                                    <div className="checkout-summary-row gst-total">

                                        <span>
                                            Included GST
                                        </span>

                                        <span className="checkout-included-tax">

                                            ₹{
                                                formatPrice(
                                                    preview.totalGst
                                                )
                                            }

                                        </span>

                                    </div>

                                    {/* =================================
                                    INTRASTATE
                                   ================================= */}

                                    {!preview.isInterState && (

                                        <div className="checkout-tax-breakdown">

                                            <div>

                                                <span>
                                                    CGST
                                                </span>

                                                <span>
                                                    ₹{
                                                        formatPrice(
                                                            preview.totalCgst
                                                        )
                                                    }
                                                </span>

                                            </div>

                                            <div>

                                                <span>
                                                    SGST
                                                </span>

                                                <span>
                                                    ₹{
                                                        formatPrice(
                                                            preview.totalSgst
                                                        )
                                                    }
                                                </span>

                                            </div>

                                        </div>

                                    )}

                                    {/* =================================
                                    INTERSTATE
                                   ================================= */}

                                    {preview.isInterState && (

                                        <div className="checkout-tax-breakdown">

                                            <div>

                                                <span>
                                                    IGST
                                                </span>

                                                <span>
                                                    ₹{
                                                        formatPrice(
                                                            preview.totalIgst
                                                        )
                                                    }
                                                </span>

                                            </div>

                                        </div>

                                    )}

                                    {/* =================================
                                    SHIPPING
                                   ================================= */}

                                    <div className="checkout-summary-row">

                                        <span>
                                            Delivery Charges
                                        </span>

                                        {
                                            Number(
                                                preview.shippingCharge
                                            ) ===
                                                0
                                                ? (

                                                    <strong className="checkout-free-delivery">
                                                        FREE
                                                    </strong>

                                                )
                                                : (

                                                    <strong>

                                                        ₹{
                                                            formatPrice(
                                                                preview.shippingCharge
                                                            )
                                                        }

                                                    </strong>

                                                )
                                        }

                                    </div>

                                    {/* =================================
                                    DISCOUNT
                                   ================================= */}

                                    {Number(
                                        preview.discountAmount
                                    ) > 0 && (

                                            <div className="checkout-summary-row discount">

                                                <span>
                                                    Discount
                                                </span>

                                                <strong>

                                                    -₹{
                                                        formatPrice(
                                                            preview.discountAmount
                                                        )
                                                    }

                                                </strong>

                                            </div>

                                        )}

                                    {/* =================================
                                    TAX TYPE
                                   ================================= */}

                                    <div className="checkout-tax-type">

                                        {preview.isInterState
                                            ? `Interstate order • IGST applies`
                                            : `Intrastate order • CGST + SGST applies`}

                                    </div>

                                </>
                            )}

                        <div className="checkout-summary-divider" />

                        {/* =========================================
                            FINAL TOTAL
                           ========================================= */}

                        <div className="checkout-grand-total">

                            <span>
                                Total Amount
                            </span>

                            <strong>

                                {preview
                                    ? `₹${formatPrice(
                                        preview.totalAmount
                                    )}`
                                    : '—'}

                            </strong>

                        </div>

                        {/* =========================================
                            TRUST INFO
                           ========================================= */}

                        {preview && (

                            <div className="checkout-price-trust-note">

                                <span>
                                    ✓
                                </span>

                                <p>

                                    <strong>
                                        Exact payable amount:
                                        {' '}
                                        ₹{
                                            formatPrice(
                                                preview.totalAmount
                                            )
                                        }
                                    </strong>

                                    <br />

                                    GST shown above is
                                    already included in the
                                    product selling price.
                                    It is not added twice.

                                </p>

                            </div>

                        )}

                        <div className="checkout-secure-note">

                            <span>
                                🔒
                            </span>

                            <p>
                                GST, delivery charges and
                                final payable amount are
                                calculated securely by the server.
                            </p>

                        </div>

                    </aside>

                </div>

            </div>

        </main>
    )
}

export default CheckoutPage