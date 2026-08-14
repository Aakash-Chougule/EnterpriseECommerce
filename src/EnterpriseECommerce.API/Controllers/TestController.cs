using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult Public()
    {
        return Ok(new
        {
            message = "This endpoint is public."
        });
    }

    [Authorize]
    [HttpGet("protected")]
    public IActionResult Protected()
    {
        return Ok(new
        {
            message = "JWT authentication successful.",
            userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            email = User.FindFirst(ClaimTypes.Email)?.Value,
            role = User.FindFirst(ClaimTypes.Role)?.Value
        });
    }

 

    [Authorize(Roles = "Customer")]
    [HttpGet("customer")]
    public IActionResult CustomerOnly()
    {
        return Ok(new
        {
            message = "Customer access granted.",
            user = User.Identity?.Name,
            role = User.FindFirst(ClaimTypes.Role)?.Value
        });
    }

    // ------------------------------------------------------------
    // Admin-only endpoint
    // ------------------------------------------------------------
    // This endpoint requires:
    // 1. A valid JWT access token.
    // 2. The user must have the "Admin" role.
    //
    // A valid Customer token will receive:
    // HTTP 403 Forbidden
    //
    // A valid Admin token will receive:
    // HTTP 200 OK
    // ------------------------------------------------------------

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public IActionResult AdminOnly()
    {
        return Ok(new
        {
            message = "Admin authorization successful.",
            userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value,
            role = User.FindFirst(ClaimTypes.Role)?.Value
        });
    }
}