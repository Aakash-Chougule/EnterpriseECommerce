using EnterpriseECommerce.Domain.Entities;

namespace EnterpriseECommerce.UnitTests;

public class CategoryTests
{
    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    [Fact]
    public void Constructor_WithValidName_CreatesActiveCategory()
    {
        var category =
            new Category(
                "Electronics",
                "Electronic products");

        Assert.NotEqual(
            Guid.Empty,
            category.Id);

        Assert.Equal(
            "Electronics",
            category.Name);

        Assert.Equal(
            "Electronic products",
            category.Description);

        Assert.True(
            category.IsActive);

        Assert.True(
            category.CreatedAt <=
            DateTime.UtcNow);

        Assert.Null(
            category.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithoutDescription_AllowsNullDescription()
    {
        var category =
            new Category(
                "Electronics");

        Assert.Equal(
            "Electronics",
            category.Name);

        Assert.Null(
            category.Description);

        Assert.True(
            category.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ThrowsArgumentException(
        string name)
    {
        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    new Category(
                        name));

        Assert.Equal(
            "Category name is required.",
            exception.Message);
    }

    // ============================================================
    // UPDATE
    // ============================================================

    [Fact]
    public void Update_WithValidData_UpdatesCategory()
    {
        var category =
            new Category(
                "Electronics",
                "Old description");

        category.Update(
            "Computer Accessories",
            "Keyboards, mouse and accessories");

        Assert.Equal(
            "Computer Accessories",
            category.Name);

        Assert.Equal(
            "Keyboards, mouse and accessories",
            category.Description);

        Assert.NotNull(
            category.UpdatedAt);
    }

    [Fact]
    public void Update_WithNullDescription_AllowsNullDescription()
    {
        var category =
            new Category(
                "Electronics",
                "Description");

        category.Update(
            "Electronics",
            null);

        Assert.Equal(
            "Electronics",
            category.Name);

        Assert.Null(
            category.Description);

        Assert.NotNull(
            category.UpdatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithInvalidName_ThrowsArgumentException(
        string name)
    {
        var category =
            new Category(
                "Electronics");

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    category.Update(
                        name,
                        "Description"));

        Assert.Equal(
            "Category name is required.",
            exception.Message);
    }

    // ============================================================
    // DEACTIVATE
    // ============================================================

    [Fact]
    public void Deactivate_SetsCategoryInactive()
    {
        var category =
            new Category(
                "Electronics");

        category.Deactivate();

        Assert.False(
            category.IsActive);

        Assert.NotNull(
            category.UpdatedAt);
    }

    // ============================================================
    // ACTIVATE
    // ============================================================

    [Fact]
    public void Activate_AfterDeactivation_SetsCategoryActive()
    {
        var category =
            new Category(
                "Electronics");

        category.Deactivate();

        Assert.False(
            category.IsActive);

        category.Activate();

        Assert.True(
            category.IsActive);

        Assert.NotNull(
            category.UpdatedAt);
    }

    // ============================================================
    // REACTIVATION FLOW
    // ============================================================

    [Fact]
    public void Category_CanBeDeactivatedUpdatedAndReactivated()
    {
        var category =
            new Category(
                "Electronics",
                "Old description");

        category.Deactivate();

        Assert.False(
            category.IsActive);

        category.Update(
            "Electronics",
            "Updated description");

        category.Activate();

        Assert.True(
            category.IsActive);

        Assert.Equal(
            "Electronics",
            category.Name);

        Assert.Equal(
            "Updated description",
            category.Description);
    }
}