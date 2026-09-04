#!/usr/bin/env bash
# SupplySafe AI — demo script (pitch-ready)
# Usage: ./scripts/demo.sh   (API must be running on :5000)
set -euo pipefail
BASE="${SUPPLYSAFE_BASE:-http://localhost:5000}"

echo "== 1) Dashboard =="
curl -s "$BASE/api/dashboard" | python -m json.tool

echo "== 2) Critical shipment SHP-2048 =="
curl -s "$BASE/api/shipments/SHP-2048" | python -m json.tool

echo "== 3) TRM FX =="
curl -s "$BASE/api/fx/trm" | python -m json.tool

echo "== 4) Ingest PO email =="
ORDER=$(curl -s -X POST "$BASE/api/orders/from-email" \
  -H "Content-Type: application/json" \
  -d '{"from":"compras@cliente.com","subject":"PO-88001 Essential Rice Quantity: 12000 Destination: Barranquilla","body":"Product: Essential Rice\nQuantity: 12000\nDestination: Barranquilla"}')
echo "$ORDER" | python -m json.tool
OID=$(echo "$ORDER" | python -c "import sys,json; print(json.load(sys.stdin)['id'])")

echo "== 5) Process order $OID =="
curl -s -X POST "$BASE/api/orders/$OID/process" | python -m json.tool

echo "== 6) Risk analyze SHP-2048 =="
curl -s -X POST "$BASE/api/risk/analyze" \
  -H "Content-Type: application/json" \
  -d '{"shipmentId":"SHP-2048"}' | python -m json.tool

echo "== 7) Execute contingency INC-2048 =="
curl -s -X POST "$BASE/api/incidents/INC-2048/execute" | python -m json.tool

echo "DONE — check Gmail if SUPPLYSAFE_SMTP_PASSWORD is set."
