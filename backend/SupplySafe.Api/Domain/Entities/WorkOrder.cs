using SupplySafe.Api.Domain.Enums;

namespace SupplySafe.Api.Domain.Entities;

/// <summary>
/// Enterprise purchase/replenishment order driven by inbound email.
/// </summary>
public class WorkOrder
{
    public string Id { get; set; } = string.Empty;
    public string ExternalReference { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public int RequestedUnits { get; set; }
    public string Destination { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public string? SourceEmailId { get; set; }
    public string? LinkedShipmentId { get; set; }
    public string? LinkedIncidentId { get; set; }
    public List<string> Timeline { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
