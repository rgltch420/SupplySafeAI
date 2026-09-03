import { Component, inject } from '@angular/core';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-automations',
  standalone: true,
  templateUrl: './automations.component.html',
  styleUrl: './automations.component.scss'
})
export class AutomationsComponent {
  private readonly api = inject(ApiService);
  sending = false;
  lastMessage = '';

  sendDemoEmail(): void {
    this.sending = true;
    this.api
      .sendEmail({
        recipient: 'operations@supplysafe.demo',
        subject: 'SUPPLYSAFE - Critical Supply Chain Risk',
        body: 'Demo alert: Essential Rice SHP-2048 contingency requires Procurement + Operations review.',
        incidentId: 'INC-2048'
      })
      .subscribe({
        next: (res) => {
          this.sending = false;
          this.lastMessage = `${res.success ? 'Sent' : 'Failed'} → ${res.recipient} · ${res.subject} · ${res.messageId}`;
        },
        error: () => {
          this.sending = false;
          this.lastMessage = 'Email request failed (mock fallback unavailable).';
        }
      });
  }
}
