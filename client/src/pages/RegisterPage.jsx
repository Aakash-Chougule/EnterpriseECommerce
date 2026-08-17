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

// ============================================================
// REGISTER PAGE
// ============================================================
//
// This page allows a new customer to create an account.
//
// React concepts used here:
// - useState
// - Controlled inputs
// - Form submission
// - Frontend validation
// - API calls
// - Navigation
// ============================================================

function RegisterPage() {

    const navigate =
        useNavigate()

    // ----------------------------------------------------------
    // FORM STATE
    // ----------------------------------------------------------
    //
    // Instead of creating one useState for every field,
    // we keep all registration fields inside one object.
    // ----------------------------------------------------------

    const [formData, setFormData] =
        useState({
            firstName: '',
            lastName: '',
            email: '',
            password: '',
            confirmPassword: '',
            phoneNumber: ''
        })

    // ----------------------------------------------------------
    // UI STATE
    // ----------------------------------------------------------

    const [error, setError] =
        useState('')

    const [success, setSuccess] =
        useState('')

    const [loading, setLoading] =
        useState(false)

    // ==========================================================
    // HANDLE INPUT CHANGE
    // ==========================================================
    //
    // This one function handles every form input.
    //
    // Example:
    //
    // name="firstName"
    // value="Aakash"
    //
    // becomes:
    //
    // formData.firstName = "Aakash"
    // ==========================================================

    const handleChange =
        (event) => {

            const {
                name,
                value
            } = event.target

            setFormData(
                (currentData) => ({
                    ...currentData,

                    [name]: value
                })
            )
        }

    // ==========================================================
    // REGISTER
    // ==========================================================

    const handleSubmit =
        async (event) => {

            // Prevent normal browser page refresh.
            event.preventDefault()

            setError('')
            setSuccess('')

            // ------------------------------------------------------
            // FRONTEND VALIDATION
            // ------------------------------------------------------

            if (!formData.firstName.trim()) {
                setError(
                    'First name is required.'
                )

                return
            }

            if (!formData.lastName.trim()) {
                setError(
                    'Last name is required.'
                )

                return
            }

            if (!formData.email.trim()) {
                setError(
                    'Email is required.'
                )

                return
            }

            if (!formData.password) {
                setError(
                    'Password is required.'
                )

                return
            }

            if (
                formData.password.length < 6
            ) {
                setError(
                    'Password must contain at least 6 characters.'
                )

                return
            }

            if (
                formData.password !==
                formData.confirmPassword
            ) {
                setError(
                    'Passwords do not match.'
                )

                return
            }

            try {

                setLoading(true)

                // ----------------------------------------------------
                // Build request for ASP.NET.
                //
                // confirmPassword is NOT sent because your backend
                // RegisterRequestDto does not contain that property.
                // ----------------------------------------------------

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
                        formData.phoneNumber.trim()
                        || null
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
                    'Account created successfully.'
                )

                // ----------------------------------------------------
                // Redirect to login after successful registration.
                // ----------------------------------------------------

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

    // ==========================================================
    // UI
    // ==========================================================

    return (
        <div>

            <h1>
                Create Account
            </h1>

            <form
                onSubmit={handleSubmit}
            >

                {/* ================================================
            FIRST NAME
           ================================================ */}

                <div>

                    <label
                        htmlFor="firstName"
                    >
                        First Name
                    </label>

                    <br />

                    <input
                        id="firstName"
                        name="firstName"
                        type="text"

                        value={
                            formData.firstName
                        }

                        placeholder={
                            'Enter first name'
                        }

                        onChange={
                            handleChange
                        }
                    />

                </div>

                <br />

                {/* ================================================
            LAST NAME
           ================================================ */}

                <div>

                    <label
                        htmlFor="lastName"
                    >
                        Last Name
                    </label>

                    <br />

                    <input
                        id="lastName"
                        name="lastName"
                        type="text"

                        value={
                            formData.lastName
                        }

                        placeholder={
                            'Enter last name'
                        }

                        onChange={
                            handleChange
                        }
                    />

                </div>

                <br />

                {/* ================================================
            EMAIL
           ================================================ */}

                <div>

                    <label
                        htmlFor="email"
                    >
                        Email
                    </label>

                    <br />

                    <input
                        id="email"
                        name="email"
                        type="email"

                        value={
                            formData.email
                        }

                        placeholder={
                            'Enter email address'
                        }

                        onChange={
                            handleChange
                        }
                    />

                </div>

                <br />

                {/* ================================================
            PHONE NUMBER
           ================================================ */}

                <div>

                    <label
                        htmlFor="phoneNumber"
                    >
                        Phone Number
                    </label>

                    <br />

                    <input
                        id="phoneNumber"
                        name="phoneNumber"
                        type="tel"

                        value={
                            formData.phoneNumber
                        }

                        placeholder={
                            'Enter phone number'
                        }

                        onChange={
                            handleChange
                        }
                    />

                </div>

                <br />

                {/* ================================================
            PASSWORD
           ================================================ */}

                <div>

                    <label
                        htmlFor="password"
                    >
                        Password
                    </label>

                    <br />

                    <input
                        id="password"
                        name="password"
                        type="password"

                        value={
                            formData.password
                        }

                        placeholder={
                            'Create password'
                        }

                        onChange={
                            handleChange
                        }
                    />

                </div>

                <br />

                {/* ================================================
            CONFIRM PASSWORD
           ================================================ */}

                <div>

                    <label
                        htmlFor="confirmPassword"
                    >
                        Confirm Password
                    </label>

                    <br />

                    <input
                        id="confirmPassword"
                        name="confirmPassword"
                        type="password"

                        value={
                            formData.confirmPassword
                        }

                        placeholder={
                            'Confirm password'
                        }

                        onChange={
                            handleChange
                        }
                    />

                </div>

                <br />

                {/* ================================================
            ERROR / SUCCESS
           ================================================ */}

                {error && (
                    <p>
                        {error}
                    </p>
                )}

                {success && (
                    <p>
                        {success}
                    </p>
                )}

                {/* ================================================
            SUBMIT
           ================================================ */}

                <button
                    type="submit"
                    disabled={loading}
                >
                    {
                        loading
                            ? 'Creating Account...'
                            : 'Register'
                    }
                </button>

            </form>

            <br />

            <p>

                Already have an account?

                {' '}

                <Link to="/login">
                    Login
                </Link>

            </p>

        </div>
    )
}

export default RegisterPage