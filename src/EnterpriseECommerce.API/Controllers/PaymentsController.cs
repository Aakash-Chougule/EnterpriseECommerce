using System.Security.Claims;

using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Interfaces;
using EnterpriseECommerce.Application.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService
        _paymentService;

    private readonly IRazorpayPaymentService
        _razorpayPaymentService;

    public PaymentsController(
        PaymentService paymentService,
        IRazorpayPaymentService razorpayPaymentService)
    {
        _paymentService =
            paymentService;

        _razorpayPaymentService =
            razorpayPaymentService;
    }

    // ============================================================
    // CREATE INTERNAL PAYMENT
    // ============================================================

    [HttpPost]
    public async Task<ActionResult<PaymentDto>>
        CreatePayment(
            [FromBody]
            CreatePaymentRequest request)
    {
        var userId =
            GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var payment =
                await _paymentService
                    .CreatePaymentAsync(
                        userId.Value,
                        request);

            return Ok(
                payment);
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
    // GET PAYMENT BY ORDER
    // ============================================================

    [HttpGet("order/{orderId:guid}")]
    public async Task<ActionResult<PaymentDto>>
        GetByOrderId(
            Guid orderId)
    {
        var userId =
            GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var payment =
            await _paymentService
                .GetPaymentByOrderIdAsync(
                    userId.Value,
                    orderId);

        if (payment is null)
        {
            return NotFound(
                new
                {
                    message =
                        "Payment not found."
                });
        }

        return Ok(
            payment);
    }

    // ============================================================
    // CREATE RAZORPAY ORDER
    // ============================================================

    [HttpPost("{paymentId:guid}/razorpay-order")]
    public async Task<ActionResult<RazorpayOrderDto>>
        CreateRazorpayOrder(
            Guid paymentId,
            CancellationToken cancellationToken)
    {
        var userId =
            GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _razorpayPaymentService
                    .CreateOrderAsync(
                        userId.Value,
                        paymentId,
                        cancellationToken);

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
    // VERIFY RAZORPAY PAYMENT
    // ============================================================

    [HttpPost("{paymentId:guid}/razorpay-verify")]
    public async Task<ActionResult<PaymentDto>>
        VerifyRazorpayPayment(
            Guid paymentId,
            [FromBody]
            VerifyRazorpayPaymentRequest request,
            CancellationToken cancellationToken)
    {
        var userId =
            GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var payment =
                await _razorpayPaymentService
                    .VerifyPaymentAsync(
                        userId.Value,
                        paymentId,
                        request,
                        cancellationToken);

            return Ok(
                payment);
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
    // PAYMENT FAILURE
    // ============================================================

    [HttpPost("{paymentId:guid}/fail")]
    public async Task<ActionResult<PaymentDto>>
        MarkFailed(
            Guid paymentId,
            [FromBody]
            PaymentFailureRequest request)
    {
        var userId =
            GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var payment =
                await _paymentService
                    .MarkPaymentFailedAsync(
                        userId.Value,
                        paymentId,
                        request.Reason);

            return Ok(
                payment);
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
    // JWT USER
    // ============================================================

    private Guid? GetUserId()
    {
        var value =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        return Guid.TryParse(
            value,
            out var userId)
            ? userId
            : null;
    }
}