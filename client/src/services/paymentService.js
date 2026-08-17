import apiClient from '../api/apiClient'

// ============================================================
// PAYMENT SERVICE
// ============================================================
//
// Handles frontend communication with PaymentsController.
//
// React Page
//     ↓
// paymentService
//     ↓
// apiClient
//     ↓
// ASP.NET PaymentsController
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
//
// Backend:
// GET /api/Payments/order/{orderId}
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

// ------------------------------------------------------------
// MARK PAYMENT SUCCESSFUL
// ------------------------------------------------------------
//
// Backend:
// POST /api/Payments/{paymentId}/success
//
// Request:
// {
//   transactionId: "..."
// }
//
// This is currently a testing/simulation endpoint.
// Later a real payment gateway such as Razorpay or Stripe
// should call the backend through a verified response/webhook.
// ------------------------------------------------------------

export async function markPaymentSuccessful(
    paymentId,
    transactionId
) {
    const response =
        await apiClient.post(
            `/Payments/${paymentId}/success`,
            {
                transactionId
            }
        )

    return response.data
}

// ------------------------------------------------------------
// MARK PAYMENT FAILED
// ------------------------------------------------------------
//
// Backend:
// POST /api/Payments/{paymentId}/fail
//
// Request:
// {
//   reason: "..."
// }
// ------------------------------------------------------------

export async function markPaymentFailed(
    paymentId,
    reason
) {
    const response =
        await apiClient.post(
            `/Payments/${paymentId}/fail`,
            {
                reason
            }
        )

    return response.data
}