import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

import { loginUser } from '../services/authService'

function LoginPage() {
    // ------------------------------------------------------------
    // React state
    // ------------------------------------------------------------
    //
    // useState stores values that can change while the page
    // is running.
    // ------------------------------------------------------------

    const [email, setEmail] = useState('')
    const [password, setPassword] = useState('')

    const [error, setError] = useState('')
    const [loading, setLoading] = useState(false)

    // ------------------------------------------------------------
    // React Router navigation
    // ------------------------------------------------------------

    const navigate = useNavigate()

    const { login } = useAuth()
    // ------------------------------------------------------------
    // Login form submission
    // ------------------------------------------------------------

    const handleSubmit = async (event) => {
        // Prevent normal browser form submission.
        event.preventDefault()

        // Clear previous error.
        setError('')

        // Basic frontend validation.
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

            // Call ASP.NET login endpoint.
            const response = await loginUser(
                email,
                password
            )

            console.log(
                'Login response:',
                response
            )

            // --------------------------------------------------------
            // JWT Storage
            // --------------------------------------------------------
            //
            // We temporarily store the token in localStorage.
            //
            // Later, we will create an AuthContext so authentication
            // is managed professionally across the whole React app.
            // --------------------------------------------------------

            // localStorage.setItem(
            //     'accessToken',
            //     response.token
            // )

            // Store the authenticated user and JWT through AuthContext.
            login(response)

            // Redirect user after successful login.
            navigate('/products')
        } catch (err) {
            console.error(
                'Login failed:',
                err
            )

            // Try to display backend error message.
            const message =
                err.response?.data?.message ||
                'Login failed. Please check your email and password.'

            setError(message)
        } finally {
            setLoading(false)
        }
    }

    return (
        <div>
            <h1>Login</h1>

            {/* 
        onSubmit runs handleSubmit when the user submits
        this form.
      */}
            <form onSubmit={handleSubmit}>

                <div>
                    <label htmlFor="email">
                        Email
                    </label>

                    <br />

                    <input
                        id="email"
                        type="email"
                        value={email}
                        placeholder="Enter your email"

                        // Whenever the user types,
                        // update React state.
                        onChange={(event) =>
                            setEmail(event.target.value)
                        }
                    />
                </div>

                <br />

                <div>
                    <label htmlFor="password">
                        Password
                    </label>

                    <br />

                    <input
                        id="password"
                        type="password"
                        value={password}
                        placeholder="Enter your password"

                        onChange={(event) =>
                            setPassword(event.target.value)
                        }
                    />
                </div>

                <br />

                {/* Show error only when one exists */}
                {error && (
                    <p>
                        {error}
                    </p>
                )}

                <button
                    type="submit"
                    disabled={loading}
                >
                    {loading
                        ? 'Logging in...'
                        : 'Login'}
                </button>

            </form>
        </div>
    )
}

export default LoginPage