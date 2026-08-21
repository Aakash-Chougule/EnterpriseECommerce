import apiClient from '../api/apiClient'

// ============================================================
// CREATE INTERNAL PAYMENT
// ============================================================

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

// ============================================================
// GET PAYMENT
// ============================================================

export async function getPaymentByOrderId(
    orderId
) {
    const response =
        await apiClient.get(
            `/Payments/order/${orderId}`
        )

    return response.data
}

// ============================================================
// CREATE RAZORPAY ORDER
// ============================================================

export async function createRazorpayOrder(
    paymentId
) {
    const response =
        await apiClient.post(
            `/Payments/${paymentId}/razorpay-order`
        )

    return response.data
}

// ============================================================
// VERIFY RAZORPAY PAYMENT
// ============================================================

export async function verifyRazorpayPayment(
    paymentId,
    razorpayPaymentId,
    razorpayOrderId,
    razorpaySignature
) {
    const response =
        await apiClient.post(
            `/Payments/${paymentId}/razorpay-verify`,
            {
                razorpayPaymentId,
                razorpayOrderId,
                razorpaySignature
            }
        )

    return response.data
}

// ============================================================
// FAILED PAYMENT
// ============================================================

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