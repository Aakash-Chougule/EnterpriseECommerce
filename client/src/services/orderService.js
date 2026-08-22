import apiClient from '../api/apiClient'

// ============================================================
// ORDER SERVICE
// ============================================================

// ============================================================
// CHECKOUT PREVIEW
// ============================================================
//
// This does NOT create an order.
//
// It asks ASP.NET to calculate:
//
// - Taxable value
// - Included GST
// - CGST
// - SGST
// - IGST
// - Shipping
// - Discount
// - Final payable amount
//
// ============================================================

export async function getCheckoutPreview(
    shippingState,
    shippingStateCode,
    postalCode = null
) {
    if (!shippingState) {
        throw new Error(
            'Shipping state is required.'
        )
    }

    if (!shippingStateCode) {
        throw new Error(
            'Shipping state code is required.'
        )
    }

    const response =
        await apiClient.post(
            '/Orders/checkout-preview',
            {
                shippingState,
                shippingStateCode,
                postalCode
            }
        )

    return response.data
}

// ============================================================
// CREATE ORDER
// ============================================================

export async function createOrder(
    orderData
) {
    if (!orderData) {
        throw new Error(
            'Order information is required.'
        )
    }

    if (!orderData.shippingAddress?.trim()) {
        throw new Error(
            'Shipping address is required.'
        )
    }

    if (!orderData.shippingState?.trim()) {
        throw new Error(
            'Shipping state is required.'
        )
    }

    if (!orderData.shippingStateCode?.trim()) {
        throw new Error(
            'Shipping state code is required.'
        )
    }

    if (!orderData.shippingPostalCode?.trim()) {
        throw new Error(
            'PIN code is required.'
        )
    }

    if (!orderData.paymentMethod) {
        throw new Error(
            'Payment method is required.'
        )
    }

    // ========================================================
    // COMPLETE SAVED SHIPPING ADDRESS
    // ========================================================

    const addressParts = [
        orderData.shippingAddress?.trim(),
        orderData.shippingCity?.trim(),
        orderData.shippingState?.trim(),
        orderData.shippingPostalCode?.trim()
    ]
        .filter(Boolean)

    const completeShippingAddress =
        addressParts.join(', ')

    // ========================================================
    // API
    // ========================================================

    const response =
        await apiClient.post(
            '/Orders',
            {
                shippingAddress:
                    completeShippingAddress,

                shippingState:
                    orderData.shippingState.trim(),

                shippingStateCode:
                    orderData.shippingStateCode.trim(),

                postalCode:
                    orderData.shippingPostalCode.trim(),

                paymentMethod:
                    orderData.paymentMethod
            }
        )

    return response.data
}

// ============================================================
// GET CURRENT USER ORDERS
// ============================================================

export async function getOrders() {
    const response =
        await apiClient.get(
            '/Orders'
        )

    return response.data
}

// ============================================================
// GET ORDER BY ID
// ============================================================

export async function getOrderById(
    orderId
) {
    if (!orderId) {
        throw new Error(
            'Order ID is required.'
        )
    }

    const response =
        await apiClient.get(
            `/Orders/${orderId}`
        )

    return response.data
}