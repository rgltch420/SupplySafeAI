using SupplySafe.Api.Domain.Enums;

namespace SupplySafe.Api.Domain.Entities;

public class Shipment
{
    public string Id { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public ShipmentStatus Status { get; set; }
    public DateTime Eta { get; set; }
    public DateTime OriginalEta { get; set; }
    public int DelayDays { get; set; }
    public int RiskScore { get; set; }
    public decimal Value { get; set; }
    public int Units { get; set; }
    public RiskLevel RiskLevel { get; set; }
}
