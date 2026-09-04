import { CurrencyPipe, DatePipe, DecimalPipe, NgClass } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { forkJoin } from 'rxjs';
import {
  DashboardDto,
  ExecuteContingencyResult,
  Incident,
  InventoryItem,
  RiskAnalysisResult,
  Shipment,
  SupplyRisk
} from '../../models/api.models';
import { ApiService } from '../../services/api.service';
import { DemoStateService } from '../../services/demo-state.service';
import { esLabel, esPhrase } from '../../shared/i18n/es-labels';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';

interface TimelineEvent {
  time: string;
  label: string;
  tone: 'cyan' | 'amber' | 'red' | 'green';
}

@Component({
  selector: 'app-mission-control',
  standalone: true,
  imports: [DecimalPipe, CurrencyPipe, DatePipe, NgClass, StatusBadgeComponent],
  templateUrl: './mission-control.component.html',
  styleUrl: './mission-control.component.scss'
})
export class MissionControlComponent implements OnInit {
  private readonly api = inject(ApiService);
  readonly demo = inject(DemoStateService);
  readonly Math = Math;
  readonly es = esPhrase;

  loading = true;
  dashboard: DashboardDto | null = null;
  shipments: Shipment[] = [];
  inventory: InventoryItem[] = [];
  risks: SupplyRisk[] = [];
  incident: Incident | null = null;
  critical: Shipment | null = null;

  analysis: RiskAnalysisResult | null = null;
  executeResult: ExecuteContingencyResult | null = null;
  tickedActions: string[] = [];
  lossAvoidedVisible = false;
  emailSent = false;
  busy = false;
  timeline: TimelineEvent[] = [
    { time: 'T-6h', label: 'Señal climática RSK-WX-01 detectada', tone: 'cyan' },
    { time: 'T-2h', label: 'Incidente INC-2048 abierto — Arroz esencial', tone: 'amber' },
    { time: 'AHORA', label: 'Esperando simulación de disrupción', tone: 'red' }
  ];

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    this.loading = true;
    forkJoin({
      dashboard: this.api.getDashboard(),
      shipments: this.api.getShipments(),
      inventory: this.api.getInventory(),
      risks: this.api.getRisks(),
      incident: this.api.getIncident('INC-2048')
    }).subscribe({
      next: ({ dashboard, shipments, inventory, risks, incident }) => {
        this.dashboard = dashboard;
        this.shipments = shipments;
        this.inventory = inventory;
        this.risks = risks;
        this.incident = incident;
        this.critical = shipments.find((s) => s.id === 'SHP-2048') ?? shipments[0] ?? null;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  productName(product: string | null | undefined): string {
    return esLabel(product) || 'Arroz esencial';
  }

  simulateDisruption(): void {
    if (this.busy) return;
    this.busy = true;
    this.demo.phase$.next('analyzing');
    this.analysis = null;
    this.executeResult = null;
    this.tickedActions = [];
    this.lossAvoidedVisible = false;
    this.emailSent = false;

    this.pushTimeline('SIM', 'Simulación de disrupción en curso…', 'amber');

    this.api.analyzeRisk('SHP-2048').subscribe({
      next: (result) => {
        this.analysis = result;
        this.demo.analysis$.next(result);
        this.demo.phase$.next('analyzed');
        if (this.dashboard) {
          this.dashboard = { ...this.dashboard, riskScore: result.riskScore };
        }
        if (this.critical) {
          this.critical = {
            ...this.critical,
            riskScore: result.riskScore,
            delayDays: result.delayDays,
            riskLevel: 'Critical',
            status: 'Delayed'
          };
        }
        this.pushTimeline(
          'IA',
          `Riesgo ${result.riskScore} · ${result.projectedShortageUnits.toLocaleString('es-CO')} unidades en riesgo`,
          'red'
        );
        this.busy = false;
      },
      error: () => {
        this.busy = false;
        this.demo.phase$.next('idle');
      }
    });
  }

  executeContingency(): void {
    if (this.busy || !this.analysis) return;
    this.busy = true;
    this.demo.phase$.next('executing');
    this.tickedActions = [];
    this.lossAvoidedVisible = false;
    this.pushTimeline('ACT', 'Ejecutando plan de contingencia…', 'cyan');

    this.api.executeContingency('INC-2048').subscribe({
      next: async (result) => {
        this.executeResult = result;
        this.demo.executeResult$.next(result);

        for (const action of result.actionsExecuted) {
          await this.delay(380);
          this.tickedActions = [...this.tickedActions, esPhrase(action)];
          this.demo.tickedActions$.next(this.tickedActions);
        }

        await this.delay(420);
        this.lossAvoidedVisible = true;
        this.demo.lossAvoidedVisible$.next(true);

        if (this.incident) {
          this.incident = {
            ...this.incident,
            status: 'CONTINGENCY_ACTIVATED',
            actionsExecuted: result.actionsExecuted
          };
        }

        this.api
          .sendEmail({
            recipient: 'operations@supplysafe.demo',
            subject: 'SUPPLYSAFE - Riesgo crítico en cadena de suministro',
            body:
              'Retraso crítico de Arroz esencial en SHP-2048. Contingencia activada. Notificar a Operaciones y Compras.',
            incidentId: 'INC-2048'
          })
          .subscribe((email) => {
            this.emailSent = email.success;
            this.demo.emailResult$.next(email);
            this.pushTimeline(
              'MAIL',
              `Correo enviado → Operaciones / Compras · ${email.subject}`,
              'green'
            );
          });

        this.pushTimeline(
          'OK',
          `$${result.estimatedCostAvoided.toLocaleString('es-CO')} PÉRDIDA EVITADA · ${esLabel(result.status)}`,
          'green'
        );
        this.demo.phase$.next('executed');
        this.busy = false;
        this.refresh();
      },
      error: () => {
        this.busy = false;
        this.demo.phase$.next('analyzed');
      }
    });
  }

  get routeStops(): string[] {
    return (this.critical?.route ?? 'Shanghái → Cartagena → Barranquilla')
      .split('→')
      .map((s) => s.trim());
  }

  get impactUnits(): number {
    return this.analysis?.projectedShortageUnits ?? this.incident?.affectedUnits ?? 13200;
  }

  get impactLoss(): number {
    return this.analysis?.estimatedFinancialImpact ?? this.incident?.estimatedLoss ?? 84600;
  }

  private pushTimeline(time: string, label: string, tone: TimelineEvent['tone']): void {
    this.timeline = [{ time, label, tone }, ...this.timeline].slice(0, 8);
  }

  private delay(ms: number): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms));
  }
}
