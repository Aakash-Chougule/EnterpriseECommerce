import apiClient from '../api/apiClient'

// ============================================================
// CART SERVICE
// ============================================================
//
// Contains all frontend API calls related to the shopping cart.
// Components should call these functions instead of calling
// Axios directly.
// ============================================================

// Get the current authenticated user's cart.
export async function getCart() {
    const response =
        await apiClient.get('/Cart')

    return response.data
}

// Add a product to the cart.
export async function addItemToCart(
    productId,
    quantity
) {
    const response =
        await apiClient.post(
            '/Cart/items',
            {
                productId,
                quantity
            }
        )

    return response.data
}

// Remove a product from the cart.
export async function removeItemFromCart(
    productId
) {
    const response =
        await apiClient.delete(
            `/Cart/items/${productId}`
        )

    return response.data
}

// Clear the entire cart.
export async function clearCart() {
    const response =
        await apiClient.delete('/Cart')

    return response.data
}