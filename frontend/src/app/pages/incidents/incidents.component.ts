import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { Incident } from '../../models/api.models';
import { ApiService } from '../../services/api.service';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-incidents',
  standalone: true,
  imports: [DecimalPipe, CurrencyPipe, DatePipe, StatusBadgeComponent],
  templateUrl: './incidents.component.html',
  styleUrl: './incidents.component.scss'
})
export class IncidentsComponent implements OnInit {
  private readonly api = inject(ApiService);
  incidents: Incident[] = [];

  ngOnInit(): void {
    this.api.getIncidents().subscribe((data) => (this.incidents = data));
  }
}
