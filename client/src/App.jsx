import {
    BrowserRouter,
    Routes,
    Route,
    Link
} from 'react-router-dom'

import HomePage from './pages/HomePage'
import LoginPage from './pages/LoginPage'
import ProductsPage from './pages/ProductsPage'

function App() {
    return (
        <BrowserRouter>

            {/* 
        Temporary navigation.
        Later we will move this into a reusable Navbar component.
      */}
            <nav>
                <Link to="/">Home</Link>
                {' | '}
                <Link to="/login">Login</Link>
                {' | '}
                <Link to="/products">Products</Link>
            </nav>

            {/* 
        Routes decides which React component should appear
        depending on the current browser URL.
      */}
            <Routes>

                <Route
                    path="/"
                    element={<HomePage />}
                />

                <Route
                    path="/login"
                    element={<LoginPage />}
                />

                <Route
                    path="/products"
                    element={<ProductsPage />}
                />

            </Routes>

        </BrowserRouter>
    )
}

export default App