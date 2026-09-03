import { DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { SupplyRisk } from '../../models/api.models';
import { ApiService } from '../../services/api.service';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-risk-intelligence',
  standalone: true,
  imports: [DatePipe, StatusBadgeComponent],
  templateUrl: './risk-intelligence.component.html',
  styleUrl: './risk-intelligence.component.scss'
})
export class RiskIntelligenceComponent implements OnInit {
  private readonly api = inject(ApiService);
  risks: SupplyRisk[] = [];

  ngOnInit(): void {
    this.api.getRisks().subscribe((data) => (this.risks = data));
  }
}
