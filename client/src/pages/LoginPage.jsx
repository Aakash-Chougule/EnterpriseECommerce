import {
    useState
} from 'react'

import {
    Link,
    useNavigate
} from 'react-router-dom'

import {
    useAuth
} from '../context/AuthContext'

import {
    loginUser
} from '../services/authService'

import './Auth.css'

// ============================================================
// LOGIN PAGE
// ============================================================

function LoginPage() {

    const [email, setEmail] =
        useState('')

    const [password, setPassword] =
        useState('')

    const [error, setError] =
        useState('')

    const [loading, setLoading] =
        useState(false)

    const navigate =
        useNavigate()

    const {
        login
    } = useAuth()

    // ========================================================
    // LOGIN
    // ========================================================

    const handleSubmit = async (event) => {

        event.preventDefault()

        setError('')

        if (!email.trim()) {
            setError('Email is required.')
            return
        }

        if (!password) {
            setError('Password is required.')
            return
        }

        try {

            setLoading(true)

            const response =
                await loginUser(
                    email,
                    password
                )

            // Store authenticated user and JWT.
            login(response)

            // ============================================================
            // ROLE-BASED REDIRECT
            // ============================================================
            //
            // Admin    -> Admin Dashboard
            // Customer -> Home Page
            // ============================================================

            if (response.role === 'Admin') {
                navigate('/admin')
            } else {
                navigate('/')
            }
        }
        catch (err) {

            console.error(
                'Login failed:',
                err
            )

            const message =
                err.response?.data?.message ||
                'Login failed. Please check your email and password.'

            setError(message)
        }
        finally {

            setLoading(false)
        }
    }

    return (

        <main className="auth-page">

            <div className="auth-container">

                {/* ==================================================
                    LEFT SIDE
                   ================================================== */}

                <section className="auth-showcase">

                    <div className="auth-showcase-content">

                        <span className="auth-showcase-badge">
                            Enterprise Commerce
                        </span>

                        <h1>
                            Welcome back.
                        </h1>

                        <p>
                            Sign in to continue shopping,
                            manage your orders and access
                            your account.
                        </p>

                        <div className="auth-benefits">

                            <div className="auth-benefit">

                                <span className="auth-benefit-icon">
                                    ✓
                                </span>

                                <div>
                                    <strong>
                                        Browse Products
                                    </strong>

                                    <p>
                                        Discover available products
                                        from our catalog.
                                    </p>
                                </div>

                            </div>

                            <div className="auth-benefit">

                                <span className="auth-benefit-icon">
                                    ✓
                                </span>

                                <div>
                                    <strong>
                                        Manage Orders
                                    </strong>

                                    <p>
                                        View your orders and follow
                                        their current status.
                                    </p>
                                </div>

                            </div>

                            <div className="auth-benefit">

                                <span className="auth-benefit-icon">
                                    ✓
                                </span>

                                <div>
                                    <strong>
                                        Secure Access
                                    </strong>

                                    <p>
                                        Your account is protected
                                        through secure authentication.
                                    </p>
                                </div>

                            </div>

                        </div>

                    </div>

                </section>

                {/* ==================================================
                    LOGIN FORM
                   ================================================== */}

                <section className="auth-form-section">

                    <div className="auth-card">

                        <div className="auth-card-header">

                            <div className="auth-logo">
                                E
                            </div>

                            <h2>
                                Sign in to your account
                            </h2>

                            <p>
                                Enter your credentials to continue.
                            </p>

                        </div>

                        {error && (

                            <div className="auth-alert auth-alert-error">
                                <span>!</span>

                                {error}
                            </div>

                        )}

                        <form
                            onSubmit={handleSubmit}
                            className="auth-form"
                        >

                            <div className="auth-form-group">

                                <label htmlFor="email">
                                    Email Address
                                </label>

                                <input
                                    id="email"
                                    type="email"
                                    value={email}
                                    placeholder="name@example.com"
                                    autoComplete="email"
                                    onChange={
                                        (event) =>
                                            setEmail(
                                                event.target.value
                                            )
                                    }
                                />

                            </div>

                            <div className="auth-form-group">

                                <label htmlFor="password">
                                    Password
                                </label>

                                <input
                                    id="password"
                                    type="password"
                                    value={password}
                                    placeholder="Enter your password"
                                    autoComplete="current-password"
                                    onChange={
                                        (event) =>
                                            setPassword(
                                                event.target.value
                                            )
                                    }
                                />

                            </div>

                            <button
                                type="submit"
                                className="auth-submit-button"
                                disabled={loading}
                            >
                                {
                                    loading
                                        ? 'Signing in...'
                                        : 'Sign In'
                                }

                                {!loading && (
                                    <span>
                                        →
                                    </span>
                                )}

                            </button>

                        </form>

                        <div className="auth-footer">

                            <span>
                                Don't have an account?
                            </span>

                            <Link to="/register">
                                Create account
                            </Link>

                        </div>

                    </div>

                </section>

            </div>

        </main>
    )
}

export default LoginPage