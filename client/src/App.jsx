import {
    BrowserRouter,
    Routes,
    Route,
    Link
} from 'react-router-dom'

import HomePage from './pages/HomePage'
import LoginPage from './pages/LoginPage'
import ProductsPage from './pages/ProductsPage'
import CartPage from './pages/CartPage'
import CheckoutPage from './pages/CheckoutPage'
import OrderSuccessPage from './pages/OrderSuccessPage'
import OrdersPage from './pages/OrdersPage'

import ProtectedRoute from './components/ProtectedRoute'
import { useAuth } from './context/AuthContext'

function App() {

    // Authentication information is available globally
    // because App is wrapped inside AuthProvider.
    const {
        user,
        isAuthenticated,
        logout
    } = useAuth()

    return (
        <BrowserRouter>

            {/* ======================================================
          TEMPORARY NAVIGATION

          Later we will replace this with a professional Navbar
          component and proper styling.
         ====================================================== */}

            <nav>

                <Link to="/">
                    Home
                </Link>

                {' | '}

                <Link to="/products">
                    Products
                </Link>

             

                {' | '}

                <Link to="/cart">
                    Cart
                </Link>

                {' | '}

                <Link to="/orders">
                    My Orders
                </Link>



                {/* -----------------------------------------------
            Show different navigation depending on whether
            the user is logged in.
           ----------------------------------------------- */}

                {isAuthenticated ? (
                    <>
                        <span>
                            Welcome {user?.firstName}
                        </span>

                        {' | '}

                        <button
                            type="button"
                            onClick={logout}
                        >
                            Logout
                        </button>
                    </>
                ) : (
                    <Link to="/login">
                        Login
                    </Link>
                )}

            </nav>

            {/* ======================================================
          APPLICATION ROUTES
         ====================================================== */}

            <Routes>

                {/* Public route */}

                <Route
                    path="/"
                    element={<HomePage />}
                />

                {/* Public route */}

                <Route
                    path="/login"
                    element={<LoginPage />}
                />

                {/* Protected route */}

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
                    path="/order-success/:orderId"
                    element={
                        <ProtectedRoute>
                            <OrderSuccessPage />
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

            </Routes>

        </BrowserRouter>
    )
}

export default App