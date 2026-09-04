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
        subject: 'SUPPLYSAFE - Riesgo crítico en cadena de suministro',
        body: 'Alerta demo: contingencia de Arroz esencial SHP-2048 requiere revisión de Compras + Operaciones.',
        incidentId: 'INC-2048'
      })
      .subscribe({
        next: (res) => {
          this.sending = false;
          this.lastMessage = `${res.success ? 'Enviado' : 'Falló'} → ${res.recipient} · ${res.subject} · ${res.messageId}`;
        },
        error: () => {
          this.sending = false;
          this.lastMessage = 'Falló la solicitud de correo (sin mock disponible).';
        }
      });
  }
}
