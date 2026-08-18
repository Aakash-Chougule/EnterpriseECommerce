using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EnterpriseECommerce.IntegrationTests;

public class AuthIntegrationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(
        CustomWebApplicationFactory factory)
    {
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
    // REGISTER DUPLICATE USER
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

        var firstResponse =
            await _client.PostAsJsonAsync(
                "/api/Auth/register",
                request);

        Assert.Equal(
            HttpStatusCode.OK,
            firstResponse.StatusCode);

        var secondResponse =
            await _client.PostAsJsonAsync(
                "/api/Auth/register",
                request);

        Assert.Equal(
            HttpStatusCode.Conflict,
            secondResponse.StatusCode);
    }

    // ============================================================
    // LOGIN
    // ============================================================

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsJwtToken()
    {
        var uniqueEmail =
            $"login-{Guid.NewGuid():N}@example.com";

        var password =
            "Password@123";

        // --------------------------------------------------------
        // Register first
        // --------------------------------------------------------

        var registerRequest =
            new
            {
                firstName = "Login",
                lastName = "User",
                email = uniqueEmail,
                password,
                phoneNumber = "9876543210"
            };

        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/Auth/register",
                registerRequest);

        Assert.Equal(
            HttpStatusCode.OK,
            registerResponse.StatusCode);

        // --------------------------------------------------------
        // Login
        // --------------------------------------------------------

        var loginRequest =
            new
            {
                email = uniqueEmail,
                password
            };

        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/Auth/login",
                loginRequest);

        Assert.Equal(
            HttpStatusCode.OK,
            loginResponse.StatusCode);

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(
            loginResult);

        Assert.False(
            string.IsNullOrWhiteSpace(
                loginResult!.Token));
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

        var registerRequest =
            new
            {
                firstName = "Wrong",
                lastName = "Password",
                email = uniqueEmail,
                password = "CorrectPassword@123",
                phoneNumber = "9876543210"
            };

        var registerResponse =
            await _client.PostAsJsonAsync(
                "/api/Auth/register",
                registerRequest);

        Assert.Equal(
            HttpStatusCode.OK,
            registerResponse.StatusCode);

        // --------------------------------------------------------
        // Attempt login with wrong password
        // --------------------------------------------------------

        var loginRequest =
            new
            {
                email = uniqueEmail,
                password = "WrongPassword@123"
            };

        var loginResponse =
            await _client.PostAsJsonAsync(
                "/api/Auth/login",
                loginRequest);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            loginResponse.StatusCode);
    }

    // ============================================================
    // PROTECTED ENDPOINT WITH JWT
    // ============================================================

    [Fact]
    public async Task OrdersEndpoint_WithValidJwt_ReturnsOk()
    {
        var uniqueEmail =
            $"jwt-{Guid.NewGuid():N}@example.com";

        var password =
            "Password@123";

        // --------------------------------------------------------
        // Register user
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
        // Login user
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

        var loginResult =
            await loginResponse.Content
                .ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(
            loginResult);

        Assert.False(
            string.IsNullOrWhiteSpace(
                loginResult!.Token));

        // --------------------------------------------------------
        // Add JWT
        // --------------------------------------------------------

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                loginResult.Token);

        // --------------------------------------------------------
        // Call protected endpoint
        // --------------------------------------------------------

        var ordersResponse =
            await _client.GetAsync(
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
        var clientWithoutToken =
            new HttpClient
            {
                BaseAddress =
                    _client.BaseAddress
            };

        var response =
            await clientWithoutToken.GetAsync(
                "/api/Orders");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    // ============================================================
    // TEST RESPONSE MODEL
    // ============================================================

    private class LoginResponse
    {
        public string Token { get; set; } =
            string.Empty;
    }
}