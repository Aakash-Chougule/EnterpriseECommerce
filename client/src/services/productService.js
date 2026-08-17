import apiClient from '../api/apiClient'

// ============================================================
// PRODUCT SERVICE
// ============================================================
//
// This service contains frontend functions related to products.
//
// React components should generally not contain all API
// communication logic directly.
//
// ProductsPage
//      ↓
// productService
//      ↓
// apiClient
//      ↓
// ASP.NET API
//
// ============================================================

export async function getProducts() {

    const response =
        await apiClient.get('/Products')

    return response.data
}