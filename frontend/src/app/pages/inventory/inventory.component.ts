import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { InventoryItem } from '../../models/api.models';
import { ApiService } from '../../services/api.service';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [DecimalPipe, StatusBadgeComponent],
  templateUrl: './inventory.component.html',
  styleUrl: './inventory.component.scss'
})
export class InventoryComponent implements OnInit {
  private readonly api = inject(ApiService);
  items: InventoryItem[] = [];

  ngOnInit(): void {
    this.api.getInventory().subscribe((data) => (this.items = data));
  }
}
