using Microsoft.AspNetCore.Mvc;
using SupplySafe.Api.Application.Interfaces;

namespace SupplySafe.Api.Controllers;

[ApiController]
[Route("api/fx")]
public class FxController : ControllerBase
{
    private readonly ITrmFxService _trmFxService;

    public FxController(ITrmFxService trmFxService)
    {
        _trmFxService = trmFxService;
    }

    /// <summary>
    /// TRM-style FX basket for the demo: USD/COP (official TRM concept) + EUR/CNY references.
    /// </summary>
    [HttpGet("trm")]
    public async Task<IActionResult> GetTrm(CancellationToken cancellationToken)
    {
        var snapshot = await _trmFxService.GetSnapshotAsync(cancellationToken);
        return Ok(snapshot);
    }
}
