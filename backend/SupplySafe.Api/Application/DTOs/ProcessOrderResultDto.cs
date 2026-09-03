namespace SupplySafe.Api.Application.DTOs;

public class ProcessOrderResultDto
{
    public string OrderId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? LinkedShipmentId { get; set; }
    public string? LinkedIncidentId { get; set; }
    public int? RiskScore { get; set; }
    public bool NotificationSent { get; set; }
    public string NotificationRecipient { get; set; } = string.Empty;
    public List<string> Timeline { get; set; } = [];
    public string Summary { get; set; } = string.Empty;
}
