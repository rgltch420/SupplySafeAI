import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import {
  ExecuteContingencyResult,
  RiskAnalysisResult,
  SendEmailResult
} from '../models/api.models';

export type DemoPhase =
  | 'idle'
  | 'analyzing'
  | 'analyzed'
  | 'executing'
  | 'executed';

@Injectable({ providedIn: 'root' })
export class DemoStateService {
  readonly phase$ = new BehaviorSubject<DemoPhase>('idle');
  readonly analysis$ = new BehaviorSubject<RiskAnalysisResult | null>(null);
  readonly executeResult$ = new BehaviorSubject<ExecuteContingencyResult | null>(null);
  readonly emailResult$ = new BehaviorSubject<SendEmailResult | null>(null);
  readonly tickedActions$ = new BehaviorSubject<string[]>([]);
  readonly lossAvoidedVisible$ = new BehaviorSubject(false);

  reset(): void {
    this.phase$.next('idle');
    this.analysis$.next(null);
    this.executeResult$.next(null);
    this.emailResult$.next(null);
    this.tickedActions$.next([]);
    this.lossAvoidedVisible$.next(false);
  }
}
