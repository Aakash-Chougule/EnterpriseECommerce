import apiClient from '../api/apiClient'

// ============================================================
// CATEGORY SERVICE
// ============================================================

// ------------------------------------------------------------
// GET ACTIVE CATEGORIES
// ------------------------------------------------------------

export async function getCategories() {

    const response =
        await apiClient.get(
            '/Categories'
        )

    return response.data
}

// ------------------------------------------------------------
// GET CATEGORY BY ID
// ------------------------------------------------------------

export async function getCategoryById(
    categoryId
) {

    const response =
        await apiClient.get(
            `/Categories/${categoryId}`
        )

    return response.data
}

// ------------------------------------------------------------
// ADMIN - GET ALL CATEGORIES
// ------------------------------------------------------------
//
// Includes:
// - Active categories
// - Inactive categories
// ------------------------------------------------------------

export async function getAllCategoriesForAdmin() {

    const response =
        await apiClient.get(
            '/Categories/admin/all'
        )

    return response.data
}

// ------------------------------------------------------------
// CREATE CATEGORY
// ------------------------------------------------------------

export async function createCategory(
    name,
    description
) {

    const response =
        await apiClient.post(
            '/Categories',
            {
                name,
                description
            }
        )

    return response.data
}

// ------------------------------------------------------------
// UPDATE CATEGORY
// ------------------------------------------------------------

export async function updateCategory(
    categoryId,
    name,
    description
) {

    const response =
        await apiClient.put(
            `/Categories/${categoryId}`,
            {
                name,
                description
            }
        )

    return response.data
}

// ------------------------------------------------------------
// DEACTIVATE CATEGORY
// ------------------------------------------------------------

export async function deactivateCategory(
    categoryId
) {

    const response =
        await apiClient.delete(
            `/Categories/${categoryId}`
        )

    return response.data
}