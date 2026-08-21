import {
    useEffect,
    useState
} from 'react'

import {
    getProfile
} from '../../services/profileService'

import './AdminAccessPage.css'

function AdminAccessPage() {

    const [profile, setProfile] =
        useState(null)

    const [loading, setLoading] =
        useState(true)

    useEffect(() => {

        const load =
            async () => {

                try {

                    const data =
                        await getProfile()

                    setProfile(
                        data
                    )
                }
                finally {

                    setLoading(
                        false
                    )
                }
            }

        load()

    }, [])

    if (loading) {

        return (

            <main className="admin-access-page">

                Loading access...

            </main>
        )
    }

    return (

        <main className="admin-access-page">

            <div className="admin-access-container">

                <header>

                    <span className="admin-access-eyebrow">
                        Security & Authorization
                    </span>

                    <h1>
                        Full System Access
                    </h1>

                    <p>
                        Main Administrator access overview.
                    </p>

                </header>

                <section className="admin-access-owner">

                    <div className="admin-access-avatar">

                        {
                            profile?.firstName
                                ?.charAt(0)
                                ?.toUpperCase()
                            ||
                            'A'
                        }

                    </div>

                    <div>

                        <h2>

                            {profile?.firstName}{' '}
                            {profile?.lastName}

                        </h2>

                        <p>
                            {profile?.email}
                        </p>

                        <span className="admin-access-main-badge">
                            Main Admin
                        </span>

                    </div>

                </section>

                <section className="admin-access-card">

                    <h2>
                        System Permissions
                    </h2>

                    <p>
                        The Main Admin has permanent
                        access to every administrative
                        function.
                    </p>

                    <div className="admin-access-grid">

                        <AccessItem
                            title="Products"
                            permission="ManageProducts"
                        />

                        <AccessItem
                            title="Categories"
                            permission="ManageCategories"
                        />

                        <AccessItem
                            title="Inventory"
                            permission="ManageInventory"
                        />

                        <AccessItem
                            title="Orders"
                            permission="ManageOrders"
                        />

                        <AccessItem
                            title="Payments"
                            permission="ManagePayments"
                        />

                        <AccessItem
                            title="Users"
                            permission="ManageUsers"
                        />

                        <AccessItem
                            title="Administrators"
                            permission="ManageAdmins"
                        />

                        <AccessItem
                            title="Reports"
                            permission="ViewReports"
                        />

                    </div>

                </section>

                <section className="admin-access-warning">

                    <strong>
                        Main Admin Protection
                    </strong>

                    <p>
                        This account cannot be demoted,
                        deactivated or have its permissions
                        restricted by another administrator.
                    </p>

                </section>

            </div>

        </main>
    )
}

function AccessItem({
    title,
    permission
}) {

    return (

        <div className="admin-access-item">

            <span className="admin-access-check">
                ✓
            </span>

            <div>

                <strong>
                    {title}
                </strong>

                <span>
                    {permission}
                </span>

            </div>

        </div>
    )
}

export default AdminAccessPage