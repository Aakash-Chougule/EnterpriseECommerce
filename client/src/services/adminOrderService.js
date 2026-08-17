import apiClient from '../api/apiClient'

// ============================================================
// ADMIN ORDER SERVICE
// ============================================================
//
// Handles communication with AdminOrdersController.
//
// All these endpoints require:
// Role = Admin
// ============================================================

// ------------------------------------------------------------
// GET ALL ORDERS
// ------------------------------------------------------------
//
// GET /api/admin/orders
// ------------------------------------------------------------

export async function getAdminOrders() {

    const response =
        await apiClient.get(
            '/admin/orders'
        )

    return response.data
}

// ------------------------------------------------------------
// CONFIRM ORDER
// ------------------------------------------------------------
//
// PUT /api/admin/orders/{id}/confirm
// ------------------------------------------------------------

export async function confirmOrder(orderId) {

    const response =
        await apiClient.put(
            `/admin/orders/${orderId}/confirm`
        )

    return response.data
}

// ------------------------------------------------------------
// START PROCESSING
// ------------------------------------------------------------

export async function startProcessingOrder(orderId) {

    const response =
        await apiClient.put(
            `/admin/orders/${orderId}/processing`
        )

    return response.data
}

// ------------------------------------------------------------
// SHIP ORDER
// ------------------------------------------------------------

export async function shipOrder(orderId) {

    const response =
        await apiClient.put(
            `/admin/orders/${orderId}/ship`
        )

    return response.data
}

// ------------------------------------------------------------
// DELIVER ORDER
// ------------------------------------------------------------

export async function deliverOrder(orderId) {

    const response =
        await apiClient.put(
            `/admin/orders/${orderId}/deliver`
        )

    return response.data
}

// ------------------------------------------------------------
// CANCEL ORDER
// ------------------------------------------------------------

export async function cancelOrder(orderId) {

    const response =
        await apiClient.put(
            `/admin/orders/${orderId}/cancel`
        )

    return response.data
}