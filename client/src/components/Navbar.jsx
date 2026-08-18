import {
    Link,
    NavLink,
    useNavigate
} from 'react-router-dom'

import {
    useState
} from 'react'

import {
    useAuth
} from '../context/AuthContext'

import './Navbar.css'

// ============================================================
// NAVBAR
// ============================================================

function Navbar() {

    const navigate =
        useNavigate()

    const {
        user,
        isAuthenticated,
        logout
    } = useAuth()

    const [mobileMenuOpen, setMobileMenuOpen] =
        useState(false)

    const [adminMenuOpen, setAdminMenuOpen] =
        useState(false)

    // ========================================================
    // LOGOUT
    // ========================================================

    const handleLogout = () => {

        logout()

        setMobileMenuOpen(false)
        setAdminMenuOpen(false)

        navigate('/login')
    }

    // ========================================================
    // CLOSE MOBILE MENU
    // ========================================================

    const closeMenu = () => {

        setMobileMenuOpen(false)
        setAdminMenuOpen(false)
    }

    // ========================================================
    // ACTIVE LINK CLASS
    // ========================================================

    const getNavLinkClass =
        ({ isActive }) =>
            isActive
                ? 'navbar-link active'
                : 'navbar-link'

    return (

        <header className="navbar">

            <div className="navbar-container">

                {/* ==================================================
                    BRAND
                   ================================================== */}

                <Link
                    to="/"
                    className="navbar-brand"
                    onClick={closeMenu}
                >
                    <span className="brand-icon">
                        E
                    </span>

                    <span className="brand-text">
                        Enterprise
                        <strong>
                            Commerce
                        </strong>
                    </span>
                </Link>

                {/* ==================================================
                    MOBILE BUTTON
                   ================================================== */}

                <button
                    type="button"
                    className="navbar-toggle"
                    onClick={
                        () =>
                            setMobileMenuOpen(
                                current => !current
                            )
                    }
                    aria-label="Toggle navigation"
                >
                    ☰
                </button>

                {/* ==================================================
                    NAVIGATION
                   ================================================== */}

                <div
                    className={
                        mobileMenuOpen
                            ? 'navbar-content open'
                            : 'navbar-content'
                    }
                >

                    <nav className="navbar-links">

                        <NavLink
                            to="/"
                            end
                            className={getNavLinkClass}
                            onClick={closeMenu}
                        >
                            Home
                        </NavLink>

                        {isAuthenticated && (

                            <>

                                <NavLink
                                    to="/products"
                                    className={getNavLinkClass}
                                    onClick={closeMenu}
                                >
                                    Products
                                </NavLink>

                                <NavLink
                                    to="/cart"
                                    className={getNavLinkClass}
                                    onClick={closeMenu}
                                >
                                    Cart
                                </NavLink>

                                <NavLink
                                    to="/orders"
                                    className={getNavLinkClass}
                                    onClick={closeMenu}
                                >
                                    My Orders
                                </NavLink>

                                {/* ==================================
                                    ADMIN DROPDOWN
                                   ================================== */}

                                {user?.role === 'Admin' && (

                                    <div className="admin-dropdown">

                                        <button
                                            type="button"
                                            className="admin-dropdown-button"
                                            onClick={
                                                () =>
                                                    setAdminMenuOpen(
                                                        current =>
                                                            !current
                                                    )
                                            }
                                        >
                                            Admin

                                            <span
                                                className={
                                                    adminMenuOpen
                                                        ? 'dropdown-arrow open'
                                                        : 'dropdown-arrow'
                                                }
                                            >
                                                ▾
                                            </span>
                                        </button>

                                        {adminMenuOpen && (

                                            <div className="admin-dropdown-menu">

                                                <NavLink
                                                    to="/admin"
                                                    end
                                                    className="dropdown-link"
                                                    onClick={closeMenu}
                                                >
                                                    Dashboard
                                                </NavLink>

                                                <NavLink
                                                    to="/admin/products"
                                                    className="dropdown-link"
                                                    onClick={closeMenu}
                                                >
                                                    Manage Products
                                                </NavLink>

                                                <NavLink
                                                    to="/admin/categories"
                                                    className="dropdown-link"
                                                    onClick={closeMenu}
                                                >
                                                    Manage Categories
                                                </NavLink>

                                                <NavLink
                                                    to="/admin/orders"
                                                    className="dropdown-link"
                                                    onClick={closeMenu}
                                                >
                                                    Manage Orders
                                                </NavLink>

                                                <NavLink
                                                    to="/admin/inventory"
                                                    className="dropdown-link"
                                                    onClick={closeMenu}
                                                >
                                                    Inventory
                                                </NavLink>

                                                <NavLink
                                                    to="/admin/data"
                                                    className="dropdown-link"
                                                    onClick={closeMenu}
                                                >
                                                    Admin Data
                                                </NavLink>

                                            </div>

                                        )}

                                    </div>

                                )}

                            </>

                        )}

                    </nav>

                    {/* ==================================================
                        USER SECTION
                       ================================================== */}

                    <div className="navbar-user">

                        {isAuthenticated ? (

                            <>

                                <div className="user-information">

                                    <div className="user-avatar">

                                        {
                                            user?.firstName
                                                ?.charAt(0)
                                                ?.toUpperCase()
                                            || 'U'
                                        }

                                    </div>

                                    <div className="user-details">

                                        <span className="user-name">
                                            {
                                                user?.firstName ||
                                                'User'
                                            }
                                        </span>

                                        <span className="user-role">
                                            {
                                                user?.role ||
                                                'Customer'
                                            }
                                        </span>

                                    </div>

                                </div>

                                <button
                                    type="button"
                                    className="logout-button"
                                    onClick={handleLogout}
                                >
                                    Logout
                                </button>

                            </>

                        ) : (

                            <>

                                <NavLink
                                    to="/login"
                                    className="login-link"
                                    onClick={closeMenu}
                                >
                                    Login
                                </NavLink>

                                <NavLink
                                    to="/register"
                                    className="register-link"
                                    onClick={closeMenu}
                                >
                                    Create Account
                                </NavLink>

                            </>

                        )}

                    </div>

                </div>

            </div>

        </header>
    )
}

export default Navbar