import axios from 'axios'

// ============================================================
// AUTH SERVICE
// ============================================================
//
// Handles frontend communication with AuthController.
//
// React Page
//     ↓
// authService
//     ↓
// Axios
//     ↓
// ASP.NET AuthController
// ============================================================

const API_URL =
    'http://localhost:5042/api/Auth'

// ------------------------------------------------------------
// LOGIN
// ------------------------------------------------------------

export async function loginUser(
    email,
    password
) {
    const response =
        await axios.post(
            `${API_URL}/login`,
            {
                email,
                password
            }
        )

    return response.data
}

// ------------------------------------------------------------
// REGISTER
// ------------------------------------------------------------
//
// Backend:
// POST /api/Auth/register
//
// Request body:
// {
//   firstName: "...",
//   lastName: "...",
//   email: "...",
//   password: "...",
//   phoneNumber: "..."
// }
// ------------------------------------------------------------

export async function registerUser(
    registerData
) {
    const response =
        await axios.post(
            `${API_URL}/register`,
            registerData
        )

    return response.data
}