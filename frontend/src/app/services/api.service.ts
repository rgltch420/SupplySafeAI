import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, catchError, map, of, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  MOCK_ANALYSIS,
  MOCK_DASHBOARD,
  MOCK_EMAIL,
  MOCK_EXECUTE,
  MOCK_INCIDENT,
  MOCK_INCIDENTS,
  MOCK_INVENTORY,
  MOCK_RISKS,
  MOCK_SHIPMENTS,
  cloneShipment
} from '../data/mock-data';
import {
  DashboardDto,
  ExecuteContingencyResult,
  Incident,
  InventoryItem,
  RiskAnalysisResult,
  SendEmailRequest,
  SendEmailResult,
  Shipment,
  SupplyRisk
} from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiBaseUrl;
  readonly usingMocks = signal(false);
  readonly apiBaseUrl = environment.apiBaseUrl;

  /** Probe backend; updates SYSTEM OPERATIONAL / MOCK MODE banner. */
  ping(): Observable<boolean> {
    return this.http.get<DashboardDto>(`${this.base}/dashboard`).pipe(
      tap(() => this.usingMocks.set(false)),
      map(() => true),
      catchError(() => {
        this.usingMocks.set(true);
        return of(false);
      })
    );
  }

  getDashboard(): Observable<DashboardDto> {
    return this.get<DashboardDto>('/dashboard', structuredClone(MOCK_DASHBOARD));
  }

  getShipments(): Observable<Shipment[]> {
    return this.get<Shipment[]>('/shipments', structuredClone(MOCK_SHIPMENTS));
  }

  getShipment(id: string): Observable<Shipment> {
    const fallback = cloneShipment(id) ?? structuredClone(MOCK_SHIPMENTS[0]);
    return this.get<Shipment>(`/shipments/${id}`, fallback);
  }

  getInventory(): Observable<InventoryItem[]> {
    return this.get<InventoryItem[]>('/inventory', structuredClone(MOCK_INVENTORY));
  }

  getRisks(): Observable<SupplyRisk[]> {
    return this.get<SupplyRisk[]>('/risks', structuredClone(MOCK_RISKS));
  }

  getIncidents(): Observable<Incident[]> {
    return this.get<Incident[]>('/incidents', structuredClone(MOCK_INCIDENTS));
  }

  getIncident(id: string): Observable<Incident> {
    const fallback =
      id === 'INC-2048'
        ? structuredClone(MOCK_INCIDENT)
        : structuredClone(MOCK_INCIDENTS[0]);
    return this.get<Incident>(`/incidents/${id}`, fallback);
  }

  analyzeRisk(shipmentId: string): Observable<RiskAnalysisResult> {
    const fallback =
      shipmentId === 'SHP-2048'
        ? structuredClone(MOCK_ANALYSIS)
        : {
            ...structuredClone(MOCK_ANALYSIS),
            shipmentId,
            riskScore: 54,
            severity: 'MEDIUM',
            projectedShortageUnits: 0,
            estimatedFinancialImpact: 0,
            predictedStockout: false,
            confidence: 76
          };

    return this.http
      .post<RiskAnalysisResult>(`${this.base}/risk/analyze`, { shipmentId })
      .pipe(
        tap(() => this.usingMocks.set(false)),
        catchError(() => {
          this.usingMocks.set(true);
          return of(fallback);
        })
      );
  }

  executeContingency(incidentId: string): Observable<ExecuteContingencyResult> {
    const fallback =
      incidentId === 'INC-2048'
        ? structuredClone(MOCK_EXECUTE)
        : {
            ...structuredClone(MOCK_EXECUTE),
            incidentId
          };

    return this.http
      .post<ExecuteContingencyResult>(`${this.base}/incidents/${incidentId}/execute`, {})
      .pipe(
        tap(() => this.usingMocks.set(false)),
        catchError(() => {
          this.usingMocks.set(true);
          return of(fallback);
        })
      );
  }

  sendEmail(request: SendEmailRequest): Observable<SendEmailResult> {
    return this.http
      .post<SendEmailResult>(`${this.base}/notifications/email`, request)
      .pipe(
        tap(() => this.usingMocks.set(false)),
        catchError(() => {
          this.usingMocks.set(true);
          return of({
            ...structuredClone(MOCK_EMAIL),
            recipient: request.recipient ?? MOCK_EMAIL.recipient,
            subject: request.subject ?? MOCK_EMAIL.subject
          });
        })
      );
  }

  private get<T>(path: string, fallback: T): Observable<T> {
    return this.http.get<T>(`${this.base}${path}`).pipe(
      tap(() => this.usingMocks.set(false)),
      catchError(() => {
        this.usingMocks.set(true);
        return of(fallback);
      })
    );
  }
}
