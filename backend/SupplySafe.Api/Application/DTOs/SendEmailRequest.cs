namespace SupplySafe.Api.Application.DTOs;

public class SendEmailRequest
{
    /// <summary>Empty = use Demo:OperationsEmail / SUPPLYSAFE_NOTIFY_EMAIL.</summary>
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = "SUPPLYSAFE - Critical Supply Chain Risk";
    public string Body { get; set; } = string.Empty;
    public string? IncidentId { get; set; }
}
