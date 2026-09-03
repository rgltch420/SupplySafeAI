using Microsoft.AspNetCore.Mvc;
using SupplySafe.Api.Infrastructure.Persistence;

namespace SupplySafe.Api.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly InMemoryStore _store;

    public InventoryController(InMemoryStore store)
    {
        _store = store;
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(_store.Inventory);
}
