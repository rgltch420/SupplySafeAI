/** Display-only Spanish labels. API values stay in English (contract unchanged). */

const STATUS: Record<string, string> = {
  InTransit: 'En tránsito',
  Delayed: 'Retrasado',
  AtRisk: 'En riesgo',
  Diverted: 'Desviado',
  Delivered: 'Entregado',
  Healthy: 'Sano',
  Watch: 'Vigilancia',
  Critical: 'Crítico',
  Low: 'Bajo',
  Medium: 'Medio',
  High: 'Alto',
  OPEN: 'Abierto',
  ANALYZING: 'Analizando',
  CONTINGENCY_ACTIVATED: 'Contingencia activada',
  RESOLVED: 'Resuelto',
  CRITICAL: 'Crítico',
  MEDIUM: 'Medio',
  HIGH: 'Alto',
  LOW: 'Bajo',
  Weather: 'Clima',
  PortCongestion: 'Congestión portuaria',
  Geopolitical: 'Geopolítico',
  Inventory: 'Inventario',
  Shipment: 'Envío',
  SupplyChain: 'Cadena de suministro'
};

const PHRASES: Record<string, string> = {
  'Divert shipment to alternate route': 'Desviar envío a ruta alternativa',
  'Activate secondary supplier': 'Activar proveedor secundario',
  'Prioritize existing inventory': 'Priorizar inventario existente',
  'Notify Operations': 'Notificar a Operaciones',
  'Notify Procurement': 'Notificar a Compras',
  'Alternative route activated': 'Ruta alternativa activada',
  'Secondary supplier activated': 'Proveedor secundario activado',
  'Inventory prioritized': 'Inventario priorizado',
  'Operations notified': 'Operaciones notificadas',
  'Procurement notified': 'Compras notificadas',
  'Severe tropical storm — Caribbean corridor':
    'Tormenta tropical severa — corredor del Caribe',
  'Cartagena port congestion elevated': 'Congestión elevada en puerto de Cartagena',
  'Trade corridor advisory — Panama transit':
    'Aviso de corredor comercial — tránsito por Panamá',
  'Critical delay — Essential Rice SHP-2048 (Shanghai → Barranquilla)':
    'Retraso crítico — Arroz esencial SHP-2048 (Shanghái → Barranquilla)',
  'Essential Rice': 'Arroz esencial',
  'Medical Supplies Kit': 'Kit de insumos médicos',
  'Industrial Lubricants': 'Lubricantes industriales',
  'Coffee Packaging Film': 'Película para empaque de café',
  'Semiconductor Components': 'Componentes semiconductores'
};

const DESCRIPTIONS: Record<string, string> = {
  'Severe weather event impacting Shanghai→Cartagena→Barranquilla corridor. Port operations degraded; vessel ETA slipped +6 days for Essential Rice (SHP-2048).':
    'Evento climático severo en el corredor Shanghái→Cartagena→Barranquilla. Operaciones portuarias degradadas; ETA del buque +6 días para Arroz esencial (SHP-2048).',
  'Berth wait times above 48h. Cascading delays for Asia–Caribbean feeders; SHP-2048 and SHP-1655 partially exposed.':
    'Tiempos de atraque >48h. Retrasos en cascada en feeders Asia–Caribe; SHP-2048 y SHP-1655 parcialmente expuestos.',
  'Heightened inspection regime on Panama transit lanes. Medium impact on Shenzhen→Bogotá semiconductor lane.':
    'Régimen de inspección reforzado en tránsito por Panamá. Impacto medio en la ruta Shenzhen→Bogotá de semiconductores.'
};

/** Soften English fragments that arrive from live API routes after execute. */
function polishRoute(value: string): string {
  return value
    .replaceAll('Alternate Hub', 'Hub alterno')
    .replaceAll('Shanghai', 'Shanghái');
}

export function esLabel(value: string | null | undefined): string {
  if (!value) return '';
  const mapped = STATUS[value] ?? PHRASES[value] ?? DESCRIPTIONS[value];
  return mapped ?? polishRoute(value);
}

export function esPhrase(value: string | null | undefined): string {
  if (!value) return '';
  const mapped = PHRASES[value] ?? DESCRIPTIONS[value] ?? STATUS[value];
  return mapped ?? polishRoute(value);
}
