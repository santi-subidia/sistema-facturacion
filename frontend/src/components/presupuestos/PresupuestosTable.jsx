import React from 'react'

const ESTADO_BADGES = {
  1: { label: 'Borrador', className: 'bg-gray-100 text-gray-800' },
  2: { label: 'Enviado', className: 'bg-blue-100 text-blue-800' },
  3: { label: 'Aceptado', className: 'bg-green-100 text-green-800' },
  4: { label: 'Rechazado', className: 'bg-red-100 text-red-800' },
  5: { label: 'Venta en Negro', className: 'bg-yellow-100 text-yellow-800' },
  6: { label: 'Facturado', className: 'bg-indigo-100 text-indigo-800' },
  7: { label: 'Vencido', className: 'bg-orange-100 text-orange-800' },
  8: { label: 'Cancelado', className: 'bg-red-100 text-red-700' },
}

function PresupuestosTable({ presupuestos, loading, error, onVerDetalle, onFacturar }) {
  if (loading) {
    return (
      <div className="flex justify-center items-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="p-4 bg-red-50 border border-red-200 rounded-md">
        <p className="text-red-800 text-sm">{error}</p>
      </div>
    )
  }

  if (presupuestos.length === 0) {
    return (
      <div className="text-center py-12">
        <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
        </svg>
        <h3 className="mt-2 text-sm font-medium text-gray-900">No hay presupuestos</h3>
        <p className="mt-1 text-sm text-gray-500">Comienza creando un nuevo presupuesto.</p>
      </div>
    )
  }

  const formatDate = (dateString) => {
    const date = new Date(dateString)
    return date.toLocaleDateString('es-AR', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit'
    })
  }

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('es-AR', {
      style: 'currency',
      currency: 'ARS'
    }).format(amount)
  }

  const getEstadoBadge = (estadoId) => {
    const badge = ESTADO_BADGES[estadoId] || { label: 'Desconocido', className: 'bg-gray-100 text-gray-800' }
    return (
      <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${badge.className}`}>
        {badge.label}
      </span>
    )
  }

  const puedeFacturar = (presupuesto) => {
    return presupuesto.idPresupuestoEstado === 3 && !presupuesto.idComprobanteGenerado
  }

  return (
    <div className="overflow-x-auto">
      <table className="min-w-full divide-y divide-gray-200">
        <thead className="bg-gray-50">
          <tr>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              N° Presupuesto
            </th>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              Fecha
            </th>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              Cliente
            </th>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              Forma de Pago
            </th>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              Total
            </th>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              Estado
            </th>
            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
              Acciones
            </th>
          </tr>
        </thead>
        <tbody className="bg-white divide-y divide-gray-200">
          {presupuestos.map((presupuesto) => (
            <tr key={presupuesto.id} className="hover:bg-gray-50 transition-colors">
              <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                #{presupuesto.numeroPresupuesto?.toString().padStart(6, '0') || presupuesto.id.toString().padStart(6, '0')}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                {formatDate(presupuesto.fecha)}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                {presupuesto.cliente ? (
                  `${presupuesto.cliente.nombre} ${presupuesto.cliente.apellido}`
                ) : presupuesto.clienteNombre ? (
                  `${presupuesto.clienteNombre} ${presupuesto.clienteApellido || ''}`
                ) : (
                  <span className="text-gray-500 italic">Consumidor Final</span>
                )}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                {presupuesto.formaPago?.nombre || 'N/A'}
                {presupuesto.porcentajeAjuste > 0 && (
                  <span className="ml-1 text-xs text-green-600">
                    (+{presupuesto.porcentajeAjuste}%)
                  </span>
                )}
                {presupuesto.porcentajeAjuste < 0 && (
                  <span className="ml-1 text-xs text-red-600">
                    ({presupuesto.porcentajeAjuste}%)
                  </span>
                )}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm font-semibold text-gray-900">
                {formatCurrency(presupuesto.total)}
              </td>
              <td className="px-6 py-4 whitespace-nowrap">
                {getEstadoBadge(presupuesto.idPresupuestoEstado)}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                <div className="flex items-center gap-2">
                  <button
                    onClick={() => onVerDetalle(presupuesto)}
                    className="inline-flex items-center px-3 py-1.5 border border-transparent text-xs font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition-colors"
                  >
                    <svg className="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                    </svg>
                    Ver Detalle
                  </button>
                  {puedeFacturar(presupuesto) && onFacturar && (
                    <button
                      onClick={() => onFacturar(presupuesto)}
                      className="inline-flex items-center px-3 py-1.5 border border-transparent text-xs font-medium rounded-md text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500 transition-colors"
                    >
                      <svg className="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                      </svg>
                      Facturar
                    </button>
                  )}
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

export default PresupuestosTable
