using SupplySafe.Api.Application.Interfaces;
using SupplySafe.Api.Domain.Enums;

namespace SupplySafe.Api.Application.Services;

/// <summary>
/// Transparent heuristic risk engine — demo-safe and explainable.
/// </summary>
public class SupplyRiskEngine : ISupplyRiskEngine
{
    public int CalculateRiskScore(
        int delayDays,
        double inventoryCoverageDays,
        RiskLevel weatherSeverity,
        RiskLevel portCongestion,
        RiskLevel geopoliticalRisk)
    {
        var score = 12;

        // Delay contribution
        if (delayDays >= 5) score += 35;
        else if (delayDays >= 3) score += 22;
        else if (delayDays >= 1) score += 10;

        // Inventory coverage contribution
        if (inventoryCoverageDays < 5) score += 30;
        else if (inventoryCoverageDays < 8) score += 22;
        else if (inventoryCoverageDays < 14) score += 10;
        else score += 2;

        score += SeverityBoost(weatherSeverity, critical: 20, high: 12, medium: 6);
        score += SeverityBoost(portCongestion, critical: 15, high: 10, medium: 5);
        score += SeverityBoost(geopoliticalRisk, critical: 12, high: 8, medium: 4);

        return Math.Clamp(score, 0, 100);
    }

    public RiskLevel ToSeverity(int score) => score switch
    {
        >= 80 => RiskLevel.Critical,
        >= 60 => RiskLevel.High,
        >= 35 => RiskLevel.Medium,
        _ => RiskLevel.Low
    };

    private static int SeverityBoost(RiskLevel level, int critical, int high, int medium) => level switch
    {
        RiskLevel.Critical => critical,
        RiskLevel.High => high,
        RiskLevel.Medium => medium,
        _ => 0
    };
}
