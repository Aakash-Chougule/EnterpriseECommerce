import {
    useEffect,
    useState
} from 'react'

import {
    getCategories,
    createCategory,
    updateCategory,
    deactivateCategory
} from '../../services/categoryService'

// ============================================================
// ADMIN CATEGORIES PAGE
// ============================================================
//
// Admin can:
// - View categories
// - Create categories
// - Edit categories
// - Deactivate categories
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

    // ========================================================
    // LOAD CATEGORIES
    // ========================================================

    const loadCategories = async () => {

        try {

            setLoading(true)
            setError('')

            const data =
                await getCategories()

            setCategories(data)
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

                // --------------------------------------------
                // EDIT MODE
                // --------------------------------------------

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

                // --------------------------------------------
                // CREATE MODE
                // --------------------------------------------

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
    // DEACTIVATE CATEGORY
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

                // If we're editing the category that was
                // deactivated, reset the form.

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
    // UI
    // ========================================================

    return (
        <div>

            <h1>
                Category Management
            </h1>

            {error && (
                <p>
                    {error}
                </p>
            )}

            {message && (
                <p>
                    {message}
                </p>
            )}

            {/* ================================================
                CATEGORY FORM
               ================================================ */}

            <section>

                <h2>
                    {
                        editingCategoryId
                            ? 'Edit Category'
                            : 'Create Category'
                    }
                </h2>

                <form
                    onSubmit={handleSubmit}
                >

                    <div>

                        <label
                            htmlFor="categoryName"
                        >
                            Name
                        </label>

                        <br />

                        <input
                            id="categoryName"
                            type="text"

                            value={name}

                            onChange={
                                (event) =>
                                    setName(
                                        event.target.value
                                    )
                            }

                            placeholder="Enter category name"
                        />

                    </div>

                    <br />

                    <div>

                        <label
                            htmlFor="categoryDescription"
                        >
                            Description
                        </label>

                        <br />

                        <textarea
                            id="categoryDescription"

                            value={description}

                            onChange={
                                (event) =>
                                    setDescription(
                                        event.target.value
                                    )
                            }

                            placeholder="Enter category description"
                        />

                    </div>

                    <br />

                    <button
                        type="submit"
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

                    {editingCategoryId && (
                        <>

                            {' '}

                            <button
                                type="button"
                                onClick={resetForm}
                                disabled={saving}
                            >
                                Cancel Edit
                            </button>

                        </>
                    )}

                </form>

            </section>

            <hr />

            {/* ================================================
                CATEGORY LIST
               ================================================ */}

            <section>

                <h2>
                    Categories
                </h2>

                {loading ? (

                    <p>
                        Loading categories...
                    </p>

                ) : categories.length === 0 ? (

                    <p>
                        No active categories found.
                    </p>

                ) : (

                    categories.map(
                        (category) => (

                            <div
                                key={category.id}
                            >

                                <h3>
                                    {category.name}
                                </h3>

                                <p>
                                    <strong>
                                        Description:
                                    </strong>

                                    {' '}

                                    {
                                        category.description ||
                                        'No description'
                                    }
                                </p>

                                <button
                                    type="button"

                                    onClick={() =>
                                        handleEdit(
                                            category
                                        )
                                    }
                                >
                                    Edit
                                </button>

                                {' '}

                                <button
                                    type="button"

                                    onClick={() =>
                                        handleDeactivate(
                                            category.id
                                        )
                                    }
                                >
                                    Deactivate
                                </button>

                                <hr />

                            </div>

                        )
                    )

                )}

            </section>

        </div>
    )
}

export default AdminCategoriesPage