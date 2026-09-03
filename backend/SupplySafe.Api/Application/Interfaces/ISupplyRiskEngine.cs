using SupplySafe.Api.Domain.Entities;
using SupplySafe.Api.Domain.Enums;

namespace SupplySafe.Api.Application.Interfaces;

public interface ISupplyRiskEngine
{
    int CalculateRiskScore(
        int delayDays,
        double inventoryCoverageDays,
        RiskLevel weatherSeverity,
        RiskLevel portCongestion,
        RiskLevel geopoliticalRisk);

    RiskLevel ToSeverity(int score);
}
