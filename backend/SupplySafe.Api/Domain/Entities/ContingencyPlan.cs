using SupplySafe.Api.Domain.Enums;

namespace SupplySafe.Api.Domain.Entities;

public class ContingencyPlan
{
    public string Id { get; set; } = string.Empty;
    public string IncidentId { get; set; } = string.Empty;
    public List<string> Actions { get; set; } = [];
    public decimal EstimatedCostAvoided { get; set; }
    public ContingencyPlanStatus Status { get; set; }
    public DateTime? ExecutedAt { get; set; }
}