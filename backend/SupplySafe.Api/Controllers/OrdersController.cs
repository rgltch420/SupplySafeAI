using Microsoft.AspNetCore.Mvc;
using SupplySafe.Api.Application.DTOs;
using SupplySafe.Api.Application.Services;

namespace SupplySafe.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly OrderWorkflowService _workflow;

    public OrdersController(OrderWorkflowService workflow)
    {
        _workflow = workflow;
    }

    /// <summary>List burned + newly ingested work orders.</summary>
    [HttpGet]
    public IActionResult GetAll() => Ok(_workflow.GetOrders());

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var order = _workflow.GetOrder(id);
        return order is null
            ? NotFound(new { error = $"Order '{id}' not found." })
            : Ok(order);
    }

    /// <summary>
    /// Simulates an inbound corporate email that creates a purchase order.
    /// </summary>
    [HttpPost("from-email")]
    public IActionResult IngestFromEmail([FromBody] IngestEmailOrderRequest? request)
    {
        request ??= new IngestEmailOrderRequest();
        var order = _workflow.IngestFromEmail(request);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    /// <summary>
    /// Processes the order like Ops: validate → link shipment → risk → notify email.
    /// </summary>
    [HttpPost("{id}/process")]
    public async Task<IActionResult> Process(string id, CancellationToken cancellationToken)
    {
        var result = await _workflow.ProcessAsync(id, cancellationToken);
        return result is null
            ? NotFound(new { error = $"Order '{id}' not found." })
            : Ok(result);
    }
}
