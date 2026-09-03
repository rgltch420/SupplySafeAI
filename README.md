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

Demo seed: shipment `SHP-2048`, incidente `INC-2048`.
