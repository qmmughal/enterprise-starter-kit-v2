namespace EnterpriseKit.HttpApi.Controllers;

using EnterpriseKit.Application.Orders.Commands.CancelOrder;
using EnterpriseKit.Application.Orders.Commands.PlaceOrder;
using EnterpriseKit.Application.Orders.Queries.GetOrderById;
using EnterpriseKit.Application.Orders.Queries.GetOrders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>REST API for Order management.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public sealed class OrdersController(ISender mediator) : ControllerBase
{
    // ── Commands ───────────────────────────────────────────────────────────

    /// <summary>Place a new order.</summary>
    /// <response code="201">Order created. Returns the new Order ID.</response>
    /// <response code="400">Validation errors in the request body.</response>
    /// <response code="422">Business rule violation.</response>
    [HttpPost]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> PlaceOrder(
        [FromBody] PlaceOrderCommand command,
        CancellationToken ct)
    {
        var orderId = await mediator.Send(command, ct);

        return CreatedAtAction(
            actionName: nameof(GetById),
            routeValues: new { id = orderId },
            value: new { orderId });
    }

    /// <summary>Cancel an existing order.</summary>
    /// <response code="204">Order successfully cancelled.</response>
    /// <response code="404">Order not found.</response>
    /// <response code="422">Order is in a state that cannot be cancelled.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CancelOrder(
        Guid id,
        [FromBody] CancelOrderRequest request,
        CancellationToken ct)
    {
        await mediator.Send(new CancelOrderCommand(id, request.Reason), ct);
        return NoContent();
    }

    // ── Queries ────────────────────────────────────────────────────────────

    /// <summary>Get a single order by its ID.</summary>
    /// <response code="200">Order found.</response>
    /// <response code="404">Order not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<OrderDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetOrderByIdQuery(id), ct);
        return Ok(result);
    }

    /// <summary>List paginated orders for a customer.</summary>
    /// <response code="200">Paged list of orders.</response>
    [HttpGet]
    [ProducesResponseType<PagedResult<OrderSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] Guid customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetOrdersQuery(customerId, page, pageSize), ct);
        return Ok(result);
    }
}

/// <summary>Request body for cancelling an order.</summary>
public sealed record CancelOrderRequest(string Reason);
