using SupplySafe.Api.Application.DTOs;
using SupplySafe.Api.Application.Interfaces;
using SupplySafe.Api.Infrastructure.Persistence;

namespace SupplySafe.Api.Application.Services;

public class RiskAnalysisService
{
    private readonly InMemoryStore _store;
    private readonly IAiRiskAnalyzer _aiRiskAnalyzer;
    private readonly ISupplyRiskEngine _riskEngine;
    private readonly ITrmFxService _trmFxService;

    public RiskAnalysisService(
        InMemoryStore store,
        IAiRiskAnalyzer aiRiskAnalyzer,
        ISupplyRiskEngine riskEngine,
        ITrmFxService trmFxService)
    {
        _store = store;
        _aiRiskAnalyzer = aiRiskAnalyzer;
        _riskEngine = riskEngine;
        _trmFxService = trmFxService;
    }

    public async Task<RiskAnalysisResultDto?> AnalyzeAsync(
        string shipmentId,
        CancellationToken cancellationToken = default)
    {
        var shipment = _store.Shipments.FirstOrDefault(s =>
            s.Id.Equals(shipmentId, StringComparison.OrdinalIgnoreCase));

        if (shipment is null)
            return null;

        var inventory = _store.Inventory.FirstOrDefault(i =>
            i.Product.Equals(shipment.Product, StringComparison.OrdinalIgnoreCase));

        var relatedRisks = _store.Risks
            .Where(r => r.AffectedShipments.Contains(shipment.Id, StringComparer.OrdinalIgnoreCase))
            .ToList();

        var result = await _aiRiskAnalyzer.AnalyzeAsync(shipment, inventory, relatedRisks, cancellationToken);

        // Convert USD impact → COP using TRM basket (official concept = USD/COP)
        var fx = await _trmFxService.GetSnapshotAsync(cancellationToken);
        result.TrmUsdCop = fx.UsdCop;
        result.EstimatedFinancialImpactCop = _trmFxService.ToCop(result.EstimatedFinancialImpact, fx);

        // Keep store in sync for dashboard / execute flow
        shipment.RiskScore = result.RiskScore;
        shipment.RiskLevel = _riskEngine.ToSeverity(result.RiskScore);
        shipment.DelayDays = result.DelayDays;

        return result;
    }
}
