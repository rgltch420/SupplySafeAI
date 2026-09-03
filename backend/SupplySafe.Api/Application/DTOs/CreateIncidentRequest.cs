namespace SupplySafe.Api.Application.DTOs;

public class CreateIncidentRequest
{
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = "HIGH";
    public int RiskScore { get; set; }
    public int AffectedUnits { get; set; }
    public decimal EstimatedLoss { get; set; }
    public string? ShipmentId { get; set; }
    public List<string> Recommendations { get; set; } = [];
}
