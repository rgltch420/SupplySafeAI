using SupplySafe.Api.Domain.Enums;

namespace SupplySafe.Api.Domain.Entities;

public class InventoryItem
{
    public string Id { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double DailyConsumption { get; set; }
    public double CoverageDays { get; set; }
    public int ReorderPoint { get; set; }
    public InventoryStatus Status { get; set; }
}
