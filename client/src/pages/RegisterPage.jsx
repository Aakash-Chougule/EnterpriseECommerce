import {
    useState
} from 'react'

import {
    Link,
    useNavigate
} from 'react-router-dom'

import {
    registerUser
} from '../services/authService'

import './Auth.css'

// ============================================================
// REGISTER PAGE
// ============================================================

function RegisterPage() {

    const navigate =
        useNavigate()

    const [formData, setFormData] =
        useState({
            firstName: '',
            lastName: '',
            email: '',
            password: '',
            confirmPassword: '',
            phoneNumber: ''
        })

    const [error, setError] =
        useState('')

    const [success, setSuccess] =
        useState('')

    const [loading, setLoading] =
        useState(false)

    // ========================================================
    // INPUT CHANGE
    // ========================================================

    const handleChange = (event) => {

        const {
            name,
            value
        } = event.target

        setFormData(
            currentData => ({
                ...currentData,
                [name]: value
            })
        )
    }

    // ========================================================
    // REGISTER
    // ========================================================

    const handleSubmit = async (event) => {

        event.preventDefault()

        setError('')
        setSuccess('')

        if (!formData.firstName.trim()) {
            setError('First name is required.')
            return
        }

        if (!formData.lastName.trim()) {
            setError('Last name is required.')
            return
        }

        if (!formData.email.trim()) {
            setError('Email is required.')
            return
        }

        if (!formData.password) {
            setError('Password is required.')
            return
        }

        if (formData.password.length < 6) {
            setError(
                'Password must contain at least 6 characters.'
            )
            return
        }

        if (
            formData.password !==
            formData.confirmPassword
        ) {
            setError('Passwords do not match.')
            return
        }

        try {

            setLoading(true)

            const registerData = {

                firstName:
                    formData.firstName.trim(),

                lastName:
                    formData.lastName.trim(),

                email:
                    formData.email.trim(),

                password:
                    formData.password,

                phoneNumber:
                    formData.phoneNumber.trim() ||
                    null
            }

            const response =
                await registerUser(
                    registerData
                )

            console.log(
                'Registration successful:',
                response
            )

            setSuccess(
                'Account created successfully. Redirecting to login...'
            )

            setTimeout(() => {
                navigate('/login')
            }, 1000)
        }
        catch (err) {

            console.error(
                'Registration failed:',
                err
            )

            const message =
                err.response?.data?.message ||
                'Registration failed. Please try again.'

            setError(message)
        }
        finally {

            setLoading(false)
        }
    }

    return (

        <main className="auth-page">

            <div className="auth-container register-container">

                {/* ==================================================
                    LEFT SIDE
                   ================================================== */}

                <section className="auth-showcase">

                    <div className="auth-showcase-content">

                        <span className="auth-showcase-badge">
                            Join Enterprise Commerce
                        </span>

                        <h1>
                            Create your account.
                        </h1>

                        <p>
                            Register today and start exploring
                            our products and shopping experience.
                        </p>

                        <div className="auth-benefits">

                            <div className="auth-benefit">

                                <span className="auth-benefit-icon">
                                    ✓
                                </span>

                                <div>
                                    <strong>
                                        Simple Shopping
                                    </strong>

                                    <p>
                                        Browse products and manage
                                        your shopping cart.
                                    </p>
                                </div>

                            </div>

                            <div className="auth-benefit">

                                <span className="auth-benefit-icon">
                                    ✓
                                </span>

                                <div>
                                    <strong>
                                        Track Your Orders
                                    </strong>

                                    <p>
                                        Access your order history
                                        whenever you need it.
                                    </p>
                                </div>

                            </div>

                            <div className="auth-benefit">

                                <span className="auth-benefit-icon">
                                    ✓
                                </span>

                                <div>
                                    <strong>
                                        Secure Account
                                    </strong>

                                    <p>
                                        Your account uses secure
                                        authentication.
                                    </p>
                                </div>

                            </div>

                        </div>

                    </div>

                </section>

                {/* ==================================================
                    REGISTER FORM
                   ================================================== */}

                <section className="auth-form-section">

                    <div className="auth-card register-card">

                        <div className="auth-card-header">

                            <div className="auth-logo">
                                E
                            </div>

                            <h2>
                                Create an account
                            </h2>

                            <p>
                                Enter your information below
                                to get started.
                            </p>

                        </div>

                        {error && (

                            <div className="auth-alert auth-alert-error">
                                <span>!</span>

                                {error}
                            </div>

                        )}

                        {success && (

                            <div className="auth-alert auth-alert-success">
                                <span>✓</span>

                                {success}
                            </div>

                        )}

                        <form
                            onSubmit={handleSubmit}
                            className="auth-form"
                        >

                            <div className="auth-name-grid">

                                <div className="auth-form-group">

                                    <label htmlFor="firstName">
                                        First Name
                                    </label>

                                    <input
                                        id="firstName"
                                        name="firstName"
                                        type="text"
                                        value={formData.firstName}
                                        placeholder="First name"
                                        autoComplete="given-name"
                                        onChange={handleChange}
                                    />

                                </div>

                                <div className="auth-form-group">

                                    <label htmlFor="lastName">
                                        Last Name
                                    </label>

                                    <input
                                        id="lastName"
                                        name="lastName"
                                        type="text"
                                        value={formData.lastName}
                                        placeholder="Last name"
                                        autoComplete="family-name"
                                        onChange={handleChange}
                                    />

                                </div>

                            </div>

                            <div className="auth-form-group">

                                <label htmlFor="email">
                                    Email Address
                                </label>

                                <input
                                    id="email"
                                    name="email"
                                    type="email"
                                    value={formData.email}
                                    placeholder="name@example.com"
                                    autoComplete="email"
                                    onChange={handleChange}
                                />

                            </div>

                            <div className="auth-form-group">

                                <label htmlFor="phoneNumber">
                                    Phone Number

                                    <span className="optional-label">
                                        Optional
                                    </span>
                                </label>

                                <input
                                    id="phoneNumber"
                                    name="phoneNumber"
                                    type="tel"
                                    value={formData.phoneNumber}
                                    placeholder="Enter phone number"
                                    autoComplete="tel"
                                    onChange={handleChange}
                                />

                            </div>

                            <div className="auth-name-grid">

                                <div className="auth-form-group">

                                    <label htmlFor="password">
                                        Password
                                    </label>

                                    <input
                                        id="password"
                                        name="password"
                                        type="password"
                                        value={formData.password}
                                        placeholder="Minimum 6 characters"
                                        autoComplete="new-password"
                                        onChange={handleChange}
                                    />

                                </div>

                                <div className="auth-form-group">

                                    <label htmlFor="confirmPassword">
                                        Confirm Password
                                    </label>

                                    <input
                                        id="confirmPassword"
                                        name="confirmPassword"
                                        type="password"
                                        value={
                                            formData.confirmPassword
                                        }
                                        placeholder="Repeat password"
                                        autoComplete="new-password"
                                        onChange={handleChange}
                                    />

                                </div>

                            </div>

                            <button
                                type="submit"
                                className="auth-submit-button"
                                disabled={loading}
                            >
                                {
                                    loading
                                        ? 'Creating Account...'
                                        : 'Create Account'
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
                                Already have an account?
                            </span>

                            <Link to="/login">
                                Sign in
                            </Link>

                        </div>

                    </div>

                </section>

            </div>

        </main>
    )
}

export default RegisterPage