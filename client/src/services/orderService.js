import apiClient from '../api/apiClient'

// ============================================================
// ORDER SERVICE
// ============================================================

// ------------------------------------------------------------
// CREATE ORDER
// ------------------------------------------------------------

export async function createOrder(
    shippingAddress,
    paymentMethod
) {
    const response =
        await apiClient.post(
            '/Orders',
            {
                shippingAddress,
                paymentMethod
            }
        )

    return response.data
}

// ------------------------------------------------------------
// GET CURRENT USER ORDERS
// ------------------------------------------------------------

export async function getOrders() {

    const response =
        await apiClient.get(
            '/Orders'
        )

    return response.data
}

// ------------------------------------------------------------
// GET ORDER BY ID
// ------------------------------------------------------------

export async function getOrderById(
    orderId
) {

    const response =
        await apiClient.get(
            `/Orders/${orderId}`
        )

    return response.data
}