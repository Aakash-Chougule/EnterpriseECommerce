using System.Security.Claims;

using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseECommerce.API.Controllers;

/// <summary>
/// Provides HTTP endpoints for the authenticated user's shopping cart.
///
/// The UserId is taken from the JWT token.
/// Clients cannot choose another user's cart by sending a UserId.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly CartService _cartService;

    public CartController(CartService cartService)
    {
        _cartService = cartService;
    }

    // ------------------------------------------------------------
    // GET: api/Cart
    // ------------------------------------------------------------
    // Returns the currently authenticated user's cart.
    // ------------------------------------------------------------

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized(new
            {
                message = "User ID was not found in the authentication token."
            });
        }

        var cart = await _cartService.GetCartAsync(userId.Value);

        return Ok(cart);
    }

    // ------------------------------------------------------------
    // POST: api/Cart/items
    // ------------------------------------------------------------
    // Adds a product to the authenticated user's cart.
    // ------------------------------------------------------------

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem(
        [FromBody] AddCartItemRequest request)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized(new
            {
                message = "User ID was not found in the authentication token."
            });
        }

        try
        {
            var cart = await _cartService.AddItemAsync(
                userId.Value,
                request);

            return Ok(cart);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    // ------------------------------------------------------------
    // DELETE: api/Cart/items/{productId}
    // ------------------------------------------------------------
    // Completely removes a product from the cart.
    // ------------------------------------------------------------

    [HttpDelete("items/{productId:guid}")]
    public async Task<ActionResult<CartDto>> RemoveItem(
        Guid productId)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized(new
            {
                message = "User ID was not found in the authentication token."
            });
        }

        try
        {
            var cart = await _cartService.RemoveItemAsync(
                userId.Value,
                productId);

            return Ok(cart);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    // ------------------------------------------------------------
    // DELETE: api/Cart
    // ------------------------------------------------------------
    // Removes all products from the cart.
    // ------------------------------------------------------------

    [HttpDelete]
    public async Task<ActionResult<CartDto>> ClearCart()
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized(new
            {
                message = "User ID was not found in the authentication token."
            });
        }

        try
        {
            var cart = await _cartService.ClearCartAsync(
                userId.Value);

            return Ok(cart);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    // ------------------------------------------------------------
    // Extract UserId from JWT
    // ------------------------------------------------------------

    private Guid? GetUserId()
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdValue))
        {
            return null;
        }

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return null;
        }

        return userId;
    }

}