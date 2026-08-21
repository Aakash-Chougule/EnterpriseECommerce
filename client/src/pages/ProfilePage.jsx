import {
    useEffect,
    useState
} from 'react'

import {
    changePassword,
    getProfile,
    updateProfile
} from '../services/profileService'

import './ProfilePage.css'

// ============================================================
// PROFILE PAGE
// ============================================================

function ProfilePage() {

    // ========================================================
    // PROFILE
    // ========================================================

    const [profile, setProfile] =
        useState(null)

    const [form, setForm] =
        useState({
            firstName: '',
            lastName: '',
            email: '',
            phoneNumber: ''
        })

    // ========================================================
    // PASSWORD
    // ========================================================

    const [passwordForm, setPasswordForm] =
        useState({
            currentPassword: '',
            newPassword: '',
            confirmPassword: ''
        })

    // ========================================================
    // STATE
    // ========================================================

    const [loading, setLoading] =
        useState(true)

    const [saving, setSaving] =
        useState(false)

    const [changingPassword, setChangingPassword] =
        useState(false)

    const [error, setError] =
        useState('')

    const [success, setSuccess] =
        useState('')

    const [passwordError, setPasswordError] =
        useState('')

    const [passwordSuccess, setPasswordSuccess] =
        useState('')

    // ========================================================
    // LOAD PROFILE
    // ========================================================

    const loadProfile =
        async () => {

            try {

                setLoading(true)
                setError('')

                const data =
                    await getProfile()

                setProfile(
                    data
                )

                setForm({
                    firstName:
                        data.firstName || '',

                    lastName:
                        data.lastName || '',

                    email:
                        data.email || '',

                    phoneNumber:
                        data.phoneNumber || ''
                })
            }
            catch (err) {

                console.error(
                    'Unable to load profile:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Unable to load your profile.'
                )
            }
            finally {

                setLoading(false)
            }
        }

    useEffect(() => {

        loadProfile()

    }, [])

    // ========================================================
    // PROFILE INPUT
    // ========================================================

    const handleProfileChange =
        (event) => {

            const {
                name,
                value
            } =
                event.target

            setForm(
                current => ({
                    ...current,
                    [name]: value
                })
            )
        }

    // ========================================================
    // PASSWORD INPUT
    // ========================================================

    const handlePasswordChange =
        (event) => {

            const {
                name,
                value
            } =
                event.target

            setPasswordForm(
                current => ({
                    ...current,
                    [name]: value
                })
            )
        }

    // ========================================================
    // UPDATE PROFILE
    // ========================================================

    const handleProfileSubmit =
        async (event) => {

            event.preventDefault()

            setError('')
            setSuccess('')

            if (!form.firstName.trim()) {

                setError(
                    'First name is required.'
                )

                return
            }

            if (!form.lastName.trim()) {

                setError(
                    'Last name is required.'
                )

                return
            }

            if (!form.email.trim()) {

                setError(
                    'Email is required.'
                )

                return
            }

            try {

                setSaving(true)

                const updated =
                    await updateProfile(
                        form
                    )

                setProfile(
                    updated
                )

                setForm({
                    firstName:
                        updated.firstName || '',

                    lastName:
                        updated.lastName || '',

                    email:
                        updated.email || '',

                    phoneNumber:
                        updated.phoneNumber || ''
                })

                setSuccess(
                    'Profile updated successfully.'
                )
            }
            catch (err) {

                console.error(
                    'Unable to update profile:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Unable to update profile.'
                )
            }
            finally {

                setSaving(false)
            }
        }

    // ========================================================
    // CHANGE PASSWORD
    // ========================================================

    const handlePasswordSubmit =
        async (event) => {

            event.preventDefault()

            setPasswordError('')
            setPasswordSuccess('')

            if (
                !passwordForm
                    .currentPassword
            ) {

                setPasswordError(
                    'Current password is required.'
                )

                return
            }

            if (
                passwordForm
                    .newPassword
                    .length < 8
            ) {

                setPasswordError(
                    'New password must contain at least 8 characters.'
                )

                return
            }

            if (
                passwordForm.newPassword !==
                passwordForm.confirmPassword
            ) {

                setPasswordError(
                    'New password and confirmation do not match.'
                )

                return
            }

            try {

                setChangingPassword(
                    true
                )

                await changePassword(
                    passwordForm.currentPassword,
                    passwordForm.newPassword
                )

                setPasswordForm({
                    currentPassword: '',
                    newPassword: '',
                    confirmPassword: ''
                })

                setPasswordSuccess(
                    'Password changed successfully.'
                )
            }
            catch (err) {

                console.error(
                    'Unable to change password:',
                    err
                )

                setPasswordError(
                    err.response?.data?.message ||
                    'Unable to change password.'
                )
            }
            finally {

                setChangingPassword(
                    false
                )
            }
        }

    // ========================================================
    // LOADING
    // ========================================================

    if (loading) {

        return (

            <main className="profile-page">

                <div className="profile-container">

                    <div className="profile-loading">

                        Loading profile...

                    </div>

                </div>

            </main>
        )
    }

    // ========================================================
    // PAGE
    // ========================================================

    return (

        <main className="profile-page">

            <div className="profile-container">

                {/* =============================================
                    HEADER
                   ============================================= */}

                <header className="profile-header">

                    <span className="profile-eyebrow">
                        Account Settings
                    </span>

                    <h1>
                        My Profile
                    </h1>

                    <p>
                        Manage your personal information
                        and account security.
                    </p>

                </header>

                {/* =============================================
                    ACCOUNT SUMMARY
                   ============================================= */}

                {profile && (

                    <section className="profile-summary">

                        <div className="profile-avatar">

                            {
                                (
                                    profile.firstName?.[0] ||
                                    'U'
                                ).toUpperCase()
                            }

                            {
                                (
                                    profile.lastName?.[0] ||
                                    ''
                                ).toUpperCase()
                            }

                        </div>

                        <div>

                            <h2>
                                {profile.firstName}{' '}
                                {profile.lastName}
                            </h2>

                            <p>
                                {profile.email}
                            </p>

                            <div className="profile-badges">

                                <span className="profile-role-badge">

                                    {profile.role}

                                </span>

                                {
                                    profile.isMainAdmin &&
                                    (
                                        <span className="main-admin-badge">

                                            Main Admin

                                        </span>
                                    )
                                }

                            </div>

                        </div>

                    </section>

                )}

                <div className="profile-grid">

                    {/* =========================================
                        PERSONAL INFORMATION
                       ========================================= */}

                    <section className="profile-card">

                        <div className="profile-card-header">

                            <h2>
                                Personal Information
                            </h2>

                            <p>
                                Update your customer account details.
                            </p>

                        </div>

                        {
                            error &&
                            (
                                <div
                                    className="profile-alert error"
                                    role="alert"
                                >
                                    {error}
                                </div>
                            )
                        }

                        {
                            success &&
                            (
                                <div
                                    className="profile-alert success"
                                    role="status"
                                >
                                    {success}
                                </div>
                            )
                        }

                        <form
                            onSubmit={
                                handleProfileSubmit
                            }
                        >

                            <div className="profile-form-row">

                                <div className="profile-field">

                                    <label htmlFor="firstName">

                                        First Name

                                    </label>

                                    <input
                                        id="firstName"
                                        name="firstName"
                                        type="text"
                                        value={
                                            form.firstName
                                        }
                                        onChange={
                                            handleProfileChange
                                        }
                                        required
                                    />

                                </div>

                                <div className="profile-field">

                                    <label htmlFor="lastName">

                                        Last Name

                                    </label>

                                    <input
                                        id="lastName"
                                        name="lastName"
                                        type="text"
                                        value={
                                            form.lastName
                                        }
                                        onChange={
                                            handleProfileChange
                                        }
                                        required
                                    />

                                </div>

                            </div>

                            <div className="profile-field">

                                <label htmlFor="email">

                                    Email Address

                                </label>

                                <input
                                    id="email"
                                    name="email"
                                    type="email"
                                    value={
                                        form.email
                                    }
                                    onChange={
                                        handleProfileChange
                                    }
                                    required
                                />

                            </div>

                            <div className="profile-field">

                                <label htmlFor="phoneNumber">

                                    Phone Number

                                </label>

                                <input
                                    id="phoneNumber"
                                    name="phoneNumber"
                                    type="tel"
                                    value={
                                        form.phoneNumber
                                    }
                                    onChange={
                                        handleProfileChange
                                    }
                                    placeholder="Enter phone number"
                                />

                            </div>

                            <button
                                type="submit"
                                className="profile-primary-button"
                                disabled={
                                    saving
                                }
                            >

                                {
                                    saving
                                        ? 'Saving...'
                                        : 'Save Changes'
                                }

                            </button>

                        </form>

                    </section>

                    {/* =========================================
                        SECURITY
                       ========================================= */}

                    <section className="profile-card">

                        <div className="profile-card-header">

                            <h2>
                                Security
                            </h2>

                            <p>
                                Change your account password.
                            </p>

                        </div>

                        {
                            passwordError &&
                            (
                                <div
                                    className="profile-alert error"
                                    role="alert"
                                >
                                    {passwordError}
                                </div>
                            )
                        }

                        {
                            passwordSuccess &&
                            (
                                <div
                                    className="profile-alert success"
                                    role="status"
                                >
                                    {passwordSuccess}
                                </div>
                            )
                        }

                        <form
                            onSubmit={
                                handlePasswordSubmit
                            }
                        >

                            <div className="profile-field">

                                <label htmlFor="currentPassword">

                                    Current Password

                                </label>

                                <input
                                    id="currentPassword"
                                    name="currentPassword"
                                    type="password"
                                    autoComplete="current-password"
                                    value={
                                        passwordForm.currentPassword
                                    }
                                    onChange={
                                        handlePasswordChange
                                    }
                                    required
                                />

                            </div>

                            <div className="profile-field">

                                <label htmlFor="newPassword">

                                    New Password

                                </label>

                                <input
                                    id="newPassword"
                                    name="newPassword"
                                    type="password"
                                    autoComplete="new-password"
                                    value={
                                        passwordForm.newPassword
                                    }
                                    onChange={
                                        handlePasswordChange
                                    }
                                    required
                                    minLength="8"
                                />

                            </div>

                            <div className="profile-field">

                                <label htmlFor="confirmPassword">

                                    Confirm New Password

                                </label>

                                <input
                                    id="confirmPassword"
                                    name="confirmPassword"
                                    type="password"
                                    autoComplete="new-password"
                                    value={
                                        passwordForm.confirmPassword
                                    }
                                    onChange={
                                        handlePasswordChange
                                    }
                                    required
                                    minLength="8"
                                />

                            </div>

                            <button
                                type="submit"
                                className="profile-secondary-button"
                                disabled={
                                    changingPassword
                                }
                            >

                                {
                                    changingPassword
                                        ? 'Changing Password...'
                                        : 'Change Password'
                                }

                            </button>

                        </form>

                    </section>

                </div>

            </div>

        </main>
    )
}

export default ProfilePage