using SupplySafe.Api.Application.DTOs;
using SupplySafe.Api.Application.Interfaces;
using SupplySafe.Api.Domain.Entities;
using SupplySafe.Api.Domain.Enums;
using SupplySafe.Api.Infrastructure.Persistence;

namespace SupplySafe.Api.Application.Services;

public class IncidentService
{
    private readonly InMemoryStore _store;
    private readonly IEmailNotificationService _emailService;

    private static readonly string[] DefaultActions =
    [
        "Alternative route activated",
        "Secondary supplier activated",
        "Inventory prioritized",
        "Operations notified",
        "Procurement notified"
    ];

    public IncidentService(
        InMemoryStore store,
        IEmailNotificationService emailService)
    {
        _store = store;
        _emailService = emailService;
    }

    public IReadOnlyList<Incident> GetAll() => _store.Incidents.OrderByDescending(i => i.CreatedAt).ToList();

    public Incident? GetById(string id) =>
        _store.Incidents.FirstOrDefault(i => i.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public Incident Create(CreateIncidentRequest request)
    {
        var severity = ParseSeverity(request.Severity);

        var incident = new Incident
        {
            Id = _store.NextIncidentId(),
            Title = string.IsNullOrWhiteSpace(request.Title)
                ? "Supply chain disruption"
                : request.Title.Trim(),
            Severity = severity,
            Status = IncidentStatus.OPEN,
            RiskScore = request.RiskScore,
            AffectedUnits = request.AffectedUnits,
            EstimatedLoss = request.EstimatedLoss,
            CreatedAt = DateTime.UtcNow,
            ShipmentId = request.ShipmentId,
            Recommendations = request.Recommendations.Count > 0
                ? request.Recommendations
                :
                [
                    "Divert shipment to alternate route",
                    "Activate secondary supplier",
                    "Prioritize existing inventory",
                    "Notify Operations",
                    "Notify Procurement"
                ],
            ActionsExecuted = []
        };

        _store.Incidents.Add(incident);

        _store.ContingencyPlans.Add(new ContingencyPlan
        {
            Id = _store.NextPlanId(),
            IncidentId = incident.Id,
            Actions = [.. incident.Recommendations],
            EstimatedCostAvoided = incident.EstimatedLoss,
            Status = ContingencyPlanStatus.Pending
        });

        return incident;
    }

    public async Task<ExecuteContingencyResultDto?> ExecuteAsync(
        string incidentId,
        CancellationToken cancellationToken = default)
    {
        var incident = GetById(incidentId);
        if (incident is null)
            return null;

        incident.Status = IncidentStatus.CONTINGENCY_ACTIVATED;
        incident.ActionsExecuted = [.. DefaultActions];

        var plan = _store.ContingencyPlans.FirstOrDefault(p =>
            p.IncidentId.Equals(incident.Id, StringComparison.OrdinalIgnoreCase));

        var costAvoided = plan?.EstimatedCostAvoided ?? incident.EstimatedLoss;
        if (costAvoided <= 0)
            costAvoided = 84600m;

        if (plan is not null)
        {
            plan.Status = ContingencyPlanStatus.Completed;
            plan.ExecutedAt = DateTime.UtcNow;
            plan.EstimatedCostAvoided = costAvoided;
            plan.Actions = [.. DefaultActions];
        }
        else
        {
            _store.ContingencyPlans.Add(new ContingencyPlan
            {
                Id = _store.NextPlanId(),
                IncidentId = incident.Id,
                Actions = [.. DefaultActions],
                EstimatedCostAvoided = costAvoided,
                Status = ContingencyPlanStatus.Completed,
                ExecutedAt = DateTime.UtcNow
            });
        }

        Shipment? shipmentForEmail = null;
        double coverageForEmail = 7.1;
        var delayForEmail = 6;

        if (!string.IsNullOrWhiteSpace(incident.ShipmentId))
        {
            shipmentForEmail = _store.Shipments.FirstOrDefault(s =>
                s.Id.Equals(incident.ShipmentId, StringComparison.OrdinalIgnoreCase));

            var inventoryItem = shipmentForEmail is null
                ? null
                : _store.Inventory.FirstOrDefault(i =>
                    i.Product.Equals(shipmentForEmail.Product, StringComparison.OrdinalIgnoreCase));

            if (inventoryItem is not null)
                coverageForEmail = inventoryItem.CoverageDays;

            if (shipmentForEmail is not null)
            {
                delayForEmail = shipmentForEmail.DelayDays;
                shipmentForEmail.Status = ShipmentStatus.Diverted;
                shipmentForEmail.Route = $"{shipmentForEmail.Origin} → Alternate Hub → {shipmentForEmail.Destination}";
                shipmentForEmail.DelayDays = Math.Max(0, shipmentForEmail.DelayDays - 3);
                shipmentForEmail.Eta = DateTime.UtcNow.AddDays(
                    Math.Max(1, (shipmentForEmail.OriginalEta - DateTime.UtcNow).TotalDays + shipmentForEmail.DelayDays));
                shipmentForEmail.RiskScore = Math.Max(25, shipmentForEmail.RiskScore - 40);
                shipmentForEmail.RiskLevel = RiskLevel.Medium;
            }

            if (inventoryItem is not null &&
                inventoryItem.Status is InventoryStatus.AtRisk or InventoryStatus.Critical)
            {
                inventoryItem.Status = InventoryStatus.Watch;
                inventoryItem.CoverageDays = Math.Round(inventoryItem.CoverageDays + 2.5, 1);
            }
        }

        var analysisSnapshot = new RiskAnalysisResultDto
        {
            ShipmentId = shipmentForEmail?.Id ?? incident.ShipmentId ?? string.Empty,
            RiskScore = incident.RiskScore,
            Severity = incident.Severity.ToString().ToUpperInvariant(),
            DelayDays = delayForEmail,
            InventoryCoverageDays = coverageForEmail,
            ProjectedShortageUnits = incident.AffectedUnits > 0 ? incident.AffectedUnits : 13200,
            EstimatedFinancialImpact = costAvoided,
            PredictedStockout = true,
            Confidence = 91,
            Recommendations = incident.Recommendations
        };

        var email = await _emailService.SendCriticalIncidentAlertAsync(
            incident,
            shipmentForEmail,
            analysisSnapshot,
            cancellationToken);

        return new ExecuteContingencyResultDto
        {
            IncidentId = incident.Id,
            Status = IncidentStatus.CONTINGENCY_ACTIVATED.ToString(),
            ActionsExecuted = [.. DefaultActions],
            EstimatedCostAvoided = costAvoided,
            NotificationSent = email.Success
        };
    }

    private static RiskLevel ParseSeverity(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return RiskLevel.High;

        return Enum.TryParse<RiskLevel>(value, ignoreCase: true, out var parsed)
            ? parsed
            : value.ToUpperInvariant() switch
            {
                "CRITICAL" => RiskLevel.Critical,
                "HIGH" => RiskLevel.High,
                "MEDIUM" => RiskLevel.Medium,
                "LOW" => RiskLevel.Low,
                _ => RiskLevel.High
            };
    }
}
