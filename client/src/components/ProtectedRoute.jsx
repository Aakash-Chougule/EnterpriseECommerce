import { Navigate } from 'react-router-dom'

import { useAuth } from '../context/AuthContext'

// ============================================================
// PROTECTED ROUTE
// ============================================================
//
// This component protects pages that require authentication.
//
// If the user is authenticated:
//     → Display the requested page.
//
// If the user is NOT authenticated:
//     → Redirect to /login.
//
// Example:
//
// <ProtectedRoute>
//     <ProductsPage />
// </ProtectedRoute>
//
// ============================================================

function ProtectedRoute({ children }) {

    const { isAuthenticated } = useAuth()

    // ----------------------------------------------------------
    // User is not authenticated
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
    // User is authenticated
    // ----------------------------------------------------------

    return children
}

export default ProtectedRoute