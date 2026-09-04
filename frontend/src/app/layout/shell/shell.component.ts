import { AsyncPipe, NgClass } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter, map, startWith } from 'rxjs';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, AsyncPipe, NgClass],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.scss'
})
export class ShellComponent implements OnInit {
  private readonly router = inject(Router);
  readonly api = inject(ApiService);

  readonly nav = [
    { path: '/mission-control', label: 'Mission Control', hint: 'Live threat board' },
    { path: '/shipments', label: 'Shipments', hint: 'Active cargo' },
    { path: '/inventory', label: 'Inventory', hint: 'Coverage & stock' },
    { path: '/risk-intelligence', label: 'Risk Intelligence', hint: 'Signals' },
    { path: '/incidents', label: 'Incidents', hint: 'Open cases' },
    { path: '/automations', label: 'Automations', hint: 'Notify & act' }
  ];

  readonly title$ = this.router.events.pipe(
    filter((e): e is NavigationEnd => e instanceof NavigationEnd),
    map(() => this.router.url),
    startWith(this.router.url),
    map((url) => this.nav.find((n) => url.startsWith(n.path))?.label ?? 'Mission Control')
  );

  ngOnInit(): void {
    this.api.ping().subscribe();
  }
}

