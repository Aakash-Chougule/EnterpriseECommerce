using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.UnitTests;

public class ProductTests
{
    // ============================================================
    // TEST HELPER
    // ============================================================

    private static Product CreateProduct(
        decimal price = 2500m,
        int stock = 10)
    {
        return new Product(
            Guid.NewGuid(),
            "Mechanical Keyboard",
            "RGB mechanical keyboard",
            "KB-001",
            price,
            stock);
    }

    // ============================================================
    // CREATE PRODUCT
    // ============================================================

    [Fact]
    public void Constructor_WithValidData_CreatesProduct()
    {
        var categoryId =
            Guid.NewGuid();

        var product =
            new Product(
                categoryId,
                "Mechanical Keyboard",
                "RGB keyboard",
                "KB-001",
                2500m,
                10);

        Assert.NotEqual(
            Guid.Empty,
            product.Id);

        Assert.Equal(
            categoryId,
            product.CategoryId);

        Assert.Equal(
            "Mechanical Keyboard",
            product.Name);

        Assert.Equal(
            2500m,
            product.Price);

        Assert.Equal(
            10,
            product.StockQuantity);

        Assert.True(
            product.IsActive);
    }

    [Fact]
    public void Constructor_WithNegativePrice_ThrowsException()
    {
        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    CreateProduct(
                        price: -1));

        Assert.Equal(
            "Price cannot be negative.",
            exception.Message);
    }

    [Fact]
    public void Constructor_WithNegativeStock_ThrowsException()
    {
        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    CreateProduct(
                        stock: -1));

        Assert.Equal(
            "Stock quantity cannot be negative.",
            exception.Message);
    }

    // ============================================================
    // PRICE
    // ============================================================

    [Fact]
    public void UpdatePrice_WithValidPrice_UpdatesPrice()
    {
        var product =
            CreateProduct();

        product.UpdatePrice(
            3000m);

        Assert.Equal(
            3000m,
            product.Price);

        Assert.NotNull(
            product.UpdatedAt);
    }

    [Fact]
    public void UpdatePrice_WithNegativePrice_ThrowsException()
    {
        var product =
            CreateProduct();

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    product.UpdatePrice(-100));

        Assert.Equal(
            "Price cannot be negative.",
            exception.Message);
    }

    // ============================================================
    // STOCK
    // ============================================================

    [Fact]
    public void UpdateStock_WithValidQuantity_UpdatesStock()
    {
        var product =
            CreateProduct();

        product.UpdateStock(
            25);

        Assert.Equal(
            25,
            product.StockQuantity);
    }

    [Fact]
    public void UpdateStock_WithNegativeQuantity_ThrowsException()
    {
        var product =
            CreateProduct();

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    product.UpdateStock(-1));

        Assert.Equal(
            "Stock quantity cannot be negative.",
            exception.Message);
    }

    // ============================================================
    // REDUCE STOCK
    // ============================================================

    [Fact]
    public void ReduceStock_WithValidQuantity_ReducesStock()
    {
        var product =
            CreateProduct(
                stock: 10);

        product.ReduceStock(
            3);

        Assert.Equal(
            7,
            product.StockQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void ReduceStock_WithInvalidQuantity_ThrowsException(
        int quantity)
    {
        var product =
            CreateProduct();

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    product.ReduceStock(
                        quantity));

        Assert.Equal(
            "Quantity must be greater than zero.",
            exception.Message);
    }

    [Fact]
    public void ReduceStock_WithInsufficientStock_ThrowsException()
    {
        var product =
            CreateProduct(
                stock: 5);

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    product.ReduceStock(6));

        Assert.Equal(
            "Insufficient stock.",
            exception.Message);

        // Stock should remain unchanged.
        Assert.Equal(
            5,
            product.StockQuantity);
    }

    // ============================================================
    // INCREASE STOCK
    // ============================================================

    [Fact]
    public void IncreaseStock_WithValidQuantity_IncreasesStock()
    {
        var product =
            CreateProduct(
                stock: 10);

        product.IncreaseStock(
            5);

        Assert.Equal(
            15,
            product.StockQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IncreaseStock_WithInvalidQuantity_ThrowsException(
        int quantity)
    {
        var product =
            CreateProduct();

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    product.IncreaseStock(
                        quantity));

        Assert.Equal(
            "Quantity must be greater than zero.",
            exception.Message);
    }

    // ============================================================
    // PRODUCT DETAILS
    // ============================================================

    [Fact]
    public void UpdateDetails_WithValidData_UpdatesProduct()
    {
        var product =
            CreateProduct();

        product.UpdateDetails(
            "Gaming Keyboard",
            "Updated description");

        Assert.Equal(
            "Gaming Keyboard",
            product.Name);

        Assert.Equal(
            "Updated description",
            product.Description);

        Assert.NotNull(
            product.UpdatedAt);
    }

    [Fact]
    public void UpdateDetails_TrimsNameAndDescription()
    {
        var product =
            CreateProduct();

        product.UpdateDetails(
            "  Gaming Keyboard  ",
            "  Gaming keyboard description  ");

        Assert.Equal(
            "Gaming Keyboard",
            product.Name);

        Assert.Equal(
            "Gaming keyboard description",
            product.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDetails_WithInvalidName_ThrowsException(
        string name)
    {
        var product =
            CreateProduct();

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    product.UpdateDetails(
                        name,
                        "Description"));

        Assert.Equal(
            "Product name is required.",
            exception.Message);
    }

    // ============================================================
    // ACTIVATE / DEACTIVATE
    // ============================================================

    [Fact]
    public void Deactivate_SetsProductInactive()
    {
        var product =
            CreateProduct();

        product.Deactivate();

        Assert.False(
            product.IsActive);

        Assert.NotNull(
            product.UpdatedAt);
    }

    [Fact]
    public void Activate_AfterDeactivation_SetsProductActive()
    {
        var product =
            CreateProduct();

        product.Deactivate();

        product.Activate();

        Assert.True(
            product.IsActive);
    }

    // ============================================================
    // CATEGORY
    // ============================================================

    [Fact]
    public void UpdateCategory_WithValidCategory_UpdatesCategory()
    {
        var product =
            CreateProduct();

        var newCategoryId =
            Guid.NewGuid();

        product.UpdateCategory(
            newCategoryId);

        Assert.Equal(
            newCategoryId,
            product.CategoryId);
    }

    [Fact]
    public void UpdateCategory_WithEmptyId_ThrowsException()
    {
        var product =
            CreateProduct();

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    product.UpdateCategory(
                        Guid.Empty));

        Assert.Equal(
            "CategoryId is required.",
            exception.Message);
    }
}