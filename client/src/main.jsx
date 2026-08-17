import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'

import './index.css'

import App from './App.jsx'
import { AuthProvider } from './context/AuthContext.jsx'

// ============================================================
// APPLICATION ENTRY POINT
// ============================================================
//
// AuthProvider wraps the complete application.
//
// Because <App /> is inside AuthProvider, every page and
// component inside App can access authentication information.
// ============================================================

createRoot(
    document.getElementById('root')
).render(
    <StrictMode>

        <AuthProvider>
            <App />
        </AuthProvider>

    </StrictMode>,
)