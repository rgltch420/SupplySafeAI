using Microsoft.AspNetCore.Mvc;
using SupplySafe.Api.Infrastructure.Persistence;

namespace SupplySafe.Api.Controllers;

[ApiController]
[Route("api/demo")]
public class DemoController : ControllerBase
{
    private readonly InMemoryStore _store;

    public DemoController(InMemoryStore store)
    {
        _store = store;
    }

    /// <summary>Reload burned seed so the pitch can be repeated without restarting the process.</summary>
    [HttpPost("reset")]
    public IActionResult Reset()
    {
        _store.Reset();
        return Ok(new
        {
            success = true,
            message = "Demo store reset. SHP-2048 / INC-2048 / ORD-3000 ready again.",
            shipments = _store.Shipments.Count,
            orders = _store.Orders.Count,
            incidents = _store.Incidents.Count
        });
    }
}
