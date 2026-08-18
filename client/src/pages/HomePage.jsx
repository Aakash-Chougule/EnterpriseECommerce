import {
    Link
} from 'react-router-dom'

import {
    useAuth
} from '../context/AuthContext'

import './HomePage.css'

// ============================================================
// HOME PAGE
// ============================================================

function HomePage() {

    const {
        isAuthenticated,
        user
    } = useAuth()

    return (

        <main className="home-page">

            {/* ==================================================
                HERO SECTION
               ================================================== */}

            <section className="home-hero">

                <div className="home-hero-content">

                    <span className="hero-badge">
                        Enterprise E-Commerce Platform
                    </span>

                    <h1 className="hero-title">
                        Everything you need,
                        <span>
                            all in one place.
                        </span>
                    </h1>

                    <p className="hero-description">
                        Discover products, manage your cart,
                        place orders and enjoy a simple,
                        secure shopping experience.
                    </p>

                    <div className="hero-actions">

                        {isAuthenticated ? (

                            <Link
                                to="/products"
                                className="hero-primary-button"
                            >
                                Browse Products
                                <span>→</span>
                            </Link>

                        ) : (

                            <>
                                <Link
                                    to="/register"
                                    className="hero-primary-button"
                                >
                                    Get Started
                                    <span>→</span>
                                </Link>

                                <Link
                                    to="/login"
                                    className="hero-secondary-button"
                                >
                                    Sign In
                                </Link>
                            </>

                        )}

                    </div>

                    {isAuthenticated && (

                        <p className="hero-welcome">
                            Welcome back,
                            <strong>
                                {' '}
                                {user?.firstName || 'User'}
                            </strong>
                        </p>

                    )}

                </div>

                {/* ==================================================
                    HERO VISUAL
                   ================================================== */}

                <div className="home-hero-visual">

                    <div className="hero-visual-card">

                        <div className="hero-shopping-icon">
                            🛍
                        </div>

                        <h2>
                            Smart Shopping
                        </h2>

                        <p>
                            Browse products and place your
                            orders quickly and securely.
                        </p>

                        <div className="hero-feature-list">

                            <div className="hero-feature-item">
                                <span>✓</span>
                                Easy product browsing
                            </div>

                            <div className="hero-feature-item">
                                <span>✓</span>
                                Simple checkout
                            </div>

                            <div className="hero-feature-item">
                                <span>✓</span>
                                Order tracking
                            </div>

                            <div className="hero-feature-item">
                                <span>✓</span>
                                Secure account access
                            </div>

                        </div>

                    </div>

                </div>

            </section>

            {/* ==================================================
                FEATURES
               ================================================== */}

            <section className="home-features">

                <div className="home-section-heading">

                    <span>
                        Why choose us?
                    </span>

                    <h2>
                        A better shopping experience
                    </h2>

                    <p>
                        Everything is designed to make
                        shopping simple and convenient.
                    </p>

                </div>

                <div className="feature-grid">

                    <article className="feature-card">

                        <div className="feature-icon">
                            🛒
                        </div>

                        <h3>
                            Easy Shopping
                        </h3>

                        <p>
                            Browse available products,
                            add items to your cart and
                            checkout with ease.
                        </p>

                    </article>

                    <article className="feature-card">

                        <div className="feature-icon">
                            📦
                        </div>

                        <h3>
                            Order Management
                        </h3>

                        <p>
                            View your previous orders
                            and follow their current
                            order status.
                        </p>

                    </article>

                    <article className="feature-card">

                        <div className="feature-icon">
                            🔒
                        </div>

                        <h3>
                            Secure Access
                        </h3>

                        <p>
                            Authentication and
                            role-based authorization
                            help protect your account.
                        </p>

                    </article>

                </div>

            </section>

            {/* ==================================================
                CTA
               ================================================== */}

            <section className="home-cta">

                <div>

                    <h2>
                        Ready to start shopping?
                    </h2>

                    <p>
                        Explore our available products
                        and find what you need.
                    </p>

                </div>

                <Link
                    to={
                        isAuthenticated
                            ? '/products'
                            : '/register'
                    }
                    className="cta-button"
                >
                    {
                        isAuthenticated
                            ? 'View Products'
                            : 'Create Account'
                    }

                    <span>→</span>

                </Link>

            </section>

        </main>
    )
}

export default HomePage