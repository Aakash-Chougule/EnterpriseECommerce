import apiClient from '../api/apiClient'

// ============================================================
// PAYMENT SERVICE
// ============================================================
//
// Handles frontend communication with PaymentsController.
// ============================================================

// ------------------------------------------------------------
// CREATE PAYMENT
// ------------------------------------------------------------
//
// Backend:
// POST /api/Payments
//
// Request:
// {
//   orderId: "...",
//   paymentMethod: "..."
// }
// ------------------------------------------------------------

export async function createPayment(
    orderId,
    paymentMethod
) {
    const response =
        await apiClient.post(
            '/Payments',
            {
                orderId,
                paymentMethod
            }
        )

    return response.data
}

// ------------------------------------------------------------
// GET PAYMENT BY ORDER
// ------------------------------------------------------------

export async function getPaymentByOrderId(
    orderId
) {
    const response =
        await apiClient.get(
            `/Payments/order/${orderId}`
        )

    return response.data
}