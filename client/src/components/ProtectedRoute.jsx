import { Navigate } from 'react-router-dom'

import { useAuth } from '../context/AuthContext'

// ============================================================
// PROTECTED ROUTE
// ============================================================
//
// This component protects pages that require authentication.
//
// It can also optionally require a specific user role.
//
// Examples:
//
// Authentication only:
//
// <ProtectedRoute>
//   <ProductsPage />
// </ProtectedRoute>
//
// Admin only:
//
// <ProtectedRoute requiredRole="Admin">
//   <AdminDashboardPage />
// </ProtectedRoute>
//
// ============================================================

function ProtectedRoute({
    children,
    requiredRole
}) {

    const {
        isAuthenticated,
        user
    } = useAuth()

    // ----------------------------------------------------------
    // NOT LOGGED IN
    // ----------------------------------------------------------
    //
    // If the user is not authenticated, redirect to login.
    // ----------------------------------------------------------

    if (!isAuthenticated) {
        return (
            <Navigate
                to="/login"
                replace
            />
        )
    }

    // ----------------------------------------------------------
    // ROLE CHECK
    // ----------------------------------------------------------
    //
    // If requiredRole was provided, the logged-in user's role
    // must match it.
    //
    // Example:
    //
    // requiredRole = "Admin"
    // user.role     = "Customer"
    //
    // Result:
    // Redirect to home.
    // ----------------------------------------------------------

    if (
        requiredRole &&
        user?.role !== requiredRole
    ) {
        return (
            <Navigate
                to="/"
                replace
            />
        )
    }

    // ----------------------------------------------------------
    // ACCESS GRANTED
    // ----------------------------------------------------------

    return children
}

export default ProtectedRoute