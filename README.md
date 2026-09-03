# SupplySafe AI

Don't wait for the supply chain to break. Act before it does.

Plataforma de Supply Chain Resilience: DETECT → PREDICT → ANALYZE IMPACT → RECOMMEND → ACT → NOTIFY

## Estructura

```
SupplySafeAI/
├── backend/
│   ├── SupplySafe.sln
│   └── SupplySafe.Api/     # ASP.NET Core REST API
└── frontend/               # Angular 19 (standalone, SCSS)
```

## Backend

```bash
cd backend
dotnet run --project SupplySafe.Api
```

- API base: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`

## Frontend

```bash
cd frontend
npm install
ng serve
```

Abre `http://localhost:4200`.

- `environment.ts` → `apiBaseUrl: 'http://localhost:5000/api'`
- Si la API no responde, la UI usa mocks con el mismo seed (SHP-2048 / INC-2048) y no se rompe.
- El top bar muestra `MOCK MODE · API OFFLINE` cuando cae a mocks.

### Pitch demo (Mission Control)

1. Dashboard con KPIs y amenaza Essential Rice
2. **SIMULATE DISRUPTION** → `POST /risk/analyze` → Risk 87 / 13,200 units
3. **EXECUTE CONTINGENCY PLAN** → ticks → `$84,600 LOSS AVOIDED` → `CONTINGENCY_ACTIVATED`
4. Email Operations / Procurement · subject `SUPPLYSAFE - Critical Supply Chain Risk`

## Contrato API

JSON camelCase. Enums string (`Critical`, `Delayed`, `OPEN`, `CONTINGENCY_ACTIVATED`, `AtRisk`, `Weather`).

| Método | Ruta |
|--------|------|
| GET | `/api/dashboard` |
| GET | `/api/shipments` |
| GET | `/api/shipments/{id}` |
| GET | `/api/inventory` |
| GET | `/api/risks` |
| POST | `/api/risk/analyze` |
| GET | `/api/incidents` |
| GET | `/api/incidents/{id}` |
| POST | `/api/incidents/{id}/execute` |
| POST | `/api/notifications/email` |
