using Microsoft.AspNetCore.Mvc;
using SupplySafe.Api.Application.DTOs;
using SupplySafe.Api.Application.Services;
using SupplySafe.Api.Infrastructure.Persistence;

namespace SupplySafe.Api.Controllers;

[ApiController]
[Route("api")]
public class RisksController : ControllerBase
{
    private readonly InMemoryStore _store;
    private readonly RiskAnalysisService _riskAnalysisService;

    public RisksController(InMemoryStore store, RiskAnalysisService riskAnalysisService)
    {
        _store = store;
        _riskAnalysisService = riskAnalysisService;
    }

    [HttpGet("risks")]
    public IActionResult GetAll() => Ok(_store.Risks);

    [HttpPost("risk/analyze")]
    public async Task<IActionResult> Analyze(
        [FromBody] AnalyzeRiskRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ShipmentId))
            return BadRequest(new { error = "shipmentId is required." });

        var result = await _riskAnalysisService.AnalyzeAsync(request.ShipmentId, cancellationToken);
        if (result is null)
            return NotFound(new { error = $"Shipment '{request.ShipmentId}' not found." });

        return Ok(result);
    }
}
