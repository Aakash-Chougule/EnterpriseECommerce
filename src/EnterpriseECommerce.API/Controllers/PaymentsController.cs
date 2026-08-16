using System.Security.Claims;

using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;

    public PaymentsController(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    // ------------------------------------------------------------
    // POST: api/Payments
    // ------------------------------------------------------------

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> CreatePayment(
        [FromBody] CreatePaymentRequest request)
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
            var payment =
                await _paymentService.CreatePaymentAsync(
                    userId.Value,
                    request);

            return Ok(payment);
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
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    // ------------------------------------------------------------
    // GET: api/Payments/order/{orderId}
    // ------------------------------------------------------------

    [HttpGet("order/{orderId:guid}")]
    public async Task<ActionResult<PaymentDto>> GetByOrderId(
        Guid orderId)
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

        var payment =
            await _paymentService.GetPaymentByOrderIdAsync(
                userId.Value,
                orderId);

        if (payment is null)
        {
            return NotFound(new
            {
                message = "Payment not found."
            });
        }

        return Ok(payment);
    }

    // ------------------------------------------------------------
    // POST: api/Payments/{paymentId}/success
    // ------------------------------------------------------------
    //
    // Temporary/testing endpoint.
    //
    // Later, when Razorpay/Stripe is integrated, this operation
    // should normally be triggered after verified gateway response
    // or webhook processing.
    // ------------------------------------------------------------

    [HttpPost("{paymentId:guid}/success")]
    public async Task<ActionResult<PaymentDto>> MarkSuccessful(
        Guid paymentId,
        [FromBody] PaymentSuccessRequest request)
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
            var payment =
                await _paymentService.MarkPaymentSuccessfulAsync(
                    userId.Value,
                    paymentId,
                    request.TransactionId);

            return Ok(payment);
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
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    // ------------------------------------------------------------
    // POST: api/Payments/{paymentId}/fail
    // ------------------------------------------------------------

    [HttpPost("{paymentId:guid}/fail")]
    public async Task<ActionResult<PaymentDto>> MarkFailed(
        Guid paymentId,
        [FromBody] PaymentFailureRequest request)
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
            var payment =
                await _paymentService.MarkPaymentFailedAsync(
                    userId.Value,
                    paymentId,
                    request.Reason);

            return Ok(payment);
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
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    // ------------------------------------------------------------
    // Extract authenticated UserId from JWT
    // ------------------------------------------------------------

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

        if (!Guid.TryParse(
            userIdValue,
            out var userId))
        {
            return null;
        }

        return userId;
    }
}