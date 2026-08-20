import axios from 'axios'

// ============================================================
// AXIOS API CLIENT
// ============================================================
//
// LOCAL DEVELOPMENT MODE:
//
// React:
// http://localhost:5173
//
// ASP.NET API:
// http://localhost:5042
// ============================================================

const apiClient = axios.create({
    baseURL: 'http://localhost:5042/api',

    headers: {
        'Content-Type': 'application/json'
    }
})

// ============================================================
// REQUEST INTERCEPTOR
// ============================================================
//
// Automatically add JWT token to authenticated requests.
// ============================================================

apiClient.interceptors.request.use(
    (config) => {

        const accessToken =
            localStorage.getItem(
                'accessToken'
            )

        if (accessToken) {

            config.headers.Authorization =
                `Bearer ${accessToken}`
        }

        return config
    },

    (error) => {

        return Promise.reject(
            error
        )
    }
)

// ============================================================
// RESPONSE INTERCEPTOR
// ============================================================
//
// Remove invalid/expired JWT when backend returns 401.
// ============================================================

apiClient.interceptors.response.use(
    (response) => {

        return response
    },

    (error) => {

        if (
            error.response?.status === 401
        ) {
            localStorage.removeItem(
                'accessToken'
            )
        }

        return Promise.reject(
            error
        )
    }
)

// ============================================================
// EXPORT
// ============================================================

export default apiClient