namespace SupplySafe.Api.Application.DTOs;

public class RiskAnalysisResultDto
{
    public string ShipmentId { get; set; } = string.Empty;
    public int RiskScore { get; set; }
    public string Severity { get; set; } = string.Empty;
    public int DelayDays { get; set; }
    public double InventoryCoverageDays { get; set; }
    public int ProjectedShortageUnits { get; set; }
    public decimal EstimatedFinancialImpact { get; set; }
    public bool PredictedStockout { get; set; }
    public int Confidence { get; set; }
    public List<string> Recommendations { get; set; } = [];
}
