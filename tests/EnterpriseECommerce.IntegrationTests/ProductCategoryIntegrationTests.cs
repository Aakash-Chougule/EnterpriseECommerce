using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace EnterpriseECommerce.IntegrationTests;

public class ProductCategoryIntegrationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ProductCategoryIntegrationTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // ============================================================
    // PUBLIC ENDPOINTS
    // ============================================================

    [Fact]
    public async Task GetCategories_WithoutAuthentication_ReturnsOk()
    {
        var client =
            _factory.CreateClient();

        var response =
            await client.GetAsync(
                "/api/Categories");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_WithoutAuthentication_ReturnsOk()
    {
        var client =
            _factory.CreateClient();

        var response =
            await client.GetAsync(
                "/api/Products");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    // ============================================================
    // ADMIN SECURITY
    // ============================================================

    [Fact]
    public async Task CreateCategory_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client =
            _factory.CreateClient();

        var response =
            await client.PostAsJsonAsync(
                "/api/Categories",
                new
                {
                    name = "Unauthorized Category",
                    description = "Should not be created"
                });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client =
            _factory.CreateClient();

        var response =
            await client.PostAsJsonAsync(
                "/api/Products",
                new
                {
                    categoryId = Guid.NewGuid(),
                    name = "Unauthorized Product",
                    description = "Should not be created",
                    sku = $"UNAUTH-{Guid.NewGuid():N}",
                    price = 1000m,
                    stockQuantity = 10
                });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    // ============================================================
    // COMPLETE ADMIN PRODUCT FLOW
    // ============================================================

    [Fact]
    public async Task Admin_CanCreateCategoryAndManageProduct()
    {
        // --------------------------------------------------------
        // ARRANGE
        // --------------------------------------------------------

        var client =
            await CreateAdminClientAsync();

        var uniqueValue =
            Guid.NewGuid()
                .ToString("N");

        // ========================================================
        // 1. CREATE CATEGORY
        // ========================================================

        var categoryResponse =
            await client.PostAsJsonAsync(
                "/api/Categories",
                new
                {
                    name =
                        $"Integration Category {uniqueValue}",

                    description =
                        "Category created by integration test."
                });

        Assert.Equal(
            HttpStatusCode.Created,
            categoryResponse.StatusCode);

        var categoryJson =
            await categoryResponse.Content
                .ReadAsStringAsync();

        using var categoryDocument =
            JsonDocument.Parse(
                categoryJson);

        var categoryId =
            GetRequiredGuid(
                categoryDocument.RootElement,
                "id");

        Assert.NotEqual(
            Guid.Empty,
            categoryId);

        // ========================================================
        // 2. GET CATEGORY BY ID
        // ========================================================

        var getCategoryResponse =
            await client.GetAsync(
                $"/api/Categories/{categoryId}");

        Assert.Equal(
            HttpStatusCode.OK,
            getCategoryResponse.StatusCode);

        // ========================================================
        // 3. CREATE PRODUCT
        // ========================================================

        var sku =
            $"INT-{uniqueValue}";

        var productResponse =
            await client.PostAsJsonAsync(
                "/api/Products",
                new
                {
                    categoryId,
                    name =
                        "Integration Mechanical Keyboard",

                    description =
                        "Product created from integration test.",

                    sku,
                    price = 2500m,
                    stockQuantity = 10
                });

        Assert.Equal(
            HttpStatusCode.Created,
            productResponse.StatusCode);

        var productJson =
            await productResponse.Content
                .ReadAsStringAsync();

        using var productDocument =
            JsonDocument.Parse(
                productJson);

        var productId =
            GetRequiredGuid(
                productDocument.RootElement,
                "id");

        Assert.NotEqual(
            Guid.Empty,
            productId);

        // ========================================================
        // 4. GET PRODUCT BY ID
        // ========================================================

        var getProductResponse =
            await client.GetAsync(
                $"/api/Products/{productId}");

        Assert.Equal(
            HttpStatusCode.OK,
            getProductResponse.StatusCode);

        var getProductJson =
            await getProductResponse.Content
                .ReadAsStringAsync();

        using var getProductDocument =
            JsonDocument.Parse(
                getProductJson);

        Assert.Equal(
            sku,
            GetRequiredString(
                getProductDocument.RootElement,
                "sku"));

        Assert.Equal(
            10,
            GetRequiredInt32(
                getProductDocument.RootElement,
                "stockQuantity"));

        // ========================================================
        // 5. INCREASE STOCK
        //
        // 10 + 5 = 15
        // ========================================================

        var increaseResponse =
            await client.PostAsJsonAsync(
                $"/api/Products/{productId}/stock/increase",
                new
                {
                    quantity = 5
                });

        Assert.Equal(
            HttpStatusCode.OK,
            increaseResponse.StatusCode);

        var increaseJson =
            await increaseResponse.Content
                .ReadAsStringAsync();

        using var increaseDocument =
            JsonDocument.Parse(
                increaseJson);

        Assert.Equal(
            15,
            GetRequiredInt32(
                increaseDocument.RootElement,
                "stockQuantity"));

        // ========================================================
        // 6. DECREASE STOCK
        //
        // 15 - 3 = 12
        // ========================================================

        var decreaseResponse =
            await client.PostAsJsonAsync(
                $"/api/Products/{productId}/stock/decrease",
                new
                {
                    quantity = 3
                });

        Assert.Equal(
            HttpStatusCode.OK,
            decreaseResponse.StatusCode);

        var decreaseJson =
            await decreaseResponse.Content
                .ReadAsStringAsync();

        using var decreaseDocument =
            JsonDocument.Parse(
                decreaseJson);

        Assert.Equal(
            12,
            GetRequiredInt32(
                decreaseDocument.RootElement,
                "stockQuantity"));

        // ========================================================
        // 7. UPDATE PRODUCT
        // ========================================================

        var updateResponse =
            await client.PutAsJsonAsync(
                $"/api/Products/{productId}",
                new
                {
                    name =
                        "Updated Integration Keyboard",

                    description =
                        "Updated by integration test.",

                    price = 2999m,

                    stockQuantity = 20
                });

        Assert.Equal(
            HttpStatusCode.OK,
            updateResponse.StatusCode);

        var updateJson =
            await updateResponse.Content
                .ReadAsStringAsync();

        using var updateDocument =
            JsonDocument.Parse(
                updateJson);

        Assert.Equal(
            "Updated Integration Keyboard",
            GetRequiredString(
                updateDocument.RootElement,
                "name"));

        Assert.Equal(
            20,
            GetRequiredInt32(
                updateDocument.RootElement,
                "stockQuantity"));

        // ========================================================
        // 8. ADMIN CAN SEE PRODUCT
        // ========================================================

        var adminProductsResponse =
            await client.GetAsync(
                "/api/Products/admin/all");

        Assert.Equal(
            HttpStatusCode.OK,
            adminProductsResponse.StatusCode);

        // ========================================================
        // 9. DEACTIVATE PRODUCT
        // ========================================================

        var deleteResponse =
            await client.DeleteAsync(
                $"/api/Products/{productId}");

        Assert.Equal(
            HttpStatusCode.OK,
            deleteResponse.StatusCode);

        // ========================================================
        // 10. PUBLIC GET SHOULD NO LONGER RETURN PRODUCT
        // ========================================================

        var publicClient =
            _factory.CreateClient();

        var afterDeleteResponse =
            await publicClient.GetAsync(
                $"/api/Products/{productId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            afterDeleteResponse.StatusCode);

        // ========================================================
        // 11. ADMIN ALL SHOULD STILL WORK
        //
        // Because this is soft delete.
        // ========================================================

        var adminAfterDeleteResponse =
            await client.GetAsync(
                "/api/Products/admin/all");

        Assert.Equal(
            HttpStatusCode.OK,
            adminAfterDeleteResponse.StatusCode);
    }

    // ============================================================
    // LOW STOCK
    // ============================================================

    [Fact]
    public async Task Admin_LowStockEndpoint_ReturnsOk()
    {
        var client =
            await CreateAdminClientAsync();

        var response =
            await client.GetAsync(
                "/api/Products/admin/low-stock?threshold=10");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    // ============================================================
    // CUSTOMER CANNOT ACCESS ADMIN PRODUCT ENDPOINT
    // ============================================================

    [Fact]
    public async Task Customer_CannotAccessAdminProducts()
    {
        var client =
            await CreateCustomerClientAsync();

        var response =
            await client.GetAsync(
                "/api/Products/admin/all");

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    // ============================================================
    // ADMIN CLIENT
    // ============================================================

    private async Task<HttpClient>
        CreateAdminClientAsync()
    {
        var client =
            _factory.CreateClient();

        var loginResponse =
            await client.PostAsJsonAsync(
                "/api/Auth/login",
                new
                {
                    email =
                        "admin@enterpriseecommerce.com",

                    password =
                        "Admin@12345"
                });

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var token =
            await ExtractTokenAsync(
                loginResponse);

        Assert.False(
            string.IsNullOrWhiteSpace(
                token));

        client
            .DefaultRequestHeaders
            .Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

        return client;
    }

    // ============================================================
    // CUSTOMER CLIENT
    // ============================================================

    private async Task<HttpClient>
        CreateCustomerClientAsync()
    {
        var client =
            _factory.CreateClient();

        var email =
            $"customer-{Guid.NewGuid():N}@example.com";

        const string password =
            "Password@123";

        // --------------------------------------------------------
        // Register customer
        // --------------------------------------------------------

        var registerResponse =
            await client.PostAsJsonAsync(
                "/api/Auth/register",
                new
                {
                    firstName =
                        "Integration",

                    lastName =
                        "Customer",

                    email,

                    password,

                    phoneNumber =
                        "9876543210"
                });

        Assert.Equal(
            HttpStatusCode.OK,
            registerResponse.StatusCode);

        // --------------------------------------------------------
        // Login
        // --------------------------------------------------------

        var loginResponse =
            await client.PostAsJsonAsync(
                "/api/Auth/login",
                new
                {
                    email,
                    password
                });

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var token =
            await ExtractTokenAsync(
                loginResponse);

        client
            .DefaultRequestHeaders
            .Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

        return client;
    }

    // ============================================================
    // TOKEN EXTRACTION
    // ============================================================

    private static async Task<string>
        ExtractTokenAsync(
            HttpResponseMessage response)
    {
        var json =
            await response.Content
                .ReadAsStringAsync();

        using var document =
            JsonDocument.Parse(
                json);

        var root =
            document.RootElement;

        if (root.TryGetProperty(
            "token",
            out var tokenElement))
        {
            return tokenElement.GetString()
                   ?? string.Empty;
        }

        if (root.TryGetProperty(
            "accessToken",
            out var accessTokenElement))
        {
            return accessTokenElement
                       .GetString()
                   ?? string.Empty;
        }

        throw new InvalidOperationException(
            $"JWT token property was not found. " +
            $"Response: {json}");
    }

    // ============================================================
    // JSON HELPERS
    // ============================================================

    private static Guid GetRequiredGuid(
        JsonElement element,
        string propertyName)
    {
        if (!element.TryGetProperty(
                propertyName,
                out var property))
        {
            throw new InvalidOperationException(
                $"Property '{propertyName}' was not found. " +
                $"JSON: {element}");
        }

        return property.GetGuid();
    }

    private static string GetRequiredString(
        JsonElement element,
        string propertyName)
    {
        if (!element.TryGetProperty(
                propertyName,
                out var property))
        {
            throw new InvalidOperationException(
                $"Property '{propertyName}' was not found. " +
                $"JSON: {element}");
        }

        return property.GetString()
               ?? string.Empty;
    }

    private static int GetRequiredInt32(
        JsonElement element,
        string propertyName)
    {
        if (!element.TryGetProperty(
                propertyName,
                out var property))
        {
            throw new InvalidOperationException(
                $"Property '{propertyName}' was not found. " +
                $"JSON: {element}");
        }

        return property.GetInt32();
    }
}