import {
    useEffect,
    useMemo,
    useState
} from 'react'

import apiClient
    from '../../api/apiClient'

import './AdminUsersPage.css'

function AdminUsersPage() {

    const [users, setUsers] =
        useState([])

    const [permissions, setPermissions] =
        useState([])

    const [loading, setLoading] =
        useState(true)

    const [saving, setSaving] =
        useState(false)

    const [error, setError] =
        useState('')

    const [success, setSuccess] =
        useState('')

    const [search, setSearch] =
        useState('')

    const [selectedUser, setSelectedUser] =
        useState(null)

    // ========================================================
    // LOAD
    // ========================================================

    const loadData =
        async () => {

            try {

                setLoading(true)
                setError('')

                const [
                    usersResponse,
                    permissionsResponse
                ] =
                    await Promise.all([
                        apiClient.get(
                            '/admin/users'
                        ),

                        apiClient.get(
                            '/admin/users/permissions'
                        )
                    ])

                setUsers(
                    usersResponse.data
                )

                setPermissions(
                    permissionsResponse.data
                )
            }
            catch (err) {

                console.error(
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Unable to load users.'
                )
            }
            finally {

                setLoading(false)
            }
        }

    useEffect(() => {

        loadData()

    }, [])

    // ========================================================
    // SEARCH
    // ========================================================

    const filteredUsers =
        useMemo(
            () => {

                const term =
                    search
                        .trim()
                        .toLowerCase()

                if (!term) {

                    return users
                }

                return users.filter(
                    user => {

                        const text =
                            [
                                user.firstName,
                                user.lastName,
                                user.email,
                                user.phoneNumber,
                                user.role
                            ]
                                .join(' ')
                                .toLowerCase()

                        return text.includes(
                            term
                        )
                    }
                )
            },
            [
                users,
                search
            ]
        )

    // ========================================================
    // OPEN USER
    // ========================================================

    const openUser =
        user => {

            setSelectedUser({
                ...user,

                permissions:
                    Array.isArray(
                        user.permissions
                    )
                        ? [
                            ...user.permissions
                        ]
                        : []
            })

            setError('')
            setSuccess('')
        }

    // ========================================================
    // UPDATE FIELD
    // ========================================================

    const updateField =
        (
            name,
            value
        ) => {

            setSelectedUser(
                current => ({
                    ...current,
                    [name]: value
                })
            )
        }

    // ========================================================
    // PERMISSION
    // ========================================================

    const togglePermission =
        permission => {

            setSelectedUser(
                current => {

                    const list =
                        current.permissions || []

                    const exists =
                        list.includes(
                            permission
                        )

                    return {
                        ...current,

                        permissions:
                            exists
                                ? list.filter(
                                    item =>
                                        item !==
                                        permission
                                )
                                : [
                                    ...list,
                                    permission
                                ]
                    }
                }
            )
        }

    // ========================================================
    // SAVE INFORMATION
    // ========================================================

    const saveInformation =
        async () => {

            if (!selectedUser) {

                return
            }

            try {

                setSaving(true)
                setError('')
                setSuccess('')

                const response =
                    await apiClient.put(
                        `/admin/users/${selectedUser.id}`,
                        {
                            firstName:
                                selectedUser.firstName,

                            lastName:
                                selectedUser.lastName,

                            email:
                                selectedUser.email,

                            phoneNumber:
                                selectedUser.phoneNumber ||
                                null,

                            isActive:
                                selectedUser.isActive
                        }
                    )

                setSelectedUser(
                    response.data
                )

                setSuccess(
                    'User updated successfully.'
                )

                await loadData()
            }
            catch (err) {

                setError(
                    err.response?.data?.message ||
                    'Unable to update user.'
                )
            }
            finally {

                setSaving(false)
            }
        }

    // ========================================================
    // PROMOTE
    // ========================================================

    const promote =
        async () => {

            try {

                setSaving(true)
                setError('')
                setSuccess('')

                const response =
                    await apiClient.post(
                        `/admin/users/${selectedUser.id}/promote`,
                        {
                            permissions:
                                selectedUser.permissions ||
                                []
                        }
                    )

                setSelectedUser(
                    response.data
                )

                setSuccess(
                    'User promoted to Admin.'
                )

                await loadData()
            }
            catch (err) {

                setError(
                    err.response?.data?.message ||
                    'Unable to promote user.'
                )
            }
            finally {

                setSaving(false)
            }
        }

    // ========================================================
    // DEMOTE
    // ========================================================

    const demote =
        async () => {

            try {

                setSaving(true)
                setError('')
                setSuccess('')

                const response =
                    await apiClient.post(
                        `/admin/users/${selectedUser.id}/demote`
                    )

                setSelectedUser(
                    response.data
                )

                setSuccess(
                    'Admin demoted to Customer.'
                )

                await loadData()
            }
            catch (err) {

                setError(
                    err.response?.data?.message ||
                    'Unable to demote admin.'
                )
            }
            finally {

                setSaving(false)
            }
        }

    // ========================================================
    // SAVE PERMISSIONS
    // ========================================================

    const savePermissions =
        async () => {

            try {

                setSaving(true)
                setError('')
                setSuccess('')

                const response =
                    await apiClient.put(
                        `/admin/users/${selectedUser.id}/permissions`,
                        {
                            permissions:
                                selectedUser.permissions
                        }
                    )

                setSelectedUser(
                    response.data
                )

                setSuccess(
                    'Permissions updated successfully.'
                )

                await loadData()
            }
            catch (err) {

                setError(
                    err.response?.data?.message ||
                    'Unable to update permissions.'
                )
            }
            finally {

                setSaving(false)
            }
        }

    if (loading) {

        return (

            <main className="admin-users-page">

                <div className="admin-users-container">

                    Loading users...

                </div>

            </main>
        )
    }

    return (

        <main className="admin-users-page">

            <div className="admin-users-container">

                <header className="admin-users-header">

                    <div>

                        <span className="admin-users-eyebrow">
                            Main Administration
                        </span>

                        <h1>
                            Users & Admins
                        </h1>

                        <p>
                            Manage customers, administrators
                            and system permissions.
                        </p>

                    </div>

                    <div className="admin-users-count">

                        <strong>
                            {users.length}
                        </strong>

                        <span>
                            Users
                        </span>

                    </div>

                </header>

                {error && (

                    <div className="admin-users-alert error">
                        {error}
                    </div>

                )}

                {success && (

                    <div className="admin-users-alert success">
                        {success}
                    </div>

                )}

                <section className="admin-users-toolbar">

                    <input
                        type="search"
                        placeholder="Search users..."
                        value={
                            search
                        }
                        onChange={
                            event =>
                                setSearch(
                                    event.target.value
                                )
                        }
                    />

                </section>

                <section className="admin-users-card">

                    <div className="admin-users-table-wrapper">

                        <table className="admin-users-table">

                            <thead>

                                <tr>

                                    <th>User</th>

                                    <th>Email</th>

                                    <th>Role</th>

                                    <th>Status</th>

                                    <th>Access</th>

                                    <th />

                                </tr>

                            </thead>

                            <tbody>

                                {filteredUsers.map(
                                    user => (

                                        <tr key={user.id}>

                                            <td>

                                                <strong>

                                                    {user.firstName}{' '}
                                                    {user.lastName}

                                                </strong>

                                                {user.isMainAdmin && (

                                                    <div className="main-admin-small-badge">

                                                        Main Admin

                                                    </div>

                                                )}

                                            </td>

                                            <td>
                                                {user.email}
                                            </td>

                                            <td>
                                                {user.role}
                                            </td>

                                            <td>

                                                {
                                                    user.isActive
                                                        ? 'Active'
                                                        : 'Inactive'
                                                }

                                            </td>

                                            <td>

                                                {
                                                    user.isMainAdmin
                                                        ? 'Full Access'
                                                        : `${user.permissions?.length || 0} permissions`
                                                }

                                            </td>

                                            <td>

                                                <button
                                                    type="button"
                                                    className="manage-user-button"
                                                    onClick={
                                                        () =>
                                                            openUser(
                                                                user
                                                            )
                                                    }
                                                >
                                                    Manage
                                                </button>

                                            </td>

                                        </tr>

                                    )
                                )}

                            </tbody>

                        </table>

                    </div>

                </section>

                {/* ==================================================
                    USER MODAL
                   ================================================== */}

                {selectedUser && (

                    <div className="admin-user-modal-backdrop">

                        <section className="admin-user-modal">

                            <div className="admin-user-modal-header">

                                <div>

                                    <span className="admin-users-eyebrow">
                                        Account Management
                                    </span>

                                    <h2>

                                        {selectedUser.firstName}{' '}
                                        {selectedUser.lastName}

                                    </h2>

                                </div>

                                <button
                                    type="button"
                                    className="admin-user-modal-close"
                                    onClick={
                                        () =>
                                            setSelectedUser(
                                                null
                                            )
                                    }
                                >
                                    ×
                                </button>

                            </div>

                            <div className="admin-user-form-grid">

                                <label>

                                    First Name

                                    <input
                                        value={
                                            selectedUser.firstName ||
                                            ''
                                        }
                                        onChange={
                                            event =>
                                                updateField(
                                                    'firstName',
                                                    event.target.value
                                                )
                                        }
                                    />

                                </label>

                                <label>

                                    Last Name

                                    <input
                                        value={
                                            selectedUser.lastName ||
                                            ''
                                        }
                                        onChange={
                                            event =>
                                                updateField(
                                                    'lastName',
                                                    event.target.value
                                                )
                                        }
                                    />

                                </label>

                                <label>

                                    Email

                                    <input
                                        type="email"
                                        value={
                                            selectedUser.email ||
                                            ''
                                        }
                                        onChange={
                                            event =>
                                                updateField(
                                                    'email',
                                                    event.target.value
                                                )
                                        }
                                    />

                                </label>

                                <label>

                                    Phone

                                    <input
                                        value={
                                            selectedUser.phoneNumber ||
                                            ''
                                        }
                                        onChange={
                                            event =>
                                                updateField(
                                                    'phoneNumber',
                                                    event.target.value
                                                )
                                        }
                                    />

                                </label>

                            </div>

                            <label className="admin-user-checkbox">

                                <input
                                    type="checkbox"
                                    checked={
                                        selectedUser.isActive
                                    }
                                    disabled={
                                        selectedUser.isMainAdmin
                                    }
                                    onChange={
                                        event =>
                                            updateField(
                                                'isActive',
                                                event.target.checked
                                            )
                                    }
                                />

                                Active Account

                            </label>

                            <button
                                type="button"
                                className="admin-user-primary-button"
                                disabled={
                                    saving
                                }
                                onClick={
                                    saveInformation
                                }
                            >
                                Save User Information
                            </button>

                            {/* ==================================================
                                ROLE
                               ================================================== */}

                            <div className="admin-user-section">

                                <h3>
                                    Role Management
                                </h3>

                                <p>
                                    Current Role:
                                    {' '}
                                    <strong>
                                        {selectedUser.role}
                                    </strong>
                                </p>

                                {
                                    !selectedUser.isMainAdmin &&
                                    selectedUser.role !== 'Admin' &&
                                    (
                                        <button
                                            type="button"
                                            className="admin-user-primary-button"
                                            disabled={
                                                saving
                                            }
                                            onClick={
                                                promote
                                            }
                                        >
                                            Promote to Admin
                                        </button>
                                    )
                                }

                                {
                                    !selectedUser.isMainAdmin &&
                                    selectedUser.role === 'Admin' &&
                                    (
                                        <button
                                            type="button"
                                            className="admin-user-danger-button"
                                            disabled={
                                                saving
                                            }
                                            onClick={
                                                demote
                                            }
                                        >
                                            Demote to Customer
                                        </button>
                                    )
                                }

                            </div>

                            {/* ==================================================
                                PERMISSIONS
                               ================================================== */}

                            {selectedUser.role === 'Admin' && (

                                <div className="admin-user-section">

                                    <h3>
                                        Access Permissions
                                    </h3>

                                    {selectedUser.isMainAdmin ? (

                                        <div className="main-admin-access-message">

                                            Main Admin has permanent
                                            full system access.

                                        </div>

                                    ) : (

                                        <>

                                            <div className="permission-grid">

                                                {permissions.map(
                                                    permission => (

                                                        <label
                                                            key={
                                                                permission
                                                            }
                                                            className="permission-option"
                                                        >

                                                            <input
                                                                type="checkbox"
                                                                checked={
                                                                    selectedUser
                                                                        .permissions
                                                                        ?.includes(
                                                                            permission
                                                                        )
                                                                    ||
                                                                    false
                                                                }
                                                                onChange={
                                                                    () =>
                                                                        togglePermission(
                                                                            permission
                                                                        )
                                                                }
                                                            />

                                                            <span>
                                                                {permission}
                                                            </span>

                                                        </label>

                                                    )
                                                )}

                                            </div>

                                            <button
                                                type="button"
                                                className="admin-user-primary-button"
                                                disabled={
                                                    saving
                                                }
                                                onClick={
                                                    savePermissions
                                                }
                                            >
                                                Save Permissions
                                            </button>

                                        </>

                                    )}

                                </div>

                            )}

                        </section>

                    </div>

                )}

            </div>

        </main>
    )
}

export default AdminUsersPage