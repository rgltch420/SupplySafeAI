namespace SupplySafe.Api.Application.DTOs;

public class SendEmailResultDto
{
    public bool Success { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string MessageId { get; set; } = string.Empty;
}
