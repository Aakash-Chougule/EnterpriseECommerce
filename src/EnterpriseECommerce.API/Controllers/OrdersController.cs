using System.Security.Claims;

using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController :
    ControllerBase
{
    private readonly OrderService
        _orderService;

    private readonly CheckoutPricingService
        _checkoutPricingService;

    public OrdersController(
        OrderService orderService,
        CheckoutPricingService checkoutPricingService)
    {
        _orderService =
            orderService;

        _checkoutPricingService =
            checkoutPricingService;
    }

    // ============================================================
    // CHECKOUT PRICE PREVIEW
    // ============================================================
    //
    // POST:
    //
    // /api/Orders/checkout-preview
    //
    // Does NOT:
    //
    // - create an order
    // - reduce stock
    // - clear cart
    // - create payment
    //
    // It only calculates pricing.
    // ============================================================

    [HttpPost("checkout-preview")]
    public async Task<
        ActionResult<CheckoutPreviewDto>>
        GetCheckoutPreview(
            [FromBody]
            CheckoutPreviewRequest request)
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
                await _checkoutPricingService
                    .GetPreviewAsync(
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
    // CREATE ORDER
    // ============================================================

    [HttpPost]
    public async Task<
        ActionResult<OrderDto>>
        CreateOrder(
            [FromBody]
            CreateOrderRequest request)
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
            var order =
                await _orderService
                    .CreateOrderAsync(
                        userId.Value,
                        request);

            return CreatedAtAction(
                nameof(
                    GetOrderById),
                new
                {
                    id =
                        order.Id
                },
                order);
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
    // GET CURRENT USER ORDERS
    // ============================================================

    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyList<OrderDto>>>
        GetOrders()
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

        var orders =
            await _orderService
                .GetUserOrdersAsync(
                    userId.Value);

        return Ok(
            orders);
    }

    // ============================================================
    // GET ORDER
    // ============================================================

    [HttpGet("{id:guid}")]
    public async Task<
        ActionResult<OrderDto>>
        GetOrderById(
            Guid id)
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

        if (id ==
            Guid.Empty)
        {
            return BadRequest(
                new
                {
                    message =
                        "Order ID is required."
                });
        }

        var order =
            await _orderService
                .GetOrderByIdAsync(
                    userId.Value,
                    id);

        if (order is null)
        {
            return NotFound(
                new
                {
                    message =
                        "Order not found."
                });
        }

        return Ok(
            order);
    }

    // ============================================================
    // USER ID
    // ============================================================

    private Guid? GetUserId()
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(
            userIdValue))
        {
            return null;
        }

        return Guid.TryParse(
            userIdValue,
            out var userId)
                ? userId
                : null;
    }
}