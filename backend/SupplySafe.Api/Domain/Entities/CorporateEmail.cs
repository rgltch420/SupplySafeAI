using SupplySafe.Api.Domain.Enums;

namespace SupplySafe.Api.Domain.Entities;

/// <summary>
/// Simulated corporate mailbox message (inbound purchase orders / outbound alerts).
/// </summary>
public class CorporateEmail
{
    public string Id { get; set; } = string.Empty;
    public MailDirection Direction { get; set; }
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
    public bool Processed { get; set; }
    public string? LinkedOrderId { get; set; }
}
