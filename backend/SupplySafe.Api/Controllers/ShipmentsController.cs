using Microsoft.AspNetCore.Mvc;
using SupplySafe.Api.Infrastructure.Persistence;

namespace SupplySafe.Api.Controllers;

[ApiController]
[Route("api/shipments")]
public class ShipmentsController : ControllerBase
{
    private readonly InMemoryStore _store;

    public ShipmentsController(InMemoryStore store)
    {
        _store = store;
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(_store.Shipments);

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var shipment = _store.Shipments.FirstOrDefault(s =>
            s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

        return shipment is null
            ? NotFound(new { error = $"Shipment '{id}' not found." })
            : Ok(shipment);
    }
}
