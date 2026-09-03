import { DecimalPipe, NgClass } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [NgClass, DecimalPipe],
  template: `
    <span class="badge" [ngClass]="tone">
      @if (value !== null && value !== undefined && showValue) {
        <span class="mono">{{ value | number: '1.0-0' }}</span>
      }
      {{ label }}
    </span>
  `,
  styles: `
    .badge {
      display: inline-flex;
      align-items: center;
      gap: 0.35rem;
      padding: 0.22rem 0.6rem;
      border: 1px solid var(--border);
      border-radius: 999px;
      font-size: 0.66rem;
      letter-spacing: 0.05em;
      text-transform: uppercase;
      font-weight: 600;
      background: rgba(0, 0, 0, 0.28);
      white-space: nowrap;
    }
    .mono {
      font-family: var(--font-mono);
      font-weight: 600;
    }
    .critical,
    .delayed,
    .open {
      color: #ffd0b5;
      border-color: rgba(243, 112, 33, 0.55);
      background: rgba(243, 112, 33, 0.18);
    }
    .high,
    .atrisk,
    .analyzing {
      color: var(--amber);
      border-color: rgba(245, 165, 36, 0.45);
      background: rgba(245, 165, 36, 0.12);
    }
    .medium,
    .watch {
      color: #fbbf24;
      border-color: rgba(251, 191, 36, 0.35);
    }
    .low,
    .healthy,
    .intransit,
    .resolved,
    .contingency_activated {
      color: var(--green);
      border-color: rgba(61, 214, 140, 0.4);
      background: rgba(61, 214, 140, 0.1);
    }
    .weather {
      color: var(--orange-hot);
      border-color: rgba(243, 112, 33, 0.45);
    }
  `
})
export class StatusBadgeComponent {
  @Input({ required: true }) label = '';
  @Input() value: number | null = null;
  @Input() showValue = false;

  get tone(): string {
    return this.label.replace(/\s+/g, '').toLowerCase();
  }
}
