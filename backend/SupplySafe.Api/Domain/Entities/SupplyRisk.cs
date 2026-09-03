using SupplySafe.Api.Domain.Enums;

namespace SupplySafe.Api.Domain.Entities;

public class SupplyRisk
{
    public string Id { get; set; } = string.Empty;
    public RiskType Type { get; set; }
    public RiskLevel Severity { get; set; }
    public int Score { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> AffectedShipments { get; set; } = [];
    public DateTime DetectedAt { get; set; }
}
