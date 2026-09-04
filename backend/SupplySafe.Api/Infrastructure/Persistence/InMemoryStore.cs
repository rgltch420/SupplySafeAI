using SupplySafe.Api.Domain.Entities;
using SupplySafe.Api.Domain.Enums;

namespace SupplySafe.Api.Infrastructure.Persistence;

/// <summary>
/// In-memory mock store seeded for the hackathon demo. Thread-safe via locks.
/// </summary>
public class InMemoryStore
{
    private readonly object _lock = new();
    private int _incidentSeq = 2049;
    private int _messageSeq = 2048;
    private int _planSeq = 1000;
    private int _orderSeq = 3001;
    private int _mailSeq = 100;

    public List<Shipment> Shipments { get; } = [];
    public List<InventoryItem> Inventory { get; } = [];
    public List<SupplyRisk> Risks { get; } = [];
    public List<Incident> Incidents { get; } = [];
    public List<Notification> Notifications { get; } = [];
    public List<ContingencyPlan> ContingencyPlans { get; } = [];
    public List<WorkOrder> Orders { get; } = [];
    public List<CorporateEmail> Mailbox { get; } = [];

    public InMemoryStore()
    {
        Seed();
    }

    public string NextIncidentId()
    {
        lock (_lock)
        {
            return $"INC-{_incidentSeq++}";
        }
    }

    public string NextMessageId()
    {
        lock (_lock)
        {
            return $"MSG-{_messageSeq++}";
        }
    }

    public string NextPlanId()
    {
        lock (_lock)
        {
            return $"PLAN-{_planSeq++}";
        }
    }

    public string NextOrderId()
    {
        lock (_lock)
        {
            return $"ORD-{_orderSeq++}";
        }
    }

    public string NextMailId()
    {
        lock (_lock)
        {
            return $"MAIL-{_mailSeq++}";
        }
    }

    /// <summary>Clears runtime state and reloads burned demo seed (for pitch re-runs).</summary>
    public void Reset()
    {
        lock (_lock)
        {
            Shipments.Clear();
            Inventory.Clear();
            Risks.Clear();
            Incidents.Clear();
            Notifications.Clear();
            ContingencyPlans.Clear();
            Orders.Clear();
            Mailbox.Clear();
            _incidentSeq = 2049;
            _messageSeq = 2048;
            _planSeq = 1000;
            _orderSeq = 3001;
            _mailSeq = 100;
            Seed();
        }
    }

    private void Seed()
    {
        var now = DateTime.UtcNow;

        Shipments.AddRange(
        [
            new Shipment
            {
                Id = "SHP-2048",
                Product = "Essential Rice",
                Origin = "Shanghai",
                Destination = "Barranquilla",
                Route = "Shanghai → Cartagena → Barranquilla",
                Status = ShipmentStatus.Delayed,
                OriginalEta = now.AddDays(3),
                Eta = now.AddDays(9),
                DelayDays = 6,
                RiskScore = 87,
                Value = 84600m,
                Units = 22000,
                RiskLevel = RiskLevel.Critical
            },
            new Shipment
            {
                Id = "SHP-1901",
                Product = "Medical Supplies Kit",
                Origin = "Miami",
                Destination = "Cartagena",
                Route = "Miami → Cartagena",
                Status = ShipmentStatus.InTransit,
                OriginalEta = now.AddDays(2),
                Eta = now.AddDays(2),
                DelayDays = 0,
                RiskScore = 18,
                Value = 125000m,
                Units = 4500,
                RiskLevel = RiskLevel.Low
            },
            new Shipment
            {
                Id = "SHP-1877",
                Product = "Industrial Lubricants",
                Origin = "Rotterdam",
                Destination = "Buenaventura",
                Route = "Rotterdam → Buenaventura → Cali",
                Status = ShipmentStatus.AtRisk,
                OriginalEta = now.AddDays(8),
                Eta = now.AddDays(11),
                DelayDays = 3,
                RiskScore = 54,
                Value = 210000m,
                Units = 8000,
                RiskLevel = RiskLevel.Medium
            },
            new Shipment
            {
                Id = "SHP-1760",
                Product = "Coffee Packaging Film",
                Origin = "São Paulo",
                Destination = "Barranquilla",
                Route = "São Paulo → Barranquilla",
                Status = ShipmentStatus.InTransit,
                OriginalEta = now.AddDays(4),
                Eta = now.AddDays(5),
                DelayDays = 1,
                RiskScore = 28,
                Value = 42000m,
                Units = 12000,
                RiskLevel = RiskLevel.Low
            },
            new Shipment
            {
                Id = "SHP-1655",
                Product = "Semiconductor Components",
                Origin = "Shenzhen",
                Destination = "Bogotá",
                Route = "Shenzhen → Panama → Bogotá",
                Status = ShipmentStatus.AtRisk,
                OriginalEta = now.AddDays(6),
                Eta = now.AddDays(10),
                DelayDays = 4,
                RiskScore = 62,
                Value = 980000m,
                Units = 1500,
                RiskLevel = RiskLevel.Medium
            }
        ]);

        Inventory.AddRange(
        [
            new InventoryItem
            {
                Id = "INV-RICE-01",
                Product = "Essential Rice",
                Quantity = 15620,
                DailyConsumption = 2200,
                CoverageDays = 7.1,
                ReorderPoint = 20000,
                Status = InventoryStatus.AtRisk
            },
            new InventoryItem
            {
                Id = "INV-MED-02",
                Product = "Medical Supplies Kit",
                Quantity = 9200,
                DailyConsumption = 310,
                CoverageDays = 29.7,
                ReorderPoint = 2500,
                Status = InventoryStatus.Healthy
            },
            new InventoryItem
            {
                Id = "INV-LUB-03",
                Product = "Industrial Lubricants",
                Quantity = 4100,
                DailyConsumption = 480,
                CoverageDays = 8.5,
                ReorderPoint = 3500,
                Status = InventoryStatus.Watch
            },
            new InventoryItem
            {
                Id = "INV-FILM-04",
                Product = "Coffee Packaging Film",
                Quantity = 18000,
                DailyConsumption = 900,
                CoverageDays = 20.0,
                ReorderPoint = 6000,
                Status = InventoryStatus.Healthy
            },
            new InventoryItem
            {
                Id = "INV-SEMI-05",
                Product = "Semiconductor Components",
                Quantity = 2200,
                DailyConsumption = 180,
                CoverageDays = 12.2,
                ReorderPoint = 1500,
                Status = InventoryStatus.Watch
            }
        ]);

        Risks.AddRange(
        [
            new SupplyRisk
            {
                Id = "RSK-WX-01",
                Type = RiskType.Weather,
                Severity = RiskLevel.Critical,
                Score = 92,
                Title = "Severe tropical storm — Caribbean corridor",
                Description =
                    "Severe weather event impacting Shanghai→Cartagena→Barranquilla corridor. Port operations degraded; vessel ETA slipped +6 days for Essential Rice (SHP-2048).",
                AffectedShipments = ["SHP-2048"],
                DetectedAt = now.AddHours(-6)
            },
            new SupplyRisk
            {
                Id = "RSK-PORT-02",
                Type = RiskType.PortCongestion,
                Severity = RiskLevel.High,
                Score = 71,
                Title = "Cartagena port congestion elevated",
                Description =
                    "Berth wait times above 48h. Cascading delays for Asia–Caribbean feeders; SHP-2048 and SHP-1655 partially exposed.",
                AffectedShipments = ["SHP-2048", "SHP-1655"],
                DetectedAt = now.AddHours(-10)
            },
            new SupplyRisk
            {
                Id = "RSK-GEO-03",
                Type = RiskType.Geopolitical,
                Severity = RiskLevel.Medium,
                Score = 48,
                Title = "Trade corridor advisory — Panama transit",
                Description =
                    "Heightened inspection regime on Panama transit lanes. Medium impact on Shenzhen→Bogotá semiconductor lane.",
                AffectedShipments = ["SHP-1655"],
                DetectedAt = now.AddHours(-18)
            }
        ]);

        Incidents.Add(new Incident
        {
            Id = "INC-2048",
            Title = "Critical delay — Essential Rice SHP-2048 (Shanghai → Barranquilla)",
            Severity = RiskLevel.Critical,
            Status = IncidentStatus.OPEN,
            RiskScore = 87,
            AffectedUnits = 13200,
            EstimatedLoss = 84600m,
            CreatedAt = now.AddHours(-2),
            ShipmentId = "SHP-2048",
            Recommendations =
            [
                "Divert shipment to alternate route",
                "Activate secondary supplier",
                "Prioritize existing inventory",
                "Notify Operations",
                "Notify Procurement"
            ],
            ActionsExecuted = []
        });

        ContingencyPlans.Add(new ContingencyPlan
        {
            Id = "PLAN-2048",
            IncidentId = "INC-2048",
            Actions =
            [
                "Divert shipment to alternate route",
                "Activate secondary supplier",
                "Prioritize existing inventory",
                "Notify Operations",
                "Notify Procurement"
            ],
            EstimatedCostAvoided = 84600m,
            Status = ContingencyPlanStatus.Pending,
            ExecutedAt = null
        });

        // --- Simulated corporate mailbox + burned work orders ---
        Mailbox.Add(new CorporateEmail
        {
            Id = "MAIL-001",
            Direction = MailDirection.Inbound,
            From = "compras@almacenespacifico.com",
            To = "orders@supplysafe.demo",
            Subject = "PO-77821 — Urgent replenishment Essential Rice 15,000 units — Barranquilla DC",
            Body =
                """
                Hello SupplySafe Ops,

                Please process purchase order PO-77821.
                Product: Essential Rice
                Quantity: 15000
                Destination: Barranquilla
                Required by: next week
                Linked preferred lane: Shanghai → Cartagena → Barranquilla

                Regards,
                Procurement — Almacenes Pacífico
                """,
            ReceivedAt = now.AddHours(-5),
            Processed = true,
            LinkedOrderId = "ORD-3000"
        });

        Mailbox.Add(new CorporateEmail
        {
            Id = "MAIL-002",
            Direction = MailDirection.Inbound,
            From = "ops@clinicacaribe.org",
            To = "orders@supplysafe.demo",
            Subject = "PO-77910 — Medical Supplies Kit restock Cartagena",
            Body =
                """
                Requesting Medical Supplies Kit x 2,000 units for Cartagena warehouse.
                Reference: PO-77910
                """,
            ReceivedAt = now.AddHours(-3),
            Processed = false,
            LinkedOrderId = null
        });

        Orders.Add(new WorkOrder
        {
            Id = "ORD-3000",
            ExternalReference = "PO-77821",
            Customer = "Almacenes Pacífico",
            Product = "Essential Rice",
            RequestedUnits = 15000,
            Destination = "Barranquilla",
            Status = OrderStatus.AtRisk,
            SourceEmailId = "MAIL-001",
            LinkedShipmentId = "SHP-2048",
            LinkedIncidentId = "INC-2048",
            CreatedAt = now.AddHours(-5),
            Timeline =
            [
                "[T+0] Email received from compras@almacenespacifico.com",
                "[T+1m] Order ORD-3000 created from PO-77821",
                "[T+3m] Linked to shipment SHP-2048 (Essential Rice)",
                "[T+5m] Risk engine flagged CRITICAL (score 87) — weather delay +6 days",
                "[T+6m] Incident INC-2048 opened — awaiting contingency activation"
            ]
        });

        Orders.Add(new WorkOrder
        {
            Id = "ORD-2990",
            ExternalReference = "PO-77002",
            Customer = "Distribuidora Norte",
            Product = "Coffee Packaging Film",
            RequestedUnits = 5000,
            Destination = "Barranquilla",
            Status = OrderStatus.Completed,
            SourceEmailId = null,
            LinkedShipmentId = "SHP-1760",
            LinkedIncidentId = null,
            CreatedAt = now.AddDays(-2),
            ProcessedAt = now.AddDays(-1),
            Timeline =
            [
                "[T+0] Order received via EDI bridge",
                "[T+10m] Linked to healthy shipment SHP-1760",
                "[T+1d] Fulfilled — no contingency required"
            ]
        });
    }
}
