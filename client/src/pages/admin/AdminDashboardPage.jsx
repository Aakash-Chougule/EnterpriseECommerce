import './AdminDashboardPage.css'

// ============================================================
// ADMIN DASHBOARD PAGE
// ============================================================
//
// Admin landing page.
//
// This page now focuses on:
// - System overview
// - Platform capabilities
// - Architecture overview
// - Main operational areas
//
// Actual management navigation is available from the
// Admin / Main Admin dropdown in the Navbar.
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
                            Enterprise Administration
                        </span>

                        <h1>
                            Enterprise E-Commerce Platform
                        </h1>

                        <p>
                            Centralized commerce system for managing
                            products, inventory, customer orders,
                            payments, users, administrators,
                            notifications and operational data.
                        </p>

                    </div>

                </section>

                {/* ==================================================
                    QUICK SYSTEM SUMMARY
                   ================================================== */}

                <section className="admin-summary-grid">

                    <div className="admin-summary-card">

                        <span className="admin-summary-icon">
                            🛒
                        </span>

                        <div>

                            <span className="admin-summary-label">
                                Commerce
                            </span>

                            <strong>
                                Product & Order Platform
                            </strong>

                        </div>

                    </div>

                    <div className="admin-summary-card">

                        <span className="admin-summary-icon">
                            🔐
                        </span>

                        <div>

                            <span className="admin-summary-label">
                                Security
                            </span>

                            <strong>
                                JWT & Role Based Access
                            </strong>

                        </div>

                    </div>

                    <div className="admin-summary-card">

                        <span className="admin-summary-icon">
                            ⚡
                        </span>

                        <div>

                            <span className="admin-summary-label">
                                Messaging
                            </span>

                            <strong>
                                Kafka Event Processing
                            </strong>

                        </div>

                    </div>

                    <div className="admin-summary-card">

                        <span className="admin-summary-icon">
                            💳
                        </span>

                        <div>

                            <span className="admin-summary-label">
                                Payments
                            </span>

                            <strong>
                                Razorpay & COD
                            </strong>

                        </div>

                    </div>

                </section>

                {/* ==================================================
                    SYSTEM OVERVIEW
                   ================================================== */}

                <section className="admin-system-overview">

                    <div className="admin-section-heading">

                        <span>
                            System Overview
                        </span>

                        <h2>
                            Platform Capabilities
                        </h2>

                        <p>
                            The system combines customer commerce,
                            administrative control, secure payments,
                            real-time event processing and role-based
                            authorization in one enterprise platform.
                        </p>

                    </div>

                    <div className="admin-overview-grid">

                        {/* ==========================================
                            PRODUCT MANAGEMENT
                           ========================================== */}

                        <article className="admin-overview-card">

                            <div className="admin-overview-icon">
                                🛍️
                            </div>

                            <div>

                                <h3>
                                    Product & Catalog Management
                                </h3>

                                <p>
                                    Manage products, categories,
                                    pricing, availability and product
                                    information from a centralized
                                    catalog.
                                </p>

                            </div>

                        </article>

                        {/* ==========================================
                            INVENTORY
                           ========================================== */}

                        <article className="admin-overview-card">

                            <div className="admin-overview-icon">
                                📦
                            </div>

                            <div>

                                <h3>
                                    Inventory Management
                                </h3>

                                <p>
                                    Track stock quantities, monitor
                                    available inventory and keep stock
                                    information synchronized with
                                    customer orders.
                                </p>

                            </div>

                        </article>

                        {/* ==========================================
                            ORDERS
                           ========================================== */}

                        <article className="admin-overview-card">

                            <div className="admin-overview-icon">
                                🧾
                            </div>

                            <div>

                                <h3>
                                    Order Management
                                </h3>

                                <p>
                                    Monitor customer orders, shipping
                                    information, payment status and
                                    order progress through the complete
                                    order lifecycle.
                                </p>

                            </div>

                        </article>

                        {/* ==========================================
                            PAYMENTS
                           ========================================== */}

                        <article className="admin-overview-card">

                            <div className="admin-overview-icon">
                                💳
                            </div>

                            <div>

                                <h3>
                                    Payment Processing
                                </h3>

                                <p>
                                    Supports online payment processing
                                    with Razorpay as well as Cash on
                                    Delivery with secure backend payment
                                    verification.
                                </p>

                            </div>

                        </article>

                        {/* ==========================================
                            USERS
                           ========================================== */}

                        <article className="admin-overview-card">

                            <div className="admin-overview-icon">
                                👥
                            </div>

                            <div>

                                <h3>
                                    User & Administrator Control
                                </h3>

                                <p>
                                    Manage customer accounts, promote
                                    users to administrators and control
                                    administrative access from the Main
                                    Admin account.
                                </p>

                            </div>

                        </article>

                        {/* ==========================================
                            AUTHORIZATION
                           ========================================== */}

                        <article className="admin-overview-card">

                            <div className="admin-overview-icon">
                                🔐
                            </div>

                            <div>

                                <h3>
                                    Permission-Based Authorization
                                </h3>

                                <p>
                                    Main Admin receives full system
                                    access while other administrators
                                    only receive explicitly assigned
                                    permissions.
                                </p>

                            </div>

                        </article>

                        {/* ==========================================
                            KAFKA
                           ========================================== */}

                        <article className="admin-overview-card">

                            <div className="admin-overview-icon">
                                ⚡
                            </div>

                            <div>

                                <h3>
                                    Event-Driven Architecture
                                </h3>

                                <p>
                                    Apache Kafka handles order,
                                    payment and order-status events
                                    between the API and background
                                    notification services.
                                </p>

                            </div>

                        </article>

                        {/* ==========================================
                            EMAIL
                           ========================================== */}

                        <article className="admin-overview-card">

                            <div className="admin-overview-icon">
                                ✉️
                            </div>

                            <div>

                                <h3>
                                    Customer Notifications
                                </h3>

                                <p>
                                    The Notification Service processes
                                    Kafka events and sends important
                                    customer emails for payments,
                                    confirmations, cancellations and
                                    delivery updates.
                                </p>

                            </div>

                        </article>

                    </div>

                </section>

                {/* ==================================================
                    TECHNOLOGY STACK
                   ================================================== */}

                <section className="admin-architecture-section">

                    <div className="admin-section-heading">

                        <span>
                            Architecture
                        </span>

                        <h2>
                            Technology Stack
                        </h2>

                        <p>
                            The platform follows a layered enterprise
                            architecture designed for maintainability,
                            scalability and separation of concerns.
                        </p>

                    </div>

                    <div className="admin-tech-grid">

                        <div className="admin-tech-card">

                            <span>
                                API
                            </span>

                            <strong>
                                ASP.NET Core
                            </strong>

                            <p>
                                REST APIs, business orchestration,
                                authentication and authorization.
                            </p>

                        </div>

                        <div className="admin-tech-card">

                            <span>
                                Frontend
                            </span>

                            <strong>
                                React
                            </strong>

                            <p>
                                Responsive customer and administration
                                interfaces.
                            </p>

                        </div>

                        <div className="admin-tech-card">

                            <span>
                                Database
                            </span>

                            <strong>
                                PostgreSQL
                            </strong>

                            <p>
                                Relational persistence using
                                Entity Framework Core.
                            </p>

                        </div>

                        <div className="admin-tech-card">

                            <span>
                                Messaging
                            </span>

                            <strong>
                                Apache Kafka
                            </strong>

                            <p>
                                Asynchronous event communication
                                between services.
                            </p>

                        </div>

                        <div className="admin-tech-card">

                            <span>
                                Containers
                            </span>

                            <strong>
                                Docker
                            </strong>

                            <p>
                                Containerized PostgreSQL, Kafka,
                                ZooKeeper and application services.
                            </p>

                        </div>

                        <div className="admin-tech-card">

                            <span>
                                Security
                            </span>

                            <strong>
                                JWT Authentication
                            </strong>

                            <p>
                                Role and permission-based access
                                control across protected endpoints.
                            </p>

                        </div>

                    </div>

                </section>

                {/* ==================================================
                    ADMINISTRATION NOTE
                   ================================================== */}

                <section className="admin-dashboard-note">

                    <div className="admin-dashboard-note-icon">
                        ℹ️
                    </div>

                    <div>

                        <h3>
                            Administration Navigation
                        </h3>

                        <p>
                            Product, category, order, inventory,
                            data, user and access-management tools
                            are available from the
                            <strong> Admin / Main Admin </strong>
                            menu in the navigation bar.
                        </p>

                    </div>

                </section>

            </div>

        </main>
    )
}

export default AdminDashboardPage