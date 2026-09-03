using Microsoft.AspNetCore.Mvc;
using SupplySafe.Api.Application.DTOs;
using SupplySafe.Api.Application.Services;

namespace SupplySafe.Api.Controllers;

[ApiController]
[Route("api/incidents")]
public class IncidentsController : ControllerBase
{
    private readonly IncidentService _incidentService;

    public IncidentsController(IncidentService incidentService)
    {
        _incidentService = incidentService;
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(_incidentService.GetAll());

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var incident = _incidentService.GetById(id);
        return incident is null
            ? NotFound(new { error = $"Incident '{id}' not found." })
            : Ok(incident);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateIncidentRequest request)
    {
        var incident = _incidentService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = incident.Id }, incident);
    }

    [HttpPost("{id}/execute")]
    public async Task<IActionResult> Execute(string id, CancellationToken cancellationToken)
    {
        var result = await _incidentService.ExecuteAsync(id, cancellationToken);
        return result is null
            ? NotFound(new { error = $"Incident '{id}' not found." })
            : Ok(result);
    }
}
