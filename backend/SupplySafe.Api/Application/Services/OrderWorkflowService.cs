using System.Text.RegularExpressions;
using SupplySafe.Api.Application.DTOs;
using SupplySafe.Api.Application.Interfaces;
using SupplySafe.Api.Domain.Entities;
using SupplySafe.Api.Domain.Enums;
using SupplySafe.Api.Infrastructure.Persistence;

namespace SupplySafe.Api.Application.Services;

/// <summary>
/// Simulates enterprise flow: inbound email → work order → link shipment → risk → notify.
/// </summary>
public class OrderWorkflowService
{
    private readonly InMemoryStore _store;
    private readonly IEmailNotificationService _emailService;
    private readonly RiskAnalysisService _riskAnalysis;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrderWorkflowService> _logger;

    public OrderWorkflowService(
        InMemoryStore store,
        IEmailNotificationService emailService,
        RiskAnalysisService riskAnalysis,
        IConfiguration configuration,
        ILogger<OrderWorkflowService> logger)
    {
        _store = store;
        _emailService = emailService;
        _riskAnalysis = riskAnalysis;
        _configuration = configuration;
        _logger = logger;
    }

    public string GetOperationsEmail()
    {
        return Environment.GetEnvironmentVariable("SUPPLYSAFE_NOTIFY_EMAIL")
               ?? _configuration["Demo:OperationsEmail"]
               ?? "operations@supplysafe.demo";
    }

    public IReadOnlyList<WorkOrder> GetOrders() =>
        _store.Orders.OrderByDescending(o => o.CreatedAt).ToList();

    public WorkOrder? GetOrder(string id) =>
        _store.Orders.FirstOrDefault(o => o.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<CorporateEmail> GetMailbox() =>
        _store.Mailbox.OrderByDescending(m => m.ReceivedAt).ToList();

    public WorkOrder IngestFromEmail(IngestEmailOrderRequest request)
    {
        var opsInbox = string.IsNullOrWhiteSpace(request.To)
            ? "orders@supplysafe.demo"
            : request.To.Trim();

        var subject = string.IsNullOrWhiteSpace(request.Subject)
            ? "PO — Replenishment request"
            : request.Subject.Trim();

        var body = string.IsNullOrWhiteSpace(request.Body)
            ? "Product: Essential Rice\nQuantity: 10000\nDestination: Barranquilla"
            : request.Body;

        var mail = new CorporateEmail
        {
            Id = _store.NextMailId(),
            Direction = MailDirection.Inbound,
            From = string.IsNullOrWhiteSpace(request.From) ? "procurement@cliente-demo.com" : request.From.Trim(),
            To = opsInbox,
            Subject = subject,
            Body = body,
            ReceivedAt = DateTime.UtcNow,
            Processed = false
        };

        var parsed = ParseOrderFields(subject, body);
        var orderId = _store.NextOrderId();
        var order = new WorkOrder
        {
            Id = orderId,
            ExternalReference = parsed.Reference,
            Customer = InferCustomer(mail.From),
            Product = parsed.Product,
            RequestedUnits = parsed.Units,
            Destination = parsed.Destination,
            Status = OrderStatus.ReceivedFromEmail,
            SourceEmailId = mail.Id,
            CreatedAt = DateTime.UtcNow,
            Timeline =
            [
                $"[T+0] Inbound email {mail.Id} received from {mail.From}",
                $"[T+0] Subject: {mail.Subject}",
                $"[T+1m] Work order {orderId} created (ref {parsed.Reference})"
            ]
        };

        mail.LinkedOrderId = order.Id;
        mail.Processed = true;

        _store.Mailbox.Add(mail);
        _store.Orders.Add(order);

        _logger.LogInformation("Ingested order {OrderId} from email {MailId}", order.Id, mail.Id);
        return order;
    }

    public async Task<ProcessOrderResultDto?> ProcessAsync(
        string orderId,
        CancellationToken cancellationToken = default)
    {
        var order = GetOrder(orderId);
        if (order is null)
            return null;

        var notifyTo = GetOperationsEmail();
        order.Status = OrderStatus.Validating;
        order.Timeline.Add($"[T+process] Validation started for {order.ExternalReference}");

        var shipment = _store.Shipments.FirstOrDefault(s =>
            s.Product.Equals(order.Product, StringComparison.OrdinalIgnoreCase)
            && s.Destination.Contains(order.Destination, StringComparison.OrdinalIgnoreCase))
            ?? _store.Shipments.FirstOrDefault(s =>
                s.Product.Equals(order.Product, StringComparison.OrdinalIgnoreCase))
            ?? _store.Shipments.FirstOrDefault(s => s.Id == "SHP-2048");

        if (shipment is null)
        {
            order.Status = OrderStatus.Rejected;
            order.Timeline.Add("[T+process] No matching shipment — order rejected");
            order.ProcessedAt = DateTime.UtcNow;
            return new ProcessOrderResultDto
            {
                OrderId = order.Id,
                Status = order.Status.ToString(),
                Timeline = [.. order.Timeline],
                NotificationSent = false,
                NotificationRecipient = notifyTo,
                Summary = "Order rejected: no shipment matched the requested product/destination."
            };
        }

        order.Status = OrderStatus.LinkedToShipment;
        order.LinkedShipmentId = shipment.Id;
        order.Timeline.Add($"[T+process] Linked to shipment {shipment.Id} ({shipment.Route})");

        var analysis = await _riskAnalysis.AnalyzeAsync(shipment.Id, cancellationToken);
        var riskScore = analysis?.RiskScore ?? shipment.RiskScore;
        order.Timeline.Add($"[T+process] Risk analysis complete — score {riskScore}/100 ({analysis?.Severity ?? shipment.RiskLevel.ToString()})");

        string? incidentId = order.LinkedIncidentId;
        if (riskScore >= 70)
        {
            order.Status = OrderStatus.AtRisk;
            var existing = _store.Incidents.FirstOrDefault(i =>
                i.ShipmentId == shipment.Id && i.Status != IncidentStatus.RESOLVED);

            if (existing is null)
            {
                var incident = new Incident
                {
                    Id = _store.NextIncidentId(),
                    Title = $"Order {order.ExternalReference} blocked by supply risk on {shipment.Id}",
                    Severity = RiskLevel.Critical,
                    Status = IncidentStatus.OPEN,
                    RiskScore = riskScore,
                    AffectedUnits = analysis?.ProjectedShortageUnits ?? order.RequestedUnits,
                    EstimatedLoss = analysis?.EstimatedFinancialImpact ?? shipment.Value,
                    CreatedAt = DateTime.UtcNow,
                    ShipmentId = shipment.Id,
                    Recommendations = analysis?.Recommendations ??
                    [
                        "Divert shipment to alternate route",
                        "Activate secondary supplier",
                        "Prioritize existing inventory"
                    ]
                };
                _store.Incidents.Add(incident);
                incidentId = incident.Id;
                order.Timeline.Add($"[T+process] Opened incident {incident.Id}");
            }
            else
            {
                incidentId = existing.Id;
                order.Timeline.Add($"[T+process] Reused open incident {existing.Id}");
            }

            order.LinkedIncidentId = incidentId;
            order.Status = OrderStatus.ContingencyRecommended;
            order.Timeline.Add("[T+process] Contingency recommended — awaiting Ops execute");
        }
        else
        {
            order.Status = OrderStatus.Processing;
            order.Timeline.Add("[T+process] Risk acceptable — order moved to fulfillment queue");
            order.Status = OrderStatus.Completed;
            order.ProcessedAt = DateTime.UtcNow;
            order.Timeline.Add("[T+process] Order completed without contingency");
        }

        var impactUsd = analysis?.EstimatedFinancialImpact;
        var impactCop = analysis?.EstimatedFinancialImpactCop;
        var trm = analysis?.TrmUsdCop;

        if (impactUsd is > 0 && trm is > 0)
        {
            order.Timeline.Add(
                $"[T+process] FX/TRM applied — impact ${impactUsd:N0} USD ≈ ${impactCop:N0} COP (TRM {trm:N2})");
        }

        var emailBody =
            $"""
            SUPPLYSAFE — ORDER WORKFLOW UPDATE

            Order: {order.Id}
            External ref: {order.ExternalReference}
            Customer: {order.Customer}
            Product: {order.Product}
            Units: {order.RequestedUnits:N0}
            Destination: {order.Destination}

            Linked shipment: {order.LinkedShipmentId}
            Risk score: {riskScore}/100
            Status: {order.Status}
            Incident: {order.LinkedIncidentId ?? "n/a"}
            Estimated impact: {(impactUsd is null ? "n/a" : $"${impactUsd:N0} USD")} / {(impactCop is null ? "n/a" : $"${impactCop:N0} COP")}
            TRM USD/COP: {(trm is null ? "n/a" : $"{trm:N2}")}

            Timeline:
            {string.Join('\n', order.Timeline.TakeLast(8))}

            — SupplySafe AI Operations Bot
            """;

        var email = await _emailService.SendAsync(new SendEmailRequest
        {
            Recipient = notifyTo,
            Subject = $"📦 SUPPLYSAFE — Order {order.ExternalReference} → {order.Status}",
            Body = emailBody
        }, cancellationToken);

        // Mirror outbound mail in corporate mailbox
        _store.Mailbox.Add(new CorporateEmail
        {
            Id = _store.NextMailId(),
            Direction = MailDirection.Outbound,
            From = _configuration["Smtp:From"] ?? "alerts@supplysafe.demo",
            To = notifyTo,
            Subject = $"📦 SUPPLYSAFE — Order {order.ExternalReference} → {order.Status}",
            Body = emailBody,
            ReceivedAt = DateTime.UtcNow,
            Processed = true,
            LinkedOrderId = order.Id
        });

        if (order.Status is OrderStatus.ContingencyRecommended or OrderStatus.AtRisk)
            order.ProcessedAt ??= DateTime.UtcNow;

        return new ProcessOrderResultDto
        {
            OrderId = order.Id,
            Status = order.Status.ToString(),
            LinkedShipmentId = order.LinkedShipmentId,
            LinkedIncidentId = order.LinkedIncidentId,
            RiskScore = riskScore,
            EstimatedImpactUsd = impactUsd,
            EstimatedImpactCop = impactCop,
            TrmUsdCop = trm,
            NotificationSent = email.Success,
            NotificationRecipient = notifyTo,
            Timeline = [.. order.Timeline],
            Summary = riskScore >= 70
                ? "Order linked to at-risk shipment. Contingency recommended. Ops notified by email."
                : "Order processed successfully with acceptable supply risk."
        };
    }

    private static (string Reference, string Product, int Units, string Destination) ParseOrderFields(
        string subject,
        string body)
    {
        var text = $"{subject}\n{body}";

        var refMatch = Regex.Match(text, @"PO[- ]?\d+", RegexOptions.IgnoreCase);
        var reference = refMatch.Success ? refMatch.Value.ToUpperInvariant().Replace(' ', '-') : $"PO-{DateTime.UtcNow:HHmmss}";

        var product = "Essential Rice";
        if (text.Contains("Medical", StringComparison.OrdinalIgnoreCase))
            product = "Medical Supplies Kit";
        else if (text.Contains("Lubricant", StringComparison.OrdinalIgnoreCase))
            product = "Industrial Lubricants";
        else if (text.Contains("Coffee", StringComparison.OrdinalIgnoreCase) || text.Contains("Film", StringComparison.OrdinalIgnoreCase))
            product = "Coffee Packaging Film";
        else if (text.Contains("Semiconductor", StringComparison.OrdinalIgnoreCase))
            product = "Semiconductor Components";
        else if (text.Contains("Rice", StringComparison.OrdinalIgnoreCase))
            product = "Essential Rice";

        var qtyMatch = Regex.Match(text, @"(?:Quantity|Units|x)\s*:?\s*([\d,\.]+)", RegexOptions.IgnoreCase);
        var units = 10000;
        if (qtyMatch.Success && int.TryParse(qtyMatch.Groups[1].Value.Replace(",", "").Replace(".", ""), out var q))
            units = q;

        var destination = text.Contains("Cartagena", StringComparison.OrdinalIgnoreCase) ? "Cartagena"
            : text.Contains("Bogotá", StringComparison.OrdinalIgnoreCase) || text.Contains("Bogota", StringComparison.OrdinalIgnoreCase) ? "Bogotá"
            : text.Contains("Cali", StringComparison.OrdinalIgnoreCase) ? "Cali"
            : "Barranquilla";

        return (reference, product, units, destination);
    }

    private static string InferCustomer(string from)
    {
        if (string.IsNullOrWhiteSpace(from))
            return "Demo Customer";

        var at = from.IndexOf('@');
        if (at <= 0)
            return from;

        var domain = from[(at + 1)..];
        var name = domain.Split('.')[0];
        return char.ToUpperInvariant(name[0]) + name[1..];
    }
}
