import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { Shipment } from '../../models/api.models';
import { ApiService } from '../../services/api.service';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-shipments',
  standalone: true,
  imports: [DecimalPipe, CurrencyPipe, DatePipe, StatusBadgeComponent],
  templateUrl: './shipments.component.html',
  styleUrl: './shipments.component.scss'
})
export class ShipmentsComponent implements OnInit {
  private readonly api = inject(ApiService);
  shipments: Shipment[] = [];

  ngOnInit(): void {
    this.api.getShipments().subscribe((data) => (this.shipments = data));
  }
}
