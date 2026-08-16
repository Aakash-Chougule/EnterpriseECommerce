using EnterpriseECommerce.Application.DTOs;
using EnterpriseECommerce.Application.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseECommerce.API.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin")]
public class AdminOrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public AdminOrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetAll()
    {
        var orders = await _orderService.GetAllOrdersAsync();

        return Ok(orders);
    }

    [HttpPut("{id:guid}/confirm")]
    public async Task<ActionResult<OrderDto>> Confirm(Guid id)
    {
        return await ExecuteStatusChange(
            () => _orderService.ConfirmOrderAsync(id));
    }

    [HttpPut("{id:guid}/processing")]
    public async Task<ActionResult<OrderDto>> StartProcessing(Guid id)
    {
        return await ExecuteStatusChange(
            () => _orderService.StartProcessingAsync(id));
    }

    [HttpPut("{id:guid}/ship")]
    public async Task<ActionResult<OrderDto>> Ship(Guid id)
    {
        return await ExecuteStatusChange(
            () => _orderService.ShipOrderAsync(id));
    }

    [HttpPut("{id:guid}/deliver")]
    public async Task<ActionResult<OrderDto>> Deliver(Guid id)
    {
        return await ExecuteStatusChange(
            () => _orderService.DeliverOrderAsync(id));
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<ActionResult<OrderDto>> Cancel(Guid id)
    {
        return await ExecuteStatusChange(
            () => _orderService.CancelOrderAsync(id));
    }

    private async Task<ActionResult<OrderDto>> ExecuteStatusChange(
        Func<Task<OrderDto>> operation)
    {
        try
        {
            var order = await operation();

            return Ok(order);
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
}