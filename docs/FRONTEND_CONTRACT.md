# Contrato API — Angular (Juan José)

Base URL: `http://localhost:5000/api`

```ts
export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:5000/api'
};
```

CORS abierto. JSON camelCase. Enums como string.

## Flujo demo (botones)

1. `GET /dashboard`
2. `GET /shipments/SHP-2048`
3. `GET /fx/trm`
4. `POST /orders/from-email` → body ejemplo abajo
5. `POST /orders/{id}/process`
6. `POST /risk/analyze` → `{ "shipmentId": "SHP-2048" }`
7. `POST /incidents/INC-2048/execute`
8. (opcional) `POST /demo/reset` antes de repetir

### from-email

```json
{
  "from": "compras@cliente.com",
  "subject": "PO-88001 Essential Rice Quantity: 12000 Destination: Barranquilla",
  "body": "Product: Essential Rice\nQuantity: 12000\nDestination: Barranquilla"
}
```

### Números del pitch (SHP-2048)

- Risk: 87 / CRITICAL  
- Delay: +6 days  
- Coverage: 7.1 days  
- Shortage: 13,200  
- Impact: $84,600 USD + COP vía TRM  

IDs quemados: `SHP-2048`, `INC-2048`, `ORD-3000`, `MAIL-001`.
