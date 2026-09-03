using Microsoft.AspNetCore.Mvc;
using SupplySafe.Api.Application.Services;

namespace SupplySafe.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public IActionResult Get() => Ok(_dashboardService.GetSummary());
}
