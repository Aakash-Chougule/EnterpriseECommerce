import axios from 'axios'

// Base URL of your ASP.NET Core API.
// Later we can move this into an environment variable.
const API_URL = 'http://localhost:5042/api/Auth'

// Sends login credentials to the backend.
export async function loginUser(email, password) {
    const response = await axios.post(
        `${API_URL}/login`,
        {
            email,
            password
        }
    )

    return response.data
}