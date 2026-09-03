namespace SupplySafe.Api.Application.DTOs;

public class DashboardDto
{
    public long CargoMonitored { get; set; }
    public int RiskScore { get; set; }
    public int ActiveIncidents { get; set; }
    public int SupplyReliability { get; set; }
    public int ShipmentsAtRisk { get; set; }
    public int InventoryAtRisk { get; set; }
}
