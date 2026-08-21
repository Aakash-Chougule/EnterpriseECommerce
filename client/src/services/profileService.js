import apiClient from '../api/apiClient'

// ============================================================
// PROFILE SERVICE
// ============================================================
//
// React
//   ↓
// profileService
//   ↓
// apiClient
//   ↓
// ASP.NET ProfileController
// ============================================================

// ============================================================
// GET CURRENT USER PROFILE
// ============================================================
//
// GET /api/Profile
// ============================================================

export async function getProfile() {

    const response =
        await apiClient.get(
            '/Profile'
        )

    return response.data
}

// ============================================================
// UPDATE CURRENT USER PROFILE
// ============================================================
//
// PUT /api/Profile
//
// {
//     firstName,
//     lastName,
//     email,
//     phoneNumber
// }
// ============================================================

export async function updateProfile(
    profile
) {

    const response =
        await apiClient.put(
            '/Profile',
            {
                firstName:
                    profile.firstName,

                lastName:
                    profile.lastName,

                email:
                    profile.email,

                phoneNumber:
                    profile.phoneNumber || null
            }
        )

    return response.data
}

// ============================================================
// CHANGE PASSWORD
// ============================================================
//
// PUT /api/Profile/password
// ============================================================

export async function changePassword(
    currentPassword,
    newPassword
) {

    const response =
        await apiClient.put(
            '/Profile/password',
            {
                currentPassword,
                newPassword
            }
        )

    return response.data
}