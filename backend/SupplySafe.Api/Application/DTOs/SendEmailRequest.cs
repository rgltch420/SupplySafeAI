namespace SupplySafe.Api.Application.DTOs;

public class SendEmailRequest
{
    public string Recipient { get; set; } = "operations@supplysafe.demo";
    public string Subject { get; set; } = "SUPPLYSAFE - Critical Supply Chain Risk";
    public string Body { get; set; } = string.Empty;
    public string? IncidentId { get; set; }
}
