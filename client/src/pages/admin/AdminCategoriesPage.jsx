import {
    useEffect,
    useMemo,
    useState
} from 'react'

import {
    getCategories,
    createCategory,
    updateCategory,
    deactivateCategory
} from '../../services/categoryService'

import './AdminCategoriesPage.css'

// ============================================================
// ADMIN CATEGORIES PAGE
// ============================================================

function AdminCategoriesPage() {

    const [categories, setCategories] =
        useState([])

    const [name, setName] =
        useState('')

    const [description, setDescription] =
        useState('')

    const [
        editingCategoryId,
        setEditingCategoryId
    ] = useState(null)

    const [loading, setLoading] =
        useState(true)

    const [saving, setSaving] =
        useState(false)

    const [error, setError] =
        useState('')

    const [message, setMessage] =
        useState('')

    const [search, setSearch] =
        useState('')

    // ========================================================
    // LOAD CATEGORIES
    // ========================================================

    const loadCategories = async () => {

        try {

            setLoading(true)
            setError('')

            const data =
                await getCategories()

            setCategories(
                Array.isArray(data)
                    ? data
                    : []
            )
        }
        catch (err) {

            console.error(
                'Failed to load categories:',
                err
            )

            setError(
                err.response?.data?.message ||
                'Unable to load categories.'
            )
        }
        finally {

            setLoading(false)
        }
    }

    // ========================================================
    // RESET FORM
    // ========================================================

    const resetForm = () => {

        setName('')
        setDescription('')
        setEditingCategoryId(null)
    }

    // ========================================================
    // START EDIT
    // ========================================================

    const handleEdit =
        (category) => {

            setEditingCategoryId(
                category.id
            )

            setName(
                category.name
            )

            setDescription(
                category.description ?? ''
            )

            setError('')
            setMessage('')

            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            })
        }

    // ========================================================
    // CREATE / UPDATE
    // ========================================================

    const handleSubmit =
        async (event) => {

            event.preventDefault()

            setError('')
            setMessage('')

            if (!name.trim()) {

                setError(
                    'Category name is required.'
                )

                return
            }

            try {

                setSaving(true)

                if (editingCategoryId) {

                    await updateCategory(
                        editingCategoryId,
                        name.trim(),
                        description.trim()
                    )

                    setMessage(
                        'Category updated successfully.'
                    )
                }
                else {

                    await createCategory(
                        name.trim(),
                        description.trim()
                    )

                    setMessage(
                        'Category created successfully.'
                    )
                }

                resetForm()

                await loadCategories()
            }
            catch (err) {

                console.error(
                    'Category save failed:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Unable to save category.'
                )
            }
            finally {

                setSaving(false)
            }
        }

    // ========================================================
    // DEACTIVATE
    // ========================================================

    const handleDeactivate =
        async (categoryId) => {

            const confirmed =
                window.confirm(
                    'Are you sure you want to deactivate this category?'
                )

            if (!confirmed) {
                return
            }

            try {

                setError('')
                setMessage('')

                await deactivateCategory(
                    categoryId
                )

                setMessage(
                    'Category deactivated successfully.'
                )

                if (
                    editingCategoryId ===
                    categoryId
                ) {
                    resetForm()
                }

                await loadCategories()
            }
            catch (err) {

                console.error(
                    'Category deactivation failed:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Unable to deactivate category.'
                )
            }
        }

    // ========================================================
    // INITIAL LOAD
    // ========================================================

    useEffect(() => {

        loadCategories()

    }, [])

    // ========================================================
    // SEARCH
    // ========================================================

    const filteredCategories =
        useMemo(() => {

            const value =
                search
                    .trim()
                    .toLowerCase()

            if (!value) {
                return categories
            }

            return categories.filter(
                category => {

                    const searchable = `
                        ${category.name ?? ''}
                        ${category.description ?? ''}
                    `.toLowerCase()

                    return searchable.includes(
                        value
                    )
                }
            )

        }, [categories, search])

    // ========================================================
    // UI
    // ========================================================

    return (

        <main className="admin-categories-page">

            <div className="admin-categories-container">

                {/* ==============================================
                    HEADER
                   ============================================== */}

                <header className="admin-categories-header">

                    <div>

                        <span className="admin-categories-eyebrow">
                            Category Management
                        </span>

                        <h1>
                            Admin Categories
                        </h1>

                        <p>
                            Create, update and organize
                            product categories.
                        </p>

                    </div>

                    <div className="admin-categories-count">

                        <strong>
                            {categories.length}
                        </strong>

                        <span>
                            Categories
                        </span>

                    </div>

                </header>

                {/* ==============================================
                    ALERTS
                   ============================================== */}

                {error && (

                    <div className="admin-category-alert error">

                        <span>
                            !
                        </span>

                        {error}

                    </div>

                )}

                {message && (

                    <div className="admin-category-alert success">

                        <span>
                            ✓
                        </span>

                        {message}

                    </div>

                )}

                {/* ==============================================
                    FORM
                   ============================================== */}

                <section className="admin-category-form-card">

                    <div className="admin-category-form-header">

                        <div>

                            <span>
                                {
                                    editingCategoryId
                                        ? 'Editing'
                                        : 'New Category'
                                }
                            </span>

                            <h2>
                                {
                                    editingCategoryId
                                        ? 'Edit Category'
                                        : 'Create Category'
                                }
                            </h2>

                            <p>
                                {
                                    editingCategoryId
                                        ? 'Update the selected category information.'
                                        : 'Add a new category for your products.'
                                }
                            </p>

                        </div>

                        {editingCategoryId && (

                            <div className="admin-category-editing-badge">
                                Edit Mode
                            </div>

                        )}

                    </div>

                    <form
                        className="admin-category-form"
                        onSubmit={handleSubmit}
                    >

                        <div className="admin-category-field">

                            <label
                                htmlFor="categoryName"
                            >
                                Category Name
                            </label>

                            <input
                                id="categoryName"
                                type="text"
                                value={name}
                                placeholder="Enter category name"
                                onChange={
                                    event =>
                                        setName(
                                            event.target.value
                                        )
                                }
                            />

                        </div>

                        <div className="admin-category-field">

                            <label
                                htmlFor="categoryDescription"
                            >
                                Description
                            </label>

                            <textarea
                                id="categoryDescription"
                                value={description}
                                rows="4"
                                placeholder="Enter category description"
                                onChange={
                                    event =>
                                        setDescription(
                                            event.target.value
                                        )
                                }
                            />

                        </div>

                        <div className="admin-category-form-actions">

                            {editingCategoryId && (

                                <button
                                    type="button"
                                    className="admin-category-cancel-button"
                                    onClick={resetForm}
                                    disabled={saving}
                                >
                                    Cancel Edit
                                </button>

                            )}

                            <button
                                type="submit"
                                className="admin-category-save-button"
                                disabled={saving}
                            >
                                {
                                    saving
                                        ? 'Saving...'
                                        : editingCategoryId
                                            ? 'Update Category'
                                            : 'Create Category'
                                }
                            </button>

                        </div>

                    </form>

                </section>

                {/* ==============================================
                    CATEGORY LIST
                   ============================================== */}

                <section className="admin-category-list-section">

                    <div className="admin-category-list-header">

                        <div>

                            <span>
                                Catalog
                            </span>

                            <h2>
                                Categories
                            </h2>

                        </div>

                        <span className="admin-category-total">
                            {categories.length}
                            {' '}
                            {
                                categories.length === 1
                                    ? 'category'
                                    : 'categories'
                            }
                        </span>

                    </div>

                    <div className="admin-category-toolbar">

                        <div className="admin-category-search">

                            <input
                                type="search"
                                value={search}
                                placeholder="Search by category name or description..."
                                onChange={
                                    event =>
                                        setSearch(
                                            event.target.value
                                        )
                                }
                            />

                            {search && (

                                <button
                                    type="button"
                                    onClick={() =>
                                        setSearch('')
                                    }
                                >
                                    ×
                                </button>

                            )}

                        </div>

                        <span className="admin-category-showing">

                            Showing{' '}

                            <strong>
                                {
                                    filteredCategories.length
                                }
                            </strong>

                            {' '}of{' '}

                            {categories.length}

                        </span>

                    </div>

                    {loading ? (

                        <div className="admin-category-loading">

                            <div className="admin-category-spinner" />

                            <p>
                                Loading categories...
                            </p>

                        </div>

                    ) : filteredCategories.length === 0 ? (

                        <div className="admin-category-empty">

                            <h3>
                                No categories found
                            </h3>

                            <p>
                                Try another search term
                                or create a new category.
                            </p>

                        </div>

                    ) : (

                        <div className="admin-category-grid">

                            {filteredCategories.map(
                                category => (

                                    <article
                                        className="admin-category-card"
                                        key={category.id}
                                    >

                                        <div className="admin-category-card-top">

                                            <div className="admin-category-icon">

                                                {category.name
                                                    ?.charAt(0)
                                                    .toUpperCase()
                                                    || 'C'}

                                            </div>

                                            <span className="admin-category-active-badge">
                                                Active
                                            </span>

                                        </div>

                                        <h3>
                                            {category.name}
                                        </h3>

                                        <p>
                                            {
                                                category.description ||
                                                'No description available.'
                                            }
                                        </p>

                                        <div className="admin-category-card-actions">

                                            <button
                                                type="button"
                                                className="admin-category-edit-button"
                                                onClick={() =>
                                                    handleEdit(
                                                        category
                                                    )
                                                }
                                            >
                                                Edit
                                            </button>

                                            <button
                                                type="button"
                                                className="admin-category-deactivate-button"
                                                onClick={() =>
                                                    handleDeactivate(
                                                        category.id
                                                    )
                                                }
                                            >
                                                Deactivate
                                            </button>

                                        </div>

                                    </article>

                                )
                            )}

                        </div>

                    )}

                </section>

            </div>

        </main>
    )
}

export default AdminCategoriesPage