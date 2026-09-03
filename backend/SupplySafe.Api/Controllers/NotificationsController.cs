using Microsoft.AspNetCore.Mvc;
using SupplySafe.Api.Application.DTOs;
using SupplySafe.Api.Application.Interfaces;

namespace SupplySafe.Api.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly IEmailNotificationService _emailService;

    public NotificationsController(IEmailNotificationService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("email")]
    public async Task<IActionResult> SendEmail(
        [FromBody] SendEmailRequest? request,
        CancellationToken cancellationToken)
    {
        request ??= new SendEmailRequest();
        var result = await _emailService.SendAsync(request, cancellationToken);
        return Ok(result);
    }
}
