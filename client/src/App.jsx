import {
    BrowserRouter,
    Routes,
    Route
} from 'react-router-dom'

import HomePage from './pages/HomePage'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import ProductsPage from './pages/ProductsPage'
import CartPage from './pages/CartPage'
import CheckoutPage from './pages/CheckoutPage'
import OrderSuccessPage from './pages/OrderSuccessPage'
import OrdersPage from './pages/OrdersPage'
import PaymentPage from './pages/PaymentPage'
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

import Navbar from './components/Navbar'
import ProtectedRoute from './components/ProtectedRoute'

function App() {

    return (
        <BrowserRouter>

            {/* ======================================================
          GLOBAL NAVIGATION
         ====================================================== */}

            <Navbar />

            {/* ======================================================
          APPLICATION ROUTES
         ====================================================== */}

            <Routes>

                {/* ====================================================
            PUBLIC ROUTES
           ==================================================== */}

                <Route
                    path="/"
                    element={<HomePage />}
                />

                <Route
                    path="/login"
                    element={<LoginPage />}
                />

                <Route
                    path="/register"
                    element={<RegisterPage />}
                />

                {/* ====================================================
            PROTECTED ROUTES
           ==================================================== */}

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

                <Route
                    path="/admin"
                    element={
                        <ProtectedRoute requiredRole="Admin">
                            <AdminDashboardPage />
                        </ProtectedRoute>
                    }
                />

                <Route
                    path="/admin/products"
                    element={
                        <ProtectedRoute requiredRole="Admin">
                            <AdminProductsPage />
                        </ProtectedRoute>
                    }
                />

                <Route
                    path="/admin/orders"
                    element={
                        <ProtectedRoute requiredRole="Admin">
                            <AdminOrdersPage />
                        </ProtectedRoute>
                    }
                />

                <Route
                    path="/admin/categories"
                    element={
                        <ProtectedRoute requiredRole="Admin">
                            <AdminCategoriesPage />
                        </ProtectedRoute>
                    }
                />

                <Route
                    path="/admin/data"
                    element={
                        <ProtectedRoute requiredRole="Admin">
                            <AdminDataPage />
                        </ProtectedRoute>
                    }
                />

            </Routes>

        </BrowserRouter>
    )
}

export default App