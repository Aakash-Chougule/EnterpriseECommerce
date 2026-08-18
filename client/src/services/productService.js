import apiClient from '../api/apiClient'

// ============================================================
// PRODUCT SERVICE
// ============================================================
//
// Handles customer and admin product API calls.
//
// Customer:
// - View active products
//
// Admin:
// - View all products
// - View low-stock products
// - Create product
// - Update product
// - Deactivate product
// ============================================================

// ------------------------------------------------------------
// GET ACTIVE PRODUCTS
// ------------------------------------------------------------
//
// GET /api/Products
// ------------------------------------------------------------

export async function getProducts() {

    const response =
        await apiClient.get(
            '/Products'
        )

    return response.data
}

// ------------------------------------------------------------
// GET PRODUCT BY ID
// ------------------------------------------------------------
//
// GET /api/Products/{id}
// ------------------------------------------------------------

export async function getProductById(
    productId
) {

    const response =
        await apiClient.get(
            `/Products/${productId}`
        )

    return response.data
}

// ------------------------------------------------------------
// ADMIN - GET ALL PRODUCTS
// ------------------------------------------------------------
//
// GET /api/Products/admin/all
//
// Includes:
// - Active products
// - Inactive products
// ------------------------------------------------------------

export async function getAllProductsForAdmin() {

    const response =
        await apiClient.get(
            '/Products/admin/all'
        )

    return response.data
}

// ------------------------------------------------------------
// ADMIN - GET LOW STOCK PRODUCTS
// ------------------------------------------------------------
//
// GET /api/Products/admin/low-stock
//
// Optional:
//
// GET /api/Products/admin/low-stock?threshold=10
//
// Default threshold is 5.
// ------------------------------------------------------------

export async function getLowStockProducts(
    threshold = 5
) {

    const response =
        await apiClient.get(
            '/Products/admin/low-stock',
            {
                params: {
                    threshold
                }
            }
        )

    return response.data
}

// ------------------------------------------------------------
// CREATE PRODUCT
// ------------------------------------------------------------
//
// POST /api/Products
//
// Admin only.
// ------------------------------------------------------------

export async function createProduct(
    productData
) {

    const response =
        await apiClient.post(
            '/Products',
            productData
        )

    return response.data
}

// ------------------------------------------------------------
// UPDATE PRODUCT
// ------------------------------------------------------------
//
// PUT /api/Products/{id}
//
// Admin only.
// ------------------------------------------------------------

export async function updateProduct(
    productId,
    productData
) {

    const response =
        await apiClient.put(
            `/Products/${productId}`,
            productData
        )

    return response.data
}

// ------------------------------------------------------------
// ADMIN - INCREASE PRODUCT STOCK
// ------------------------------------------------------------
//
// POST /api/Products/{id}/stock/increase
//
// Request:
// {
//   quantity: 5
// }
// ------------------------------------------------------------

export async function increaseProductStock(
    productId,
    quantity
) {

    const response =
        await apiClient.post(
            `/Products/${productId}/stock/increase`,
            {
                quantity
            }
        )

    return response.data
}

// ------------------------------------------------------------
// ADMIN - DECREASE PRODUCT STOCK
// ------------------------------------------------------------
//
// POST /api/Products/{id}/stock/decrease
// ------------------------------------------------------------

export async function decreaseProductStock(
    productId,
    quantity
) {

    const response =
        await apiClient.post(
            `/Products/${productId}/stock/decrease`,
            {
                quantity
            }
        )

    return response.data
}

// ------------------------------------------------------------
// DEACTIVATE PRODUCT
// ------------------------------------------------------------
//
// DELETE /api/Products/{id}
//
// Admin only.
//
// This is a soft delete.
// ------------------------------------------------------------

export async function deactivateProduct(
    productId
) {

    const response =
        await apiClient.delete(
            `/Products/${productId}`
        )

    return response.data
}