import {
    Link
} from 'react-router-dom'

import './AdminDashboardPage.css'

// ============================================================
// ADMIN DASHBOARD PAGE
// ============================================================
//
// Admin landing page.
//
// Provides quick access to:
// - Product Management
// - Category Management
// - Order Management
// - Inventory
// - Admin Data / Archive
// ============================================================

function AdminDashboardPage() {

    return (

        <main className="admin-dashboard-page">

            <div className="admin-dashboard-container">

                {/* ==================================================
                    HEADER
                   ================================================== */}

                <section className="admin-dashboard-header">

                    <div>

                        <span className="admin-dashboard-eyebrow">
                            Administration
                        </span>

                        <h1>
                            Admin Dashboard
                        </h1>

                        <p>
                            Manage products, categories,
                            orders and inventory from one place.
                        </p>

                    </div>

                </section>

                {/* ==================================================
                    QUICK SUMMARY
                   ================================================== */}

                <section className="admin-summary-grid">

                    <div className="admin-summary-card">

                        <span className="admin-summary-icon">
                            📦
                        </span>

                        <div>

                            <span className="admin-summary-label">
                                Products
                            </span>

                            <strong>
                                Product Management
                            </strong>

                        </div>

                    </div>

                    <div className="admin-summary-card">

                        <span className="admin-summary-icon">
                            🗂
                        </span>

                        <div>

                            <span className="admin-summary-label">
                                Categories
                            </span>

                            <strong>
                                Category Management
                            </strong>

                        </div>

                    </div>

                    <div className="admin-summary-card">

                        <span className="admin-summary-icon">
                            🧾
                        </span>

                        <div>

                            <span className="admin-summary-label">
                                Orders
                            </span>

                            <strong>
                                Order Management
                            </strong>

                        </div>

                    </div>

                    <div className="admin-summary-card">

                        <span className="admin-summary-icon">
                            📊
                        </span>

                        <div>

                            <span className="admin-summary-label">
                                Inventory
                            </span>

                            <strong>
                                Stock Management
                            </strong>

                        </div>

                    </div>

                </section>

                {/* ==================================================
                    MANAGEMENT
                   ================================================== */}

                <section className="admin-management-section">

                    <div className="admin-section-heading">

                        <span>
                            Management
                        </span>

                        <h2>
                            Admin Tools
                        </h2>

                        <p>
                            Choose an area to manage.
                        </p>

                    </div>

                    <div className="admin-management-grid">

                        {/* ==========================================
                            PRODUCTS
                           ========================================== */}

                        <Link
                            to="/admin/products"
                            className="admin-management-card"
                        >

                            <div className="admin-management-icon">
                                📦
                            </div>

                            <div className="admin-management-content">

                                <h3>
                                    Manage Products
                                </h3>

                                <p>
                                    Create, update and deactivate
                                    products in the catalog.
                                </p>

                            </div>

                            <span className="admin-card-arrow">
                                →
                            </span>

                        </Link>

                        {/* ==========================================
                            CATEGORIES
                           ========================================== */}

                        <Link
                            to="/admin/categories"
                            className="admin-management-card"
                        >

                            <div className="admin-management-icon">
                                🗂
                            </div>

                            <div className="admin-management-content">

                                <h3>
                                    Manage Categories
                                </h3>

                                <p>
                                    Create and organize product
                                    categories.
                                </p>

                            </div>

                            <span className="admin-card-arrow">
                                →
                            </span>

                        </Link>

                        {/* ==========================================
                            ORDERS
                           ========================================== */}

                        <Link
                            to="/admin/orders"
                            className="admin-management-card"
                        >

                            <div className="admin-management-icon">
                                🧾
                            </div>

                            <div className="admin-management-content">

                                <h3>
                                    Manage Orders
                                </h3>

                                <p>
                                    Review orders and update
                                    order progress.
                                </p>

                            </div>

                            <span className="admin-card-arrow">
                                →
                            </span>

                        </Link>

                        {/* ==========================================
                            INVENTORY
                           ========================================== */}

                        <Link
                            to="/admin/inventory"
                            className="admin-management-card"
                        >

                            <div className="admin-management-icon">
                                📊
                            </div>

                            <div className="admin-management-content">

                                <h3>
                                    Inventory
                                </h3>

                                <p>
                                    Monitor stock levels and
                                    adjust product inventory.
                                </p>

                            </div>

                            <span className="admin-card-arrow">
                                →
                            </span>

                        </Link>

                        {/* ==========================================
                            ADMIN DATA
                           ========================================== */}

                        <Link
                            to="/admin/data"
                            className="admin-management-card"
                        >

                            <div className="admin-management-icon">
                                🗄
                            </div>

                            <div className="admin-management-content">

                                <h3>
                                    Admin Data
                                </h3>

                                <p>
                                    View active and inactive
                                    products and categories.
                                </p>

                            </div>

                            <span className="admin-card-arrow">
                                →
                            </span>

                        </Link>

                    </div>

                </section>

            </div>

        </main>
    )
}

export default AdminDashboardPage