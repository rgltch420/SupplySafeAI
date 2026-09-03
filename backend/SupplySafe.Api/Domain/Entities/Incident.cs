using SupplySafe.Api.Domain.Enums;

namespace SupplySafe.Api.Domain.Entities;

public class Incident
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public RiskLevel Severity { get; set; }
    public IncidentStatus Status { get; set; }
    public int RiskScore { get; set; }
    public int AffectedUnits { get; set; }
    public decimal EstimatedLoss { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Recommendations { get; set; } = [];
    public List<string> ActionsExecuted { get; set; } = [];
    public string? ShipmentId { get; set; }
}
