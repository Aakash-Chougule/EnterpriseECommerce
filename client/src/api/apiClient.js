import axios from 'axios'

// ============================================================
// AXIOS API CLIENT
// ============================================================
//
// This creates one reusable Axios instance for communicating
// with our ASP.NET Core backend.
//
// Instead of writing the complete API URL everywhere:
//
// axios.get('http://localhost:5042/api/Products')
//
// we can simply write:
//
// apiClient.get('/Products')
//
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
// An interceptor runs BEFORE every HTTP request.
//
// We use it to automatically retrieve the JWT from localStorage
// and attach it to the Authorization header.
//
// Authorization: Bearer eyJhbGciOi...
//
// This means individual services don't need to manually add
// the JWT every time.
// ============================================================

apiClient.interceptors.request.use(
    (config) => {

        const accessToken =
            localStorage.getItem('accessToken')

        if (accessToken) {
            config.headers.Authorization =
                `Bearer ${accessToken}`
        }

        return config
    },

    (error) => {
        return Promise.reject(error)
    }
)

export default apiClient