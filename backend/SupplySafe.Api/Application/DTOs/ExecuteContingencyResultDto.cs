namespace SupplySafe.Api.Application.DTOs;

public class ExecuteContingencyResultDto
{
    public string IncidentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<string> ActionsExecuted { get; set; } = [];
    public decimal EstimatedCostAvoided { get; set; }
    public bool NotificationSent { get; set; }
}
