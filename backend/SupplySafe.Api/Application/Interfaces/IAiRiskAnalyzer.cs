using SupplySafe.Api.Application.DTOs;
using SupplySafe.Api.Domain.Entities;

namespace SupplySafe.Api.Application.Interfaces;

public interface IAiRiskAnalyzer
{
    Task<RiskAnalysisResultDto> AnalyzeAsync(
        Shipment shipment,
        InventoryItem? inventory,
        IReadOnlyList<SupplyRisk> relatedRisks,
        CancellationToken cancellationToken = default);
}
