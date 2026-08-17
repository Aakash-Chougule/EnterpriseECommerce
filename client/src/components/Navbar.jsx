import {
    Link,
    useNavigate
} from 'react-router-dom'

import {
    useAuth
} from '../context/AuthContext'

// ============================================================
// NAVBAR COMPONENT
// ============================================================
//
// This component is responsible for application navigation.
//
// It reads authentication information from AuthContext and
// displays different links depending on:
//
// - Whether the user is logged in
// - Whether the user is an Admin or Customer
//
// Later we can improve this navbar with:
// - Professional styling
// - Responsive/mobile menu
// - Cart item count
// - User profile menu
// ============================================================

function Navbar() {

    const navigate =
        useNavigate()

    const {
        user,
        isAuthenticated,
        logout
    } = useAuth()

    // ==========================================================
    // LOGOUT
    // ==========================================================
    //
    // AuthContext removes:
    // - accessToken
    // - user
    //
    // After logout, redirect the user to the login page.
    // ==========================================================

    const handleLogout = () => {

        logout()

        navigate('/login')
    }

    return (
        <nav>

            {/* ======================================================
          PUBLIC LINK
         ====================================================== */}

            <Link to="/">
                Home
            </Link>

            {/* ======================================================
          AUTHENTICATED USER LINKS
         ====================================================== */}

            {isAuthenticated && (
                <>

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

                    {/* ==================================================
              ADMIN LINKS
             ==================================================
             
              These links are shown only when the authenticated
              user's role is exactly "Admin".

              Customers will not see these links.
             ================================================== */}

                    {user?.role === 'Admin' && (
                        <>

                            {' | '}

                            <Link to="/admin">
                                Admin Dashboard
                            </Link>

                            {' | '}

                            <Link to="/admin/products">
                                Manage Products
                            </Link>

                            {' | '}

                            <Link to="/admin/orders">
                                Manage Orders
                            </Link>

                            {' | '}

                            <Link to="/admin/categories">
                                Manage Categories
                            </Link>

                            {' | '}

                            <Link to="/admin/data">
                                Admin Data
                            </Link>

                        </>
                    )}

                </>
            )}

            {/* ======================================================
          AUTHENTICATION LINKS
         ====================================================== */}

            {isAuthenticated ? (

                <>

                    {' | '}

                    <span>
                        Welcome {user?.firstName}
                    </span>

                    {/* --------------------------------------------------
              Display the user's role temporarily.

              This is useful while developing and testing
              Admin/Customer authorization.

              Later we can remove this or display it inside a
              profile dropdown.
             -------------------------------------------------- */}

                    {user?.role && (
                        <>
                            {' '}
                            ({user.role})
                        </>
                    )}

                    {' | '}

                    <button
                        type="button"
                        onClick={handleLogout}
                    >
                        Logout
                    </button>

                </>

            ) : (

                <>

                    {' | '}

                    <Link to="/login">
                        Login
                    </Link>

                    {' | '}

                    <Link to="/register">
                        Register
                    </Link>

                </>

            )}

        </nav>
    )
}

export default Navbar