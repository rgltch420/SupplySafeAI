export type RiskLevel = 'Low' | 'Medium' | 'High' | 'Critical';
export type ShipmentStatus = 'InTransit' | 'Delayed' | 'AtRisk' | 'Diverted' | 'Delivered';
export type InventoryStatus = 'Healthy' | 'Watch' | 'AtRisk' | 'Critical';
export type IncidentStatus = 'OPEN' | 'ANALYZING' | 'CONTINGENCY_ACTIVATED' | 'RESOLVED';
export type RiskType =
  | 'Weather'
  | 'PortCongestion'
  | 'Geopolitical'
  | 'Inventory'
  | 'Shipment'
  | 'SupplyChain';

export interface DashboardDto {
  cargoMonitored: number;
  riskScore: number;
  activeIncidents: number;
  supplyReliability: number;
  shipmentsAtRisk: number;
  inventoryAtRisk: number;
}

export interface Shipment {
  id: string;
  product: string;
  origin: string;
  destination: string;
  route: string;
  status: ShipmentStatus;
  eta: string;
  originalEta: string;
  delayDays: number;
  riskScore: number;
  value: number;
  units: number;
  riskLevel: RiskLevel;
}

export interface InventoryItem {
  id: string;
  product: string;
  quantity: number;
  dailyConsumption: number;
  coverageDays: number;
  reorderPoint: number;
  status: InventoryStatus;
}

export interface SupplyRisk {
  id: string;
  type: RiskType;
  severity: RiskLevel;
  score: number;
  title: string;
  description: string;
  affectedShipments: string[];
  detectedAt: string;
}

export interface Incident {
  id: string;
  title: string;
  severity: RiskLevel;
  status: IncidentStatus;
  riskScore: number;
  affectedUnits: number;
  estimatedLoss: number;
  createdAt: string;
  recommendations: string[];
  actionsExecuted: string[];
  shipmentId?: string | null;
}

export interface RiskAnalysisResult {
  shipmentId: string;
  riskScore: number;
  severity: string;
  delayDays: number;
  inventoryCoverageDays: number;
  projectedShortageUnits: number;
  estimatedFinancialImpact: number;
  predictedStockout: boolean;
  confidence: number;
  recommendations: string[];
}

export interface ExecuteContingencyResult {
  incidentId: string;
  status: string;
  actionsExecuted: string[];
  estimatedCostAvoided: number;
  notificationSent: boolean;
}

export interface SendEmailRequest {
  recipient?: string;
  subject?: string;
  body?: string;
  incidentId?: string;
}

export interface SendEmailResult {
  success: boolean;
  recipient: string;
  subject: string;
  messageId: string;
}
