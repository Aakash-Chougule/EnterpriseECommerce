// ============================================================
// ADMIN DASHBOARD PAGE
// ============================================================
//
// This page is only accessible to users with role = "Admin".
//
// Later this dashboard can show:
// - Product count
// - Order count
// - Pending orders
// - Revenue
// - User count
// ============================================================

function AdminDashboardPage() {
    return (
        <div>

            <h1>
                Admin Dashboard
            </h1>

            <p>
                Welcome to the administration area.
            </p>

            <div>

                <h2>
                    Management
                </h2>

                <p>
                    Product management will be added here.
                </p>

                <p>
                    Order management will be added here.
                </p>

            </div>

        </div>
    )
}

export default AdminDashboardPage