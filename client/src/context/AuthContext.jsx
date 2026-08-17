import {
    createContext,
    useContext,
    useState
} from 'react'

// ============================================================
// AUTH CONTEXT
// ============================================================
//
// Context allows authentication information to be shared
// throughout the React application.
//
// Without Context, we would have to manually pass the logged-in
// user and token from component to component using props.
// ============================================================

const AuthContext = createContext(null)

// ============================================================
// AUTH PROVIDER
// ============================================================
//
// AuthProvider will wrap our application.
//
// Any component inside AuthProvider will be able to access
// authentication information.
// ============================================================

export function AuthProvider({ children }) {

    // ----------------------------------------------------------
    // Access Token
    // ----------------------------------------------------------
    //
    // Read the existing token from localStorage when React starts.
    // This means refreshing the browser will not immediately lose
    // the login session.
    // ----------------------------------------------------------

    const [accessToken, setAccessToken] = useState(
        () => localStorage.getItem('accessToken')
    )

    // ----------------------------------------------------------
    // User
    // ----------------------------------------------------------

    const [user, setUser] = useState(() => {

        const savedUser =
            localStorage.getItem('user')

        if (!savedUser) {
            return null
        }

        try {
            return JSON.parse(savedUser)
        } catch {
            return null
        }
    })

    // ----------------------------------------------------------
    // Login
    // ----------------------------------------------------------
    //
    // This function will be called after the backend successfully
    // authenticates the user.
    // ----------------------------------------------------------

    const login = (loginResponse) => {

        const token =
            loginResponse.accessToken

        const loggedInUser = {
            userId: loginResponse.userId,
            firstName: loginResponse.firstName,
            lastName: loginResponse.lastName,
            email: loginResponse.email,
            role: loginResponse.role
        }

        // Save authentication information in browser storage.

        localStorage.setItem(
            'accessToken',
            token
        )

        localStorage.setItem(
            'user',
            JSON.stringify(loggedInUser)
        )

        // Update React state.

        setAccessToken(token)

        setUser(loggedInUser)
    }

    // ----------------------------------------------------------
    // Logout
    // ----------------------------------------------------------

    const logout = () => {

        // Remove authentication information from browser storage.

        localStorage.removeItem(
            'accessToken'
        )

        localStorage.removeItem(
            'user'
        )

        // Clear React state.

        setAccessToken(null)

        setUser(null)
    }

    // ----------------------------------------------------------
    // Authentication Status
    // ----------------------------------------------------------

    const isAuthenticated =
        Boolean(accessToken)

    // ----------------------------------------------------------
    // Context Value
    // ----------------------------------------------------------
    //
    // Everything placed here becomes available to components
    // using useAuth().
    // ----------------------------------------------------------

    const value = {
        user,
        accessToken,
        isAuthenticated,
        login,
        logout
    }

    return (
        <AuthContext.Provider value={value}>
            {children}
        </AuthContext.Provider>
    )
}

// ============================================================
// CUSTOM HOOK
// ============================================================
//
// Instead of importing and using useContext(AuthContext)
// everywhere, our components can simply use:
//
// const { user, logout } = useAuth()
//
// ============================================================

export function useAuth() {

    const context =
        useContext(AuthContext)

    if (!context) {
        throw new Error(
            'useAuth must be used inside AuthProvider.'
        )
    }

    return context
}