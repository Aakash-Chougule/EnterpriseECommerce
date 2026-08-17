import apiClient from '../api/apiClient'

// ============================================================
// ORDER SERVICE
// ============================================================
//
// Contains all frontend API calls related to customer orders.
//
// React Page
//     ↓
// orderService
//     ↓
// apiClient
//     ↓
// ASP.NET OrdersController
// ============================================================

// ------------------------------------------------------------
// CREATE ORDER
// ------------------------------------------------------------
//
// Backend:
// POST /api/Orders
//
// Request:
// {
//   shippingAddress: "..."
// }
//
// The backend uses the authenticated user's cart to create
// the order.
// ------------------------------------------------------------

export async function createOrder(
    shippingAddress
) {
    const response =
        await apiClient.post(
            '/Orders',
            {
                shippingAddress
            }
        )

    return response.data
}

// ------------------------------------------------------------
// GET CURRENT USER'S ORDERS
// ------------------------------------------------------------

export async function getOrders() {
    const response =
        await apiClient.get('/Orders')

    return response.data
}

// ------------------------------------------------------------
// GET ONE ORDER
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