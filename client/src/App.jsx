import {
    BrowserRouter,
    Routes,
    Route
} from 'react-router-dom'

import HomePage
    from './pages/HomePage'

import LoginPage
    from './pages/LoginPage'

import RegisterPage
    from './pages/RegisterPage'

import ProductsPage
    from './pages/ProductsPage'

import CartPage
    from './pages/CartPage'

import CheckoutPage
    from './pages/CheckoutPage'

import OrderSuccessPage
    from './pages/OrderSuccessPage'

import OrdersPage
    from './pages/OrdersPage'

import PaymentPage
    from './pages/PaymentPage'

import ProfilePage
    from './pages/ProfilePage'

// ============================================================
// ADMIN PAGES
// ============================================================

import AdminDashboardPage
    from './pages/admin/AdminDashboardPage'

import AdminProductsPage
    from './pages/admin/AdminProductsPage'

import AdminOrdersPage
    from './pages/admin/AdminOrdersPage'

import AdminCategoriesPage
    from './pages/admin/AdminCategoriesPage'

import AdminDataPage
    from './pages/admin/AdminDataPage'

import AdminInventoryPage
    from './pages/admin/AdminInventoryPage'

import AdminUsersPage
    from './pages/admin/AdminUsersPage'

import AdminAccessPage
    from './pages/admin/AdminAccessPage'

// ============================================================
// REPORTS
// ============================================================

import AdminReportsPage
    from './pages/admin/AdminReportsPage'

// ============================================================
// COMPONENTS
// ============================================================

import Navbar
    from './components/Navbar'

import ProtectedRoute
    from './components/ProtectedRoute'

// ============================================================
// APPLICATION
// ============================================================

function App() {

    return (

        <BrowserRouter>

            {/* ==================================================
                GLOBAL NAVBAR
               ================================================== */}

            <Navbar />

            {/* ==================================================
                ROUTES
               ================================================== */}

            <Routes>

                {/* ==============================================
                    PUBLIC
                   ============================================== */}

                <Route
                    path="/"
                    element={
                        <HomePage />
                    }
                />

                <Route
                    path="/login"
                    element={
                        <LoginPage />
                    }
                />

                <Route
                    path="/register"
                    element={
                        <RegisterPage />
                    }
                />

                {/* ==============================================
                    CUSTOMER
                   ============================================== */}

                <Route
                    path="/products"
                    element={
                        <ProtectedRoute>

                            <ProductsPage />

                        </ProtectedRoute>
                    }
                />

                <Route
                    path="/cart"
                    element={
                        <ProtectedRoute>

                            <CartPage />

                        </ProtectedRoute>
                    }
                />

                <Route
                    path="/checkout"
                    element={
                        <ProtectedRoute>

                            <CheckoutPage />

                        </ProtectedRoute>
                    }
                />

                <Route
                    path="/orders"
                    element={
                        <ProtectedRoute>

                            <OrdersPage />

                        </ProtectedRoute>
                    }
                />

                <Route
                    path="/profile"
                    element={
                        <ProtectedRoute>

                            <ProfilePage />

                        </ProtectedRoute>
                    }
                />

                {/* ==============================================
                    PAYMENT
                   ============================================== */}

                <Route
                    path="/payment/:orderId"
                    element={
                        <ProtectedRoute>

                            <PaymentPage />

                        </ProtectedRoute>
                    }
                />

                <Route
                    path="/order-success/:orderId"
                    element={
                        <ProtectedRoute>

                            <OrderSuccessPage />

                        </ProtectedRoute>
                    }
                />

                {/* ==============================================
                    ADMIN DASHBOARD
                   ============================================== */}

                <Route
                    path="/admin"
                    element={
                        <ProtectedRoute
                            requiredRole="Admin"
                        >

                            <AdminDashboardPage />

                        </ProtectedRoute>
                    }
                />

                {/* ==============================================
                    ADMIN PRODUCTS
                   ============================================== */}

                <Route
                    path="/admin/products"
                    element={
                        <ProtectedRoute
                            requiredRole="Admin"
                        >

                            <AdminProductsPage />

                        </ProtectedRoute>
                    }
                />

                {/* ==============================================
                    ADMIN CATEGORIES
                   ============================================== */}

                <Route
                    path="/admin/categories"
                    element={
                        <ProtectedRoute
                            requiredRole="Admin"
                        >

                            <AdminCategoriesPage />

                        </ProtectedRoute>
                    }
                />

                {/* ==============================================
                    ADMIN ORDERS
                   ============================================== */}

                <Route
                    path="/admin/orders"
                    element={
                        <ProtectedRoute
                            requiredRole="Admin"
                        >

                            <AdminOrdersPage />

                        </ProtectedRoute>
                    }
                />

                {/* ==============================================
                    ADMIN INVENTORY
                   ============================================== */}

                <Route
                    path="/admin/inventory"
                    element={
                        <ProtectedRoute
                            requiredRole="Admin"
                        >

                            <AdminInventoryPage />

                        </ProtectedRoute>
                    }
                />

                {/* ==============================================
                    REPORTS
                   ============================================== */}

                <Route
                    path="/admin/reports"
                    element={
                        <ProtectedRoute
                            requiredRole="Admin"
                        >

                            <AdminReportsPage />

                        </ProtectedRoute>
                    }
                />

                {/* ==============================================
                    ADMIN DATA
                   ============================================== */}

                <Route
                    path="/admin/data"
                    element={
                        <ProtectedRoute
                            requiredRole="Admin"
                        >

                            <AdminDataPage />

                        </ProtectedRoute>
                    }
                />

                {/* ==============================================
                    USERS & ADMINS
                   ============================================== */}

                <Route
                    path="/admin/users"
                    element={
                        <ProtectedRoute
                            requiredRole="Admin"
                        >

                            <AdminUsersPage />

                        </ProtectedRoute>
                    }
                />

                {/* ==============================================
                    MAIN ADMIN ACCESS
                   ============================================== */}

                <Route
                    path="/admin/access"
                    element={
                        <ProtectedRoute
                            requiredRole="Admin"
                        >

                            <AdminAccessPage />

                        </ProtectedRoute>
                    }
                />

            </Routes>

        </BrowserRouter>
    )
}

export default App