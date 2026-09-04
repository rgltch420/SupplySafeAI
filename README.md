# SupplySafe AI

Don't wait for the supply chain to break. Act before it does.

Plataforma de Supply Chain Resilience: DETECT → PREDICT → ANALYZE IMPACT → RECOMMEND → ACT → NOTIFY

## Estructura

```
SupplySafeAI/
├── backend/
│   ├── SupplySafe.sln
│   └── SupplySafe.Api/     # ASP.NET Core (.NET 10) REST API
└── frontend/               # Angular (Juan José)
```

## Backend — cómo correrlo

```bash
cd backend
dotnet run --project SupplySafe.Api
```

O desde la carpeta del proyecto:

```bash
cd backend/SupplySafe.Api
dotnet run
```

- API base: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`

Si ves `address already in use` en el puerto 5000, ya hay otra instancia corriendo. Úsala o ciérrala:

```bash
# ver quién usa el puerto
ss -tlnp | grep 5000
# cerrar ese proceso (reemplaza PID)
kill <PID>
```

Grok (opcional):

```bash
export XAI_API_KEY=tu_key
dotnet run --project SupplySafe.Api
```

Sin key, la API usa fallback local (la demo no se cae).

## Angular — cómo conectar (contrato)

CORS está abierto (`AllowAnyOrigin` / `AllowAnyHeader` / `AllowAnyMethod`) para el hackathon.

En Angular (`environment.ts` / `environment.development.ts`):

```ts
export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:5000/api'
};
```

Ejemplo de servicio:

```ts
this.http.get(`${environment.apiBaseUrl}/dashboard`);
this.http.get(`${environment.apiBaseUrl}/shipments/SHP-2048`);
this.http.post(`${environment.apiBaseUrl}/risk/analyze`, { shipmentId: 'SHP-2048' });
this.http.post(`${environment.apiBaseUrl}/incidents/INC-2048/execute`, {});
```

JSON en **camelCase**. Enums como string (`"Critical"`, `"Delayed"`, etc.).

### Endpoints principales

| Método | Ruta | Uso en demo |
|--------|------|-------------|
| GET | `/api/dashboard` | KPIs |
| GET | `/api/shipments` | Lista |
| GET | `/api/shipments/{id}` | Detalle SHP-2048 |
| GET | `/api/inventory` | Inventario |
| GET | `/api/risks` | Riesgos |
| POST | `/api/risk/analyze` | Análisis IA/fallback |
| GET/POST | `/api/incidents` | Incidentes |
| GET | `/api/incidents/{id}` | INC-2048 |
| POST | `/api/incidents/{id}/execute` | Activar contingencia |
| POST | `/api/notifications/email` | Email simulado |
| GET | `/api/mailbox` | Bandeja corporativa simulada |
| GET | `/api/orders` | Órdenes de trabajo |
| GET | `/api/orders/{id}` | Detalle de orden |
| POST | `/api/orders/from-email` | Simula llegada de orden por correo |
| POST | `/api/orders/{id}/process` | Procesa orden (link → risk → notify) |
| GET | `/api/fx/trm` | TRM USD/COP + refs EUR/CNY |

Demo seed: shipment `SHP-2048`, incidente `INC-2048`, orden `ORD-3000`, mail `MAIL-001`.

### Flujo empresa (correo → orden → riesgo)

```bash
# 1) Ver bandeja quemada
curl http://localhost:5000/api/mailbox

# 2) Simular que llega un correo de compra
curl -X POST http://localhost:5000/api/orders/from-email \
  -H "Content-Type: application/json" \
  -d '{"from":"compras@cliente.com","subject":"PO-88001 Essential Rice Quantity: 12000 Destination: Barranquilla","body":"Urgent replenishment Essential Rice"}'

# 3) Procesar la orden (reemplaza ORD-xxxx)
curl -X POST http://localhost:5000/api/orders/ORD-3001/process

# Notificaciones: export SUPPLYSAFE_NOTIFY_EMAIL=tu@correo.com
# SMTP real (opcional): Smtp__Host, Smtp__Username, Smtp__Password
```

## Frontend — Angular (Juan José)

```bash
cd frontend
npm install
ng serve
```

Abre http://localhost:4200

