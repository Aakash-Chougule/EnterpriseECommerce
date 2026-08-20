using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace EnterpriseECommerce.IntegrationTests;

public class AuthIntegrationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthIntegrationTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;

        _client =
            factory.CreateClient();
    }

    // ============================================================
    // REGISTER
    // ============================================================

    [Fact]
    public async Task Register_WithValidData_ReturnsOk()
    {
        var uniqueEmail =
            $"integration-{Guid.NewGuid():N}@example.com";

        var request =
            new
            {
                firstName = "Integration",
                lastName = "User",
                email = uniqueEmail,
                password = "Password@123",
                phoneNumber = "9876543210"
            };

        var response =
            await _client.PostAsJsonAsync(
                "/api/Auth/register",
                request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    // ============================================================
    // DUPLICATE REGISTRATION
    // ============================================================

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var uniqueEmail =
            $"duplicate-{Guid.NewGuid():N}@example.com";

        var request =
            new
            {
                firstName = "Duplicate",
                lastName = "User",
                email = uniqueEmail,
                password = "Password@123",
                phoneNumber = "9876543210"
            };

        // --------------------------------------------------------
        // First registration
        // --------------------------------------------------------

        var firstResponse =
            await _client.PostAsJsonAsync(
                "/api/Auth/register",
                request);

        Assert.Equal(
            HttpStatusCode.OK,
            firstResponse.StatusCode);

        // --------------------------------------------------------
        // Duplicate registration
        // --------------------------------------------------------

        var secondResponse =
            await _client.PostAsJsonAsync(
                "/api/Auth/register",
                request);

        Assert.Equal(
            HttpStatusCode.Conflict,
            secondResponse.StatusCode);
    }

    // ============================================================
    // VALID LOGIN
    // ============================================================

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsJwtToken()
    {
        var uniqueEmail =
            $"login-{Guid.NewGuid():N}@example.com";

        const string password =
            "Password@123";

        // --------------------------------------------------------
        // Register
        // --------------------------------------------------------

        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/Auth/register",
                new
                {
                    firstName = "Login",
                    lastName = "User",
                    email = uniqueEmail,
                    password,
                    phoneNumber = "9876543210"
                });

        Assert.Equal(
            HttpStatusCode.OK,
            registerResponse.StatusCode);

        // --------------------------------------------------------
        // Login
        // --------------------------------------------------------

        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/Auth/login",
                new
                {
                    email = uniqueEmail,
                    password
                });

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        // --------------------------------------------------------
        // Extract JWT
        // --------------------------------------------------------

        var token =
            await ExtractTokenAsync(
                loginResponse);

        Assert.False(
            string.IsNullOrWhiteSpace(
                token));
    }

    // ============================================================
    // INVALID LOGIN
    // ============================================================

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var uniqueEmail =
            $"wrong-password-{Guid.NewGuid():N}@example.com";

        // --------------------------------------------------------
        // Register
        // --------------------------------------------------------

        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/Auth/register",
                new
                {
                    firstName = "Wrong",
                    lastName = "Password",
                    email = uniqueEmail,
                    password = "CorrectPassword@123",
                    phoneNumber = "9876543210"
                });

        Assert.Equal(
            HttpStatusCode.OK,
            registerResponse.StatusCode);

        // --------------------------------------------------------
        // Wrong password
        // --------------------------------------------------------

        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/Auth/login",
                new
                {
                    email = uniqueEmail,
                    password = "WrongPassword@123"
                });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            loginResponse.StatusCode);
    }

    // ============================================================
    // PROTECTED ENDPOINT WITH VALID JWT
    // ============================================================

    [Fact]
    public async Task OrdersEndpoint_WithValidJwt_ReturnsOk()
    {
        var uniqueEmail =
            $"jwt-{Guid.NewGuid():N}@example.com";

        const string password =
            "Password@123";

        // --------------------------------------------------------
        // Register
        // --------------------------------------------------------

        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/Auth/register",
                new
                {
                    firstName = "JWT",
                    lastName = "User",
                    email = uniqueEmail,
                    password,
                    phoneNumber = "9876543210"
                });

        Assert.Equal(
            HttpStatusCode.OK,
            registerResponse.StatusCode);

        // --------------------------------------------------------
        // Login
        // --------------------------------------------------------

        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/Auth/login",
                new
                {
                    email = uniqueEmail,
                    password
                });

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        // --------------------------------------------------------
        // Extract JWT
        // --------------------------------------------------------

        var token =
            await ExtractTokenAsync(
                loginResponse);

        Assert.False(
            string.IsNullOrWhiteSpace(
                token));

        // --------------------------------------------------------
        // Use a dedicated authenticated client
        // --------------------------------------------------------

        var authenticatedClient =
            _factory.CreateClient();

        authenticatedClient
            .DefaultRequestHeaders
            .Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

        // --------------------------------------------------------
        // Protected endpoint
        // --------------------------------------------------------

        var ordersResponse =
            await authenticatedClient
                .GetAsync(
                    "/api/Orders");

        Assert.Equal(
            HttpStatusCode.OK,
            ordersResponse.StatusCode);
    }

    // ============================================================
    // PROTECTED ENDPOINT WITHOUT JWT
    // ============================================================

    [Fact]
    public async Task OrdersEndpoint_WithoutJwt_ReturnsUnauthorized()
    {
        // IMPORTANT:
        //
        // Do NOT create:
        //
        // new HttpClient()
        //
        // because that would connect to localhost:80.
        //
        // factory.CreateClient() talks to the ASP.NET integration
        // test server.

        var clientWithoutToken =
            _factory.CreateClient();

        var response =
            await clientWithoutToken
                .GetAsync(
                    "/api/Orders");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    // ============================================================
    // JWT EXTRACTION
    // ============================================================

    private static async Task<string>
        ExtractTokenAsync(
            HttpResponseMessage response)
    {
        var json =
            await response.Content
                .ReadAsStringAsync();

        Assert.False(
            string.IsNullOrWhiteSpace(
                json));

        using var document =
            JsonDocument.Parse(
                json);

        var root =
            document.RootElement;

        // --------------------------------------------------------
        // Backend may return:
        //
        // {
        //     "token": "..."
        // }
        //
        // or:
        //
        // {
        //     "accessToken": "..."
        // }
        // --------------------------------------------------------

        if (root.TryGetProperty(
            "token",
            out var tokenElement))
        {
            return (
                tokenElement.GetString()
                ?? string.Empty
            );
        }

        if (root.TryGetProperty(
            "accessToken",
            out var accessTokenElement))
        {
            return (
                accessTokenElement.GetString()
                ?? string.Empty
            );
        }

        // --------------------------------------------------------
        // Fail with useful information.
        // --------------------------------------------------------

        throw new InvalidOperationException(
            $"JWT token property was not found. " +
            $"Login response was: {json}");
    }
}