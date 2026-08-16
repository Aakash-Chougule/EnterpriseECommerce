using System.Security.Claims;

using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseECommerce.API.Controllers;

/// <summary>
/// Provides endpoints for creating and viewing orders
/// belonging to the authenticated user.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    // ------------------------------------------------------------
    // POST: api/Orders
    // ------------------------------------------------------------
    // Creates an order from the authenticated user's cart.
    // ------------------------------------------------------------

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(
        [FromBody] CreateOrderRequest request)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized(new
            {
                message =
                    "User ID was not found in the authentication token."
            });
        }

        try
        {
            var order = await _orderService.CreateOrderAsync(
                userId.Value,
                request);

            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = order.Id },
                order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    // ------------------------------------------------------------
    // GET: api/Orders
    // ------------------------------------------------------------
    // Returns all orders belonging to the authenticated user.
    // ------------------------------------------------------------

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetOrders()
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized(new
            {
                message =
                    "User ID was not found in the authentication token."
            });
        }

        var orders = await _orderService.GetUserOrdersAsync(
            userId.Value);

        return Ok(orders);
    }

    // ------------------------------------------------------------
    // GET: api/Orders/{id}
    // ------------------------------------------------------------
    // Returns one order belonging to the authenticated user.
    // ------------------------------------------------------------

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrderById(Guid id)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized(new
            {
                message =
                    "User ID was not found in the authentication token."
            });
        }

        if (id == Guid.Empty)
        {
            return BadRequest(new
            {
                message = "Order ID is required."
            });
        }

        var order = await _orderService.GetOrderByIdAsync(
            userId.Value,
            id);

        if (order is null)
        {
            return NotFound(new
            {
                message = "Order not found."
            });
        }

        return Ok(order);
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