using System.Security.Claims;

using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Security;
using EnterpriseECommerce.Application.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseECommerce.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly AdminUserService
        _adminUserService;

    public AdminUsersController(
        AdminUserService adminUserService)
    {
        _adminUserService =
            adminUserService;
    }

    // ============================================================
    // GET ALL USERS
    // ============================================================
    //
    // GET:
    // /api/admin/users
    //
    // Permission:
    // ManageUsers
    // ============================================================

    [HttpGet]
    [Authorize(
        Policy = PermissionNames.ManageUsers)]
    public async Task<
        ActionResult<IReadOnlyList<UserProfileDto>>>
        GetAllUsers()
    {
        var users =
            await _adminUserService
                .GetAllUsersAsync();

        return Ok(users);
    }

    // ============================================================
    // GET USER BY ID
    // ============================================================
    //
    // GET:
    // /api/admin/users/{userId}
    // ============================================================

    [HttpGet("{userId:guid}")]
    [Authorize(
        Policy = PermissionNames.ManageUsers)]
    public async Task<ActionResult<UserProfileDto>>
        GetUser(
            Guid userId)
    {
        try
        {
            var user =
                await _adminUserService
                    .GetUserAsync(
                        userId);

            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(
                new
                {
                    message =
                        ex.Message
                });
        }
    }

    // ============================================================
    // UPDATE ANY USER
    // ============================================================
    //
    // PUT:
    // /api/admin/users/{userId}
    //
    // Main Admin:
    // Can edit any user.
    //
    // Normal Admin:
    // Requires ManageUsers.
    //
    // Main Admin protection is also enforced inside
    // AdminUserService.
    // ============================================================

    [HttpPut("{userId:guid}")]
    [Authorize(
        Policy = PermissionNames.ManageUsers)]
    public async Task<ActionResult<UserProfileDto>>
        UpdateUser(
            Guid userId,
            [FromBody]
            AdminUpdateUserRequest request)
    {
        var actorUserId =
            GetCurrentUserId();

        if (actorUserId is null)
        {
            return Unauthorized(
                new
                {
                    message =
                        "User ID was not found in the authentication token."
                });
        }

        try
        {
            var user =
                await _adminUserService
                    .UpdateUserAsync(
                        actorUserId.Value,
                        userId,
                        request);

            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                new
                {
                    message =
                        ex.Message
                });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(
                new
                {
                    message =
                        ex.Message
                });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new
                {
                    message =
                        ex.Message
                });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(
                new
                {
                    message =
                        ex.Message
                });
        }
    }

    // ============================================================
    // CREATE NEW ADMIN
    // ============================================================
    //
    // POST:
    // /api/admin/users/admin
    //
    // Permission:
    // ManageAdmins
    //
    // Main Admin automatically passes this permission.
    // ============================================================

    [HttpPost("admin")]
    [Authorize(
        Policy = PermissionNames.ManageAdmins)]
    public async Task<ActionResult<UserProfileDto>>
        CreateAdmin(
            [FromBody]
            CreateAdminRequest request)
    {
        try
        {
            var user =
                await _adminUserService
                    .CreateAdminAsync(
                        request);

            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                new
                {
                    message =
                        ex.Message
                });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(
                new
                {
                    message =
                        ex.Message
                });
        }
    }

    // ============================================================
    // PROMOTE EXISTING USER TO ADMIN
    // ============================================================
    //
    // POST:
    // /api/admin/users/{userId}/promote
    //
    // Example:
    //
    // Customer
    //     ↓
    // Admin
    //
    // Permissions are supplied in request.
    // ============================================================

    [HttpPost("{userId:guid}/promote")]
    [Authorize(
        Policy = PermissionNames.ManageAdmins)]
    public async Task<ActionResult<UserProfileDto>>
        PromoteToAdmin(
            Guid userId,
            [FromBody]
            PromoteUserToAdminRequest request)
    {
        try
        {
            var user =
                await _adminUserService
                    .PromoteToAdminAsync(
                        userId,
                        request);

            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                new
                {
                    message =
                        ex.Message
                });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                new
                {
                    message =
                        ex.Message
                });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(
                new
                {
                    message =
                        ex.Message
                });
        }
    }

    // ============================================================
    // DEMOTE ADMIN TO CUSTOMER
    // ============================================================
    //
    // POST:
    // /api/admin/users/{userId}/demote
    //
    // IMPORTANT:
    //
    // Main Admin cannot be demoted.
    // ============================================================

    [HttpPost("{userId:guid}/demote")]
    [Authorize(
        Policy = PermissionNames.ManageAdmins)]
    public async Task<ActionResult<UserProfileDto>>
        DemoteAdmin(
            Guid userId)
    {
        try
        {
            var user =
                await _adminUserService
                    .DemoteAdminAsync(
                        userId);

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                new
                {
                    message =
                        ex.Message
                });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(
                new
                {
                    message =
                        ex.Message
                });
        }
    }

    // ============================================================
    // UPDATE ADMIN PERMISSIONS
    // ============================================================
    //
    // PUT:
    // /api/admin/users/{userId}/permissions
    //
    // Example body:
    //
    // {
    //     "permissions": [
    //         "ManageProducts",
    //         "ManageCategories",
    //         "ManageOrders"
    //     ]
    // }
    //
    // Main Admin permissions cannot be restricted.
    // ============================================================

    [HttpPut("{userId:guid}/permissions")]
    [Authorize(
        Policy = PermissionNames.ManageAdmins)]
    public async Task<ActionResult<UserProfileDto>>
        SetPermissions(
            Guid userId,
            [FromBody]
            UpdateAdminPermissionsRequest request)
    {
        try
        {
            var user =
                await _adminUserService
                    .SetPermissionsAsync(
                        userId,
                        request);

            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                new
                {
                    message =
                        ex.Message
                });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                new
                {
                    message =
                        ex.Message
                });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(
                new
                {
                    message =
                        ex.Message
                });
        }
    }

    // ============================================================
    // GET AVAILABLE PERMISSIONS
    // ============================================================
    //
    // GET:
    // /api/admin/users/permissions
    //
    // Used by React to display permission checkboxes.
    // ============================================================

    [HttpGet("permissions")]
    [Authorize(
        Policy = PermissionNames.ManageAdmins)]
    public async Task<
        ActionResult<IReadOnlyList<string>>>
        GetPermissions()
    {
        var permissions =
            await _adminUserService
                .GetAvailablePermissionsAsync();

        return Ok(
            permissions);
    }

    // ============================================================
    // CURRENT AUTHENTICATED USER ID
    // ============================================================

    private Guid? GetCurrentUserId()
    {
        var value =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(
            value))
        {
            return null;
        }

        if (!Guid.TryParse(
            value,
            out var userId))
        {
            return null;
        }

        return userId;
    }
}