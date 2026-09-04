import { Routes } from '@angular/router';
import { ShellComponent } from './layout/shell/shell.component';

export const routes: Routes = [
  {
    path: '',
    component: ShellComponent,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'mission-control' },
      {
        path: 'mission-control',
        loadComponent: () =>
          import('./pages/mission-control/mission-control.component').then(
            (m) => m.MissionControlComponent
          )
      },
      {
        path: 'shipments',
        loadComponent: () =>
          import('./pages/shipments/shipments.component').then((m) => m.ShipmentsComponent)
      },
      {
        path: 'inventory',
        loadComponent: () =>
          import('./pages/inventory/inventory.component').then((m) => m.InventoryComponent)
      },
      {
        path: 'risk-intelligence',
        loadComponent: () =>
          import('./pages/risk-intelligence/risk-intelligence.component').then(
            (m) => m.RiskIntelligenceComponent
          )
      },
      {
        path: 'incidents',
        loadComponent: () =>
          import('./pages/incidents/incidents.component').then((m) => m.IncidentsComponent)
      },
      {
        path: 'automations',
        loadComponent: () =>
          import('./pages/automations/automations.component').then((m) => m.AutomationsComponent)
      }
    ]
  },
  { path: '**', redirectTo: 'mission-control' }
];
