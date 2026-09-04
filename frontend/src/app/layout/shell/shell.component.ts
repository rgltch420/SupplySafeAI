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
    { path: '/mission-control', label: 'Centro de mando', hint: 'Tablero de amenazas' },
    { path: '/shipments', label: 'Envíos', hint: 'Carga activa' },
    { path: '/inventory', label: 'Inventario', hint: 'Cobertura y stock' },
    { path: '/risk-intelligence', label: 'Inteligencia de riesgo', hint: 'Señales' },
    { path: '/incidents', label: 'Incidentes', hint: 'Casos abiertos' },
    { path: '/automations', label: 'Automatizaciones', hint: 'Notificar y actuar' }
  ];

  readonly title$ = this.router.events.pipe(
    filter((e): e is NavigationEnd => e instanceof NavigationEnd),
    map(() => this.router.url),
    startWith(this.router.url),
    map((url) => this.nav.find((n) => url.startsWith(n.path))?.label ?? 'Centro de mando')
  );

  ngOnInit(): void {
    this.api.ping().subscribe();
  }
}
