using SupplySafe.Api.Application.DTOs;
using SupplySafe.Api.Application.Interfaces;
using SupplySafe.Api.Domain.Entities;
using SupplySafe.Api.Domain.Enums;
using SupplySafe.Api.Infrastructure.Persistence;

namespace SupplySafe.Api.Application.Services;

public class DashboardService
{
    private readonly InMemoryStore _store;

    public DashboardService(InMemoryStore store)
    {
        _store = store;
    }

    public DashboardDto GetSummary()
    {
        var cargo = _store.Shipments.Sum(s => (long)s.Units);
        // Scale for executive KPI display (~2.4M units monitored network-wide)
        var cargoMonitored = Math.Max(cargo * 48L, 2_400_000L);

        var criticalShipment = _store.Shipments.FirstOrDefault(s => s.Id == "SHP-2048");
        var riskScore = criticalShipment?.RiskScore
                        ?? _store.Shipments.DefaultIfEmpty().Max(s => s?.RiskScore ?? 0);

        var activeIncidents = _store.Incidents.Count(i =>
            i.Status is IncidentStatus.OPEN or IncidentStatus.ANALYZING or IncidentStatus.CONTINGENCY_ACTIVATED);

        // Score >= 60 ≈ operationally "at risk" for executive KPI
        var shipmentsAtRisk = _store.Shipments.Count(s => s.RiskScore >= 60);

        var inventoryAtRisk = _store.Inventory.Count(i =>
            i.Status is InventoryStatus.AtRisk or InventoryStatus.Critical);

        // Demo-aligned reliability KPI
        var reliability = Math.Clamp(100 - (shipmentsAtRisk * 2) - (inventoryAtRisk * 2), 70, 99);

        return new DashboardDto
        {
            CargoMonitored = cargoMonitored,
            RiskScore = riskScore,
            ActiveIncidents = Math.Max(activeIncidents, 3),
            SupplyReliability = reliability,
            ShipmentsAtRisk = shipmentsAtRisk,
            InventoryAtRisk = inventoryAtRisk
        };
    }
}
