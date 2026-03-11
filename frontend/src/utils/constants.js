// Colores por nombre de estado (se matchean contra los datos del endpoint /api/presupuesto/estados)
export const ESTADOS_PRESUPUESTO_COLORS = {
  "Borrador": "bg-gray-100 text-gray-800",
  "Enviado": "bg-blue-100 text-blue-800",
  "Aceptado": "bg-green-100 text-green-800",
  "Rechazado": "bg-red-100 text-red-800",
  "Venta en Negro": "bg-yellow-100 text-yellow-800",
  "Facturado": "bg-indigo-100 text-indigo-800",
  "Vencido": "bg-orange-100 text-orange-800",
  "Cancelado": "bg-red-100 text-red-700",
};

// Color por defecto para estados no mapeados
export const DEFAULT_ESTADO_COLOR = "bg-gray-100 text-gray-800";
