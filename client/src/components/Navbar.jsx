import {
    Link,
    NavLink,
    useNavigate
} from 'react-router-dom'

import {
    useEffect,
    useState
} from 'react'

import {
    useAuth
} from '../context/AuthContext'

import {
    getProfile
} from '../services/profileService'

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

    // ========================================================
    // UI STATE
    // ========================================================

    const [mobileMenuOpen, setMobileMenuOpen] =
        useState(false)

    const [adminMenuOpen, setAdminMenuOpen] =
        useState(false)

    const [profile, setProfile] =
        useState(null)

    // ========================================================
    // LOAD CURRENT PROFILE
    // ========================================================

    useEffect(() => {

        let cancelled =
            false

        const loadProfile =
            async () => {

                if (!isAuthenticated) {

                    setProfile(null)

                    return
                }

                try {

                    const data =
                        await getProfile()

                    if (!cancelled) {

                        setProfile(
                            data
                        )
                    }
                }
                catch (error) {

                    console.error(
                        'Unable to load navbar profile:',
                        error
                    )
                }
            }

        loadProfile()

        return () => {

            cancelled =
                true
        }

    }, [
        isAuthenticated,
        user?.userId,
        user?.id
    ])

    // ========================================================
    // CURRENT USER
    // ========================================================

    const currentUser =
    {
        ...(user || {}),
        ...(profile || {})
    }

    const isAdmin =
        currentUser?.role === 'Admin'

    const isMainAdmin =
        currentUser?.isMainAdmin === true

    const permissions =
        Array.isArray(
            currentUser?.permissions
        )
            ? currentUser.permissions
            : []

    // ========================================================
    // PERMISSION CHECK
    // ========================================================

    const hasPermission =
        (permission) => {

            if (!isAdmin) {

                return false
            }

            if (isMainAdmin) {

                return true
            }

            return permissions.includes(
                permission
            )
        }

    // ========================================================
    // LOGOUT
    // ========================================================

    const handleLogout =
        () => {

            logout()

            setProfile(null)

            setMobileMenuOpen(
                false
            )

            setAdminMenuOpen(
                false
            )

            navigate(
                '/login'
            )
        }

    // ========================================================
    // CLOSE MENU
    // ========================================================

    const closeMenu =
        () => {

            setMobileMenuOpen(
                false
            )

            setAdminMenuOpen(
                false
            )
        }

    // ========================================================
    // NAV LINK CLASS
    // ========================================================

    const getNavLinkClass =
        ({ isActive }) =>
            isActive
                ? 'navbar-link active'
                : 'navbar-link'

    // ========================================================
    // ADMIN MENU
    // ========================================================

    const showAdminMenu =
        isAuthenticated &&
        isAdmin

    return (

        <header className="navbar">

            <div className="navbar-container">

                {/* ==================================================
                    BRAND
                   ================================================== */}

                <Link
                    to="/"
                    className="navbar-brand"
                    onClick={
                        closeMenu
                    }
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
                                current =>
                                    !current
                            )
                    }
                    aria-label="Toggle navigation"
                    aria-expanded={
                        mobileMenuOpen
                    }
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

                        {/* HOME */}

                        <NavLink
                            to="/"
                            end
                            className={
                                getNavLinkClass
                            }
                            onClick={
                                closeMenu
                            }
                        >
                            Home
                        </NavLink>

                        {/* ==================================================
                            AUTHENTICATED LINKS
                           ================================================== */}

                        {isAuthenticated && (

                            <>

                                <NavLink
                                    to="/products"
                                    className={
                                        getNavLinkClass
                                    }
                                    onClick={
                                        closeMenu
                                    }
                                >
                                    Products
                                </NavLink>

                                <NavLink
                                    to="/cart"
                                    className={
                                        getNavLinkClass
                                    }
                                    onClick={
                                        closeMenu
                                    }
                                >
                                    Cart
                                </NavLink>

                                <NavLink
                                    to="/orders"
                                    className={
                                        getNavLinkClass
                                    }
                                    onClick={
                                        closeMenu
                                    }
                                >
                                    My Orders
                                </NavLink>

                                <NavLink
                                    to="/profile"
                                    className={
                                        getNavLinkClass
                                    }
                                    onClick={
                                        closeMenu
                                    }
                                >
                                    My Profile
                                </NavLink>

                                {/* ==================================================
                                    ADMIN
                                   ================================================== */}

                                {showAdminMenu && (

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
                                            aria-expanded={
                                                adminMenuOpen
                                            }
                                        >

                                            {
                                                isMainAdmin
                                                    ? 'Main Admin'
                                                    : 'Admin'
                                            }

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

                                                {/* DASHBOARD */}

                                                <NavLink
                                                    to="/admin"
                                                    end
                                                    className="dropdown-link"
                                                    onClick={
                                                        closeMenu
                                                    }
                                                >
                                                    Dashboard
                                                </NavLink>

                                                {/* PRODUCTS */}

                                                {
                                                    hasPermission(
                                                        'ManageProducts'
                                                    ) &&
                                                    (
                                                        <NavLink
                                                            to="/admin/products"
                                                            className="dropdown-link"
                                                            onClick={
                                                                closeMenu
                                                            }
                                                        >
                                                            Manage Products
                                                        </NavLink>
                                                    )
                                                }

                                                {/* CATEGORIES */}

                                                {
                                                    hasPermission(
                                                        'ManageCategories'
                                                    ) &&
                                                    (
                                                        <NavLink
                                                            to="/admin/categories"
                                                            className="dropdown-link"
                                                            onClick={
                                                                closeMenu
                                                            }
                                                        >
                                                            Manage Categories
                                                        </NavLink>
                                                    )
                                                }

                                                {/* ORDERS */}

                                                {
                                                    hasPermission(
                                                        'ManageOrders'
                                                    ) &&
                                                    (
                                                        <NavLink
                                                            to="/admin/orders"
                                                            className="dropdown-link"
                                                            onClick={
                                                                closeMenu
                                                            }
                                                        >
                                                            Manage Orders
                                                        </NavLink>
                                                    )
                                                }

                                                {/* INVENTORY */}

                                                {
                                                    hasPermission(
                                                        'ManageInventory'
                                                    ) &&
                                                    (
                                                        <NavLink
                                                            to="/admin/inventory"
                                                            className="dropdown-link"
                                                            onClick={
                                                                closeMenu
                                                            }
                                                        >
                                                            Inventory
                                                        </NavLink>
                                                    )
                                                }

                                                {
                                                    (
                                                        isMainAdmin ||
                                                        hasPermission(
                                                            'ViewReports'
                                                        )
                                                    ) &&
                                                    (
                                                        <NavLink
                                                            to="/admin/reports"
                                                            className="dropdown-link"
                                                            onClick={
                                                                closeMenu
                                                            }
                                                        >
                                                            Reports
                                                        </NavLink>
                                                    )
                                                }

                                                {/* DATA / REPORTS */}

                                                {
                                                    (
                                                        isMainAdmin ||
                                                        hasPermission(
                                                            'ViewReports'
                                                        )
                                                    ) &&
                                                    (
                                                        <NavLink
                                                            to="/admin/data"
                                                            className="dropdown-link"
                                                            onClick={
                                                                closeMenu
                                                            }
                                                        >
                                                            Admin Data
                                                        </NavLink>
                                                    )
                                                }

                                                {/* ==================================================
                                                    USERS & ADMINS
                                                   ================================================== */}

                                                {
                                                    (
                                                        isMainAdmin ||
                                                        hasPermission(
                                                            'ManageUsers'
                                                        ) ||
                                                        hasPermission(
                                                            'ManageAdmins'
                                                        )
                                                    ) &&
                                                    (
                                                        <NavLink
                                                            to="/admin/users"
                                                            className="dropdown-link"
                                                            onClick={
                                                                closeMenu
                                                            }
                                                        >
                                                            Users & Admins
                                                        </NavLink>
                                                    )
                                                }

                                                {/* ==================================================
                                                    FULL SYSTEM ACCESS
                                                    MAIN ADMIN ONLY
                                                   ================================================== */}

                                                {isMainAdmin && (

                                                    <NavLink
                                                        to="/admin/access"
                                                        className="dropdown-link"
                                                        onClick={
                                                            closeMenu
                                                        }
                                                    >
                                                        Full System Access
                                                    </NavLink>

                                                )}

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

                                <NavLink
                                    to="/profile"
                                    className="user-information"
                                    onClick={
                                        closeMenu
                                    }
                                    title="Open My Profile"
                                >

                                    <div className="user-avatar">

                                        {
                                            currentUser
                                                ?.firstName
                                                ?.charAt(0)
                                                ?.toUpperCase()
                                            ||
                                            'U'
                                        }

                                    </div>

                                    <div className="user-details">

                                        <span className="user-name">

                                            {
                                                currentUser
                                                    ?.firstName
                                                ||
                                                'User'
                                            }

                                        </span>

                                        <span className="user-role">

                                            {
                                                isMainAdmin
                                                    ? 'Main Admin'
                                                    : (
                                                        currentUser
                                                            ?.role
                                                        ||
                                                        'Customer'
                                                    )
                                            }

                                        </span>

                                    </div>

                                </NavLink>

                                <button
                                    type="button"
                                    className="logout-button"
                                    onClick={
                                        handleLogout
                                    }
                                >
                                    Logout
                                </button>

                            </>

                        ) : (

                            <>

                                <NavLink
                                    to="/login"
                                    className="login-link"
                                    onClick={
                                        closeMenu
                                    }
                                >
                                    Login
                                </NavLink>

                                <NavLink
                                    to="/register"
                                    className="register-link"
                                    onClick={
                                        closeMenu
                                    }
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