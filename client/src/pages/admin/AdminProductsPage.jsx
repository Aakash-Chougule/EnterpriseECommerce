import {
    useEffect,
    useState
} from 'react'

import {
    getProducts,
    createProduct,
    updateProduct,
    deactivateProduct
} from '../../services/productService'

import {
    getCategories
} from '../../services/categoryService'

// ============================================================
// ADMIN PRODUCTS PAGE
// ============================================================
//
// Admin capabilities:
// - View products
// - Create product
// - Edit product
// - Deactivate product
// - Select category from dropdown
//
// Later we can improve this page with:
// - Better styling
// - Search
// - Pagination
// - Image upload
// ============================================================

function AdminProductsPage() {

    // ==========================================================
    // PRODUCT LIST
    // ==========================================================

    const [products, setProducts] =
        useState([])

    // ==========================================================
    // CATEGORY LIST
    // ==========================================================
    //
    // Categories are loaded from:
    //
    // GET /api/Categories
    //
    // The dropdown displays category.name,
    // but stores category.id.
    // ==========================================================

    const [categories, setCategories] =
        useState([])

    // ==========================================================
    // FORM MODE
    // ==========================================================
    //
    // editingProductId = null
    //     → Create mode
    //
    // editingProductId = product id
    //     → Edit mode
    // ==========================================================

    const [
        editingProductId,
        setEditingProductId
    ] = useState(null)

    // ==========================================================
    // FORM STATE
    // ==========================================================

    const [formData, setFormData] =
        useState({
            categoryId: '',
            name: '',
            description: '',
            sku: '',
            price: '',
            stockQuantity: ''
        })

    // ==========================================================
    // UI STATE
    // ==========================================================

    const [loading, setLoading] =
        useState(true)

    const [saving, setSaving] =
        useState(false)

    const [error, setError] =
        useState('')

    const [message, setMessage] =
        useState('')

    // ==========================================================
    // LOAD PRODUCTS
    // ==========================================================

    const loadProducts = async () => {

        try {

            setLoading(true)

            setError('')

            const data =
                await getProducts()

            setProducts(data)
        }
        catch (err) {

            console.error(
                'Failed to load products:',
                err
            )

            setError(
                err.response?.data?.message ||
                'Unable to load products.'
            )
        }
        finally {

            setLoading(false)
        }
    }

    // ==========================================================
    // LOAD CATEGORIES
    // ==========================================================
    //
    // The backend returns active categories.
    //
    // Example:
    //
    // [
    //   {
    //     id: "...",
    //     name: "Electronics"
    //   }
    // ]
    //
    // ==========================================================

    const loadCategories = async () => {

        try {

            const data =
                await getCategories()

            console.log(
                'Categories received:',
                data
            )

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
    }

    // ==========================================================
    // HANDLE FORM INPUT
    // ==========================================================

    const handleChange =
        (event) => {

            const {
                name,
                value
            } = event.target

            setFormData(
                current => ({
                    ...current,

                    [name]: value
                })
            )
        }

    // ==========================================================
    // RESET FORM
    // ==========================================================

    const resetForm = () => {

        setEditingProductId(null)

        setFormData({
            categoryId: '',
            name: '',
            description: '',
            sku: '',
            price: '',
            stockQuantity: ''
        })
    }

    // ==========================================================
    // START EDITING
    // ==========================================================

    const handleEdit =
        (product) => {

            setEditingProductId(
                product.id
            )

            setFormData({
                categoryId:
                    product.categoryId,

                name:
                    product.name,

                description:
                    product.description,

                sku:
                    product.sku,

                price:
                    product.price,

                stockQuantity:
                    product.stockQuantity
            })

            setMessage('')
            setError('')
        }

    // ==========================================================
    // SAVE PRODUCT
    // ==========================================================

    const handleSubmit =
        async (event) => {

            event.preventDefault()

            setError('')
            setMessage('')

            // ------------------------------------------------------
            // PRODUCT NAME
            // ------------------------------------------------------

            if (!formData.name.trim()) {

                setError(
                    'Product name is required.'
                )

                return
            }

            // ------------------------------------------------------
            // PRICE
            // ------------------------------------------------------

            if (
                Number(formData.price) < 0
            ) {

                setError(
                    'Price cannot be negative.'
                )

                return
            }

            // ------------------------------------------------------
            // STOCK
            // ------------------------------------------------------

            if (
                Number(
                    formData.stockQuantity
                ) < 0
            ) {

                setError(
                    'Stock quantity cannot be negative.'
                )

                return
            }

            try {

                setSaving(true)

                // ====================================================
                // EDIT MODE
                // ====================================================
                //
                // Your backend UpdateProductRequest currently accepts:
                //
                // - Name
                // - Description
                // - Price
                // - StockQuantity
                //
                // It does NOT currently update CategoryId or SKU.
                // ====================================================

                if (editingProductId) {

                    const updateData = {

                        name:
                            formData.name.trim(),

                        description:
                            formData.description.trim(),

                        price:
                            Number(
                                formData.price
                            ),

                        stockQuantity:
                            Number(
                                formData.stockQuantity
                            )
                    }

                    await updateProduct(
                        editingProductId,
                        updateData
                    )

                    setMessage(
                        'Product updated successfully.'
                    )
                }

                // ====================================================
                // CREATE MODE
                // ====================================================

                else {

                    // --------------------------------------------------
                    // CATEGORY VALIDATION
                    // --------------------------------------------------

                    if (!formData.categoryId) {

                        setError(
                            'Please select a category.'
                        )

                        return
                    }

                    // --------------------------------------------------
                    // SKU VALIDATION
                    // --------------------------------------------------

                    if (!formData.sku.trim()) {

                        setError(
                            'SKU is required.'
                        )

                        return
                    }

                    const createData = {

                        categoryId:
                            formData.categoryId,

                        name:
                            formData.name.trim(),

                        description:
                            formData.description.trim(),

                        sku:
                            formData.sku.trim(),

                        price:
                            Number(
                                formData.price
                            ),

                        stockQuantity:
                            Number(
                                formData.stockQuantity
                            )
                    }

                    await createProduct(
                        createData
                    )

                    setMessage(
                        'Product created successfully.'
                    )
                }

                resetForm()

                await loadProducts()
            }
            catch (err) {

                console.error(
                    'Product save failed:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Unable to save product.'
                )
            }
            finally {

                setSaving(false)
            }
        }

    // ==========================================================
    // DEACTIVATE PRODUCT
    // ==========================================================

    const handleDeactivate =
        async (productId) => {

            const confirmed =
                window.confirm(
                    'Are you sure you want to deactivate this product?'
                )

            if (!confirmed) {
                return
            }

            try {

                setError('')
                setMessage('')

                await deactivateProduct(
                    productId
                )

                setMessage(
                    'Product deactivated successfully.'
                )

                await loadProducts()
            }
            catch (err) {

                console.error(
                    'Failed to deactivate product:',
                    err
                )

                setError(
                    err.response?.data?.message ||
                    'Unable to deactivate product.'
                )
            }
        }

    // ==========================================================
    // INITIAL LOAD
    // ==========================================================
    //
    // Load both products and categories when this page opens.
    // ==========================================================

    useEffect(() => {

        loadProducts()

        loadCategories()

    }, [])

    // ==========================================================
    // UI
    // ==========================================================

    return (
        <div>

            <h1>
                Admin Product Management
            </h1>

            {/* ====================================================
          ERROR MESSAGE
         ==================================================== */}

            {error && (
                <p>
                    {error}
                </p>
            )}

            {/* ====================================================
          SUCCESS MESSAGE
         ==================================================== */}

            {message && (
                <p>
                    {message}
                </p>
            )}

            {/* ====================================================
          PRODUCT FORM
         ==================================================== */}

            <section>

                <h2>
                    {
                        editingProductId
                            ? 'Edit Product'
                            : 'Create Product'
                    }
                </h2>

                <form
                    onSubmit={handleSubmit}
                >

                    {/* ================================================
              CATEGORY DROPDOWN
             ================================================
             
              The admin sees the category name.

              React stores the category GUID.

              Example:

              Admin sees:
              Electronics

              React stores:
              773d...-category-guid
             ================================================ */}

                    {!editingProductId && (
                        <div>

                            <label
                                htmlFor="categoryId"
                            >
                                Category
                            </label>

                            <br />

                            <select
                                id="categoryId"
                                name="categoryId"

                                value={
                                    formData.categoryId
                                }

                                onChange={
                                    handleChange
                                }

                                required
                            >

                                <option value="">
                                    -- Select Category --
                                </option>

                                {categories.map(
                                    (category) => (

                                        <option
                                            key={
                                                category.id
                                            }

                                            value={
                                                category.id
                                            }
                                        >
                                            {
                                                category.name
                                            }
                                        </option>

                                    )
                                )}

                            </select>

                        </div>
                    )}

                    <br />

                    {/* ================================================
              NAME
             ================================================ */}

                    <div>

                        <label
                            htmlFor="name"
                        >
                            Name
                        </label>

                        <br />

                        <input
                            id="name"
                            name="name"
                            type="text"

                            value={
                                formData.name
                            }

                            onChange={
                                handleChange
                            }
                        />

                    </div>

                    <br />

                    {/* ================================================
              DESCRIPTION
             ================================================ */}

                    <div>

                        <label
                            htmlFor="description"
                        >
                            Description
                        </label>

                        <br />

                        <textarea
                            id="description"
                            name="description"

                            value={
                                formData.description
                            }

                            onChange={
                                handleChange
                            }
                        />

                    </div>

                    <br />

                    {/* ================================================
              SKU
             ================================================ */}

                    {!editingProductId && (
                        <div>

                            <label
                                htmlFor="sku"
                            >
                                SKU
                            </label>

                            <br />

                            <input
                                id="sku"
                                name="sku"
                                type="text"

                                value={
                                    formData.sku
                                }

                                onChange={
                                    handleChange
                                }
                            />

                        </div>
                    )}

                    <br />

                    {/* ================================================
              PRICE
             ================================================ */}

                    <div>

                        <label
                            htmlFor="price"
                        >
                            Price
                        </label>

                        <br />

                        <input
                            id="price"
                            name="price"
                            type="number"
                            min="0"
                            step="0.01"

                            value={
                                formData.price
                            }

                            onChange={
                                handleChange
                            }
                        />

                    </div>

                    <br />

                    {/* ================================================
              STOCK QUANTITY
             ================================================ */}

                    <div>

                        <label
                            htmlFor="stockQuantity"
                        >
                            Stock Quantity
                        </label>

                        <br />

                        <input
                            id="stockQuantity"
                            name="stockQuantity"
                            type="number"
                            min="0"

                            value={
                                formData.stockQuantity
                            }

                            onChange={
                                handleChange
                            }
                        />

                    </div>

                    <br />

                    {/* ================================================
              SAVE BUTTON
             ================================================ */}

                    <button
                        type="submit"
                        disabled={saving}
                    >
                        {
                            saving
                                ? 'Saving...'
                                : editingProductId
                                    ? 'Update Product'
                                    : 'Create Product'
                        }
                    </button>

                    {/* ================================================
              CANCEL EDIT
             ================================================ */}

                    {editingProductId && (
                        <>

                            {' '}

                            <button
                                type="button"
                                onClick={resetForm}
                            >
                                Cancel Edit
                            </button>

                        </>
                    )}

                </form>

            </section>

            <hr />

            {/* ====================================================
          PRODUCT LIST
         ==================================================== */}

            <section>

                <h2>
                    Products
                </h2>

                {loading ? (

                    <p>
                        Loading products...
                    </p>

                ) : products.length === 0 ? (

                    <p>
                        No products found.
                    </p>

                ) : (

                    products.map(
                        (product) => (

                            <div
                                key={product.id}
                            >

                                <h3>
                                    {product.name}
                                </h3>

                                <p>

                                    <strong>
                                        SKU:
                                    </strong>

                                    {' '}

                                    {product.sku}

                                </p>

                                <p>

                                    <strong>
                                        Price:
                                    </strong>

                                    {' '}

                                    ₹{Number(
                                        product.price
                                    ).toLocaleString(
                                        'en-IN'
                                    )}

                                </p>

                                <p>

                                    <strong>
                                        Stock:
                                    </strong>

                                    {' '}

                                    {
                                        product.stockQuantity
                                    }

                                </p>

                                <p>

                                    <strong>
                                        Active:
                                    </strong>

                                    {' '}

                                    {
                                        product.isActive
                                            ? 'Yes'
                                            : 'No'
                                    }

                                </p>

                                <button
                                    type="button"

                                    onClick={() =>
                                        handleEdit(
                                            product
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
                                            product.id
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

export default AdminProductsPage