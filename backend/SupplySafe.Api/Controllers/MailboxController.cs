using Microsoft.AspNetCore.Mvc;
using SupplySafe.Api.Application.Services;

namespace SupplySafe.Api.Controllers;

[ApiController]
[Route("api/mailbox")]
public class MailboxController : ControllerBase
{
    private readonly OrderWorkflowService _workflow;

    public MailboxController(OrderWorkflowService workflow)
    {
        _workflow = workflow;
    }

    /// <summary>Simulated corporate inbox/outbox for the demo.</summary>
    [HttpGet]
    public IActionResult GetAll() => Ok(_workflow.GetMailbox());
}
