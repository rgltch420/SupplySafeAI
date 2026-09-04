import {
  DashboardDto,
  ExecuteContingencyResult,
  Incident,
  InventoryItem,
  RiskAnalysisResult,
  SendEmailResult,
  Shipment,
  SupplyRisk
} from '../models/api.models';

const now = Date.now();
const days = (n: number) => new Date(now + n * 86_400_000).toISOString();
const hoursAgo = (h: number) => new Date(now - h * 3_600_000).toISOString();

export const MOCK_SHIPMENTS: Shipment[] = [
  {
    id: 'SHP-2048',
    product: 'Essential Rice',
    origin: 'Shanghai',
    destination: 'Barranquilla',
    route: 'Shanghai → Cartagena → Barranquilla',
    status: 'Delayed',
    originalEta: days(3),
    eta: days(9),
    delayDays: 6,
    riskScore: 87,
    value: 84600,
    units: 22000,
    riskLevel: 'Critical'
  },
  {
    id: 'SHP-1901',
    product: 'Medical Supplies Kit',
    origin: 'Miami',
    destination: 'Cartagena',
    route: 'Miami → Cartagena',
    status: 'InTransit',
    originalEta: days(2),
    eta: days(2),
    delayDays: 0,
    riskScore: 18,
    value: 125000,
    units: 4500,
    riskLevel: 'Low'
  },
  {
    id: 'SHP-1877',
    product: 'Industrial Lubricants',
    origin: 'Rotterdam',
    destination: 'Buenaventura',
    route: 'Rotterdam → Buenaventura → Cali',
    status: 'AtRisk',
    originalEta: days(8),
    eta: days(11),
    delayDays: 3,
    riskScore: 54,
    value: 210000,
    units: 8000,
    riskLevel: 'Medium'
  },
  {
    id: 'SHP-1760',
    product: 'Coffee Packaging Film',
    origin: 'São Paulo',
    destination: 'Barranquilla',
    route: 'São Paulo → Barranquilla',
    status: 'InTransit',
    originalEta: days(4),
    eta: days(5),
    delayDays: 1,
    riskScore: 28,
    value: 42000,
    units: 12000,
    riskLevel: 'Low'
  },
  {
    id: 'SHP-1655',
    product: 'Semiconductor Components',
    origin: 'Shenzhen',
    destination: 'Bogotá',
    route: 'Shenzhen → Panama → Bogotá',
    status: 'AtRisk',
    originalEta: days(6),
    eta: days(10),
    delayDays: 4,
    riskScore: 62,
    value: 980000,
    units: 1500,
    riskLevel: 'Medium'
  }
];

export const MOCK_INVENTORY: InventoryItem[] = [
  {
    id: 'INV-RICE-01',
    product: 'Essential Rice',
    quantity: 15620,
    dailyConsumption: 2200,
    coverageDays: 7.1,
    reorderPoint: 20000,
    status: 'AtRisk'
  },
  {
    id: 'INV-MED-02',
    product: 'Medical Supplies Kit',
    quantity: 9200,
    dailyConsumption: 310,
    coverageDays: 29.7,
    reorderPoint: 2500,
    status: 'Healthy'
  },
  {
    id: 'INV-LUB-03',
    product: 'Industrial Lubricants',
    quantity: 4100,
    dailyConsumption: 480,
    coverageDays: 8.5,
    reorderPoint: 3500,
    status: 'Watch'
  },
  {
    id: 'INV-FILM-04',
    product: 'Coffee Packaging Film',
    quantity: 18000,
    dailyConsumption: 900,
    coverageDays: 20.0,
    reorderPoint: 6000,
    status: 'Healthy'
  },
  {
    id: 'INV-SEMI-05',
    product: 'Semiconductor Components',
    quantity: 2200,
    dailyConsumption: 180,
    coverageDays: 12.2,
    reorderPoint: 1500,
    status: 'Watch'
  }
];

export const MOCK_RISKS: SupplyRisk[] = [
  {
    id: 'RSK-WX-01',
    type: 'Weather',
    severity: 'Critical',
    score: 92,
    title: 'Severe tropical storm — Caribbean corridor',
    description:
      'Severe weather event impacting Shanghai→Cartagena→Barranquilla corridor. Port operations degraded; vessel ETA slipped +6 days for Essential Rice (SHP-2048).',
    affectedShipments: ['SHP-2048'],
    detectedAt: hoursAgo(6)
  },
  {
    id: 'RSK-PORT-02',
    type: 'PortCongestion',
    severity: 'High',
    score: 71,
    title: 'Cartagena port congestion elevated',
    description:
      'Berth wait times above 48h. Cascading delays for Asia–Caribbean feeders; SHP-2048 and SHP-1655 partially exposed.',
    affectedShipments: ['SHP-2048', 'SHP-1655'],
    detectedAt: hoursAgo(10)
  },
  {
    id: 'RSK-GEO-03',
    type: 'Geopolitical',
    severity: 'Medium',
    score: 48,
    title: 'Trade corridor advisory — Panama transit',
    description:
      'Heightened inspection regime on Panama transit lanes. Medium impact on Shenzhen→Bogotá semiconductor lane.',
    affectedShipments: ['SHP-1655'],
    detectedAt: hoursAgo(18)
  }
];

export const MOCK_INCIDENT: Incident = {
  id: 'INC-2048',
  title: 'Critical delay — Essential Rice SHP-2048 (Shanghai → Barranquilla)',
  severity: 'Critical',
  status: 'OPEN',
  riskScore: 87,
  affectedUnits: 13200,
  estimatedLoss: 84600,
  createdAt: hoursAgo(2),
  shipmentId: 'SHP-2048',
  recommendations: [
    'Divert shipment to alternate route',
    'Activate secondary supplier',
    'Prioritize existing inventory',
    'Notify Operations',
    'Notify Procurement'
  ],
  actionsExecuted: []
};

export const MOCK_INCIDENTS: Incident[] = [MOCK_INCIDENT];

export const MOCK_DASHBOARD: DashboardDto = {
  cargoMonitored: 2_400_000,
  riskScore: 87,
  activeIncidents: 3,
  supplyReliability: 94,
  shipmentsAtRisk: 2,
  inventoryAtRisk: 1
};

export const MOCK_ANALYSIS: RiskAnalysisResult = {
  shipmentId: 'SHP-2048',
  riskScore: 87,
  severity: 'CRITICAL',
  delayDays: 6,
  inventoryCoverageDays: 7.1,
  projectedShortageUnits: 13200,
  estimatedFinancialImpact: 84600,
  predictedStockout: true,
  confidence: 91,
  recommendations: [
    'Divert shipment to alternate route',
    'Activate secondary supplier',
    'Prioritize existing inventory'
  ]
};

export const MOCK_EXECUTE: ExecuteContingencyResult = {
  incidentId: 'INC-2048',
  status: 'CONTINGENCY_ACTIVATED',
  actionsExecuted: [
    'Alternative route activated',
    'Secondary supplier activated',
    'Inventory prioritized',
    'Operations notified',
    'Procurement notified'
  ],
  estimatedCostAvoided: 84600,
  notificationSent: true
};

export const MOCK_EMAIL: SendEmailResult = {
  success: true,
  recipient: 'operations@supplysafe.demo',
  subject: 'SUPPLYSAFE - Critical Supply Chain Risk',
  messageId: 'MSG-2048'
};

export function cloneShipment(id: string): Shipment | undefined {
  const found = MOCK_SHIPMENTS.find((s) => s.id === id);
  return found ? structuredClone(found) : undefined;
}
