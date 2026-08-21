using System.Security.Claims;

using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController :
    ControllerBase
{
    private readonly UserProfileService
        _profileService;

    public ProfileController(
        UserProfileService profileService)
    {
        _profileService =
            profileService;
    }

    // ============================================================
    // GET CURRENT USER PROFILE
    // ============================================================
    //
    // GET:
    // /api/Profile
    // ============================================================

    [HttpGet]
    public async Task<ActionResult<UserProfileDto>>
        GetProfile()
    {
        var userId =
            GetUserId();

        if (userId is null)
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
            var result =
                await _profileService
                    .GetProfileAsync(
                        userId.Value);

            return Ok(
                result);
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
    // UPDATE CURRENT USER PROFILE
    // ============================================================
    //
    // PUT:
    // /api/Profile
    //
    // User can change:
    //
    // FirstName
    // LastName
    // Email
    // PhoneNumber
    //
    // User CANNOT change:
    //
    // Role
    // Permissions
    // IsMainAdmin
    // IsActive
    // ============================================================

    [HttpPut]
    public async Task<ActionResult<UserProfileDto>>
        UpdateProfile(
            [FromBody]
            UpdateMyProfileRequest request)
    {
        var userId =
            GetUserId();

        if (userId is null)
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
            var result =
                await _profileService
                    .UpdateProfileAsync(
                        userId.Value,
                        request);

            return Ok(
                result);
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
    // CHANGE PASSWORD
    // ============================================================
    //
    // PUT:
    // /api/Profile/password
    // ============================================================

    [HttpPut("password")]
    public async Task<IActionResult>
        ChangePassword(
            [FromBody]
            ChangePasswordRequest request)
    {
        var userId =
            GetUserId();

        if (userId is null)
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
            await _profileService
                .ChangePasswordAsync(
                    userId.Value,
                    request);

            return Ok(
                new
                {
                    message =
                        "Password changed successfully."
                });
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(
                new
                {
                    message =
                        ex.Message
                });
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
    // EXTRACT USER ID FROM JWT
    // ============================================================

    private Guid? GetUserId()
    {
        var value =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(
            value))
        {
            return null;
        }

        return Guid.TryParse(
            value,
            out var userId)
            ? userId
            : null;
    }
}