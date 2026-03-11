import React from 'react'
import { useNavigate } from 'react-router-dom'
import { toCurrency, toDate } from '../../utils/formatters' // Assuming these exist, or I will use the local functions if not present. The original code defined them locally. I will keep local for now to minimize risk.

function ComprobantesTable({ comprobantes, loading, error, onVerDetalle }) {
  const navigate = useNavigate()
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

  if (!comprobantes || comprobantes.length === 0) {
    return (
      <div className="text-center py-12">
        <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
        </svg>
        <h3 className="mt-2 text-sm font-medium text-gray-900">No hay facturas</h3>
        <p className="mt-1 text-sm text-gray-500">Comienza creando una nueva factura.</p>
      </div>
    )
  }

  const formatDate = (dateString) => {
    if (!dateString) return '-'
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

  return (
    <div className="overflow-x-auto">
      <table className="min-w-full divide-y divide-slate-200">
        <thead className="bg-slate-50">
          <tr>
            <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase tracking-wider">
              N° Factura
            </th>
            <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase tracking-wider">
              Fecha
            </th>
            <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase tracking-wider">
              Cliente
            </th>
            <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase tracking-wider">
              Tipo
            </th>
            <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase tracking-wider">
              Estado
            </th>
            <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase tracking-wider">
              Forma de Pago
            </th>
            <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase tracking-wider">
              Total
            </th>
            <th className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase tracking-wider">
              Acciones
            </th>
          </tr>
        </thead>
        <tbody className="bg-white divide-y divide-slate-200">
          {comprobantes.map((comprobante) => (
            <tr key={comprobante.id} className="hover:bg-blue-50/50 transition-colors">
              <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-slate-900">
                #{comprobante.numeroComprobante.toString().padStart(6, '0')}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm text-slate-900">
                {formatDate(comprobante.fecha)}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm text-slate-900">
                {comprobante.cliente ? (
                  `${comprobante.cliente.nombre} ${comprobante.cliente.apellido}`
                ) : comprobante.clienteNombre ? (
                  `${comprobante.clienteNombre} ${comprobante.clienteApellido || ''}`
                ) : (
                  <span className="text-slate-500 italic">Consumidor Final</span>
                )}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm text-slate-900">
                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                  {comprobante.tipoComprobante?.nombre || 'N/A'}
                </span>
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm text-slate-900">
                {comprobante.estadoComprobante ? (
                  <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${comprobante.estadoComprobante.nombre === 'Anulada' ? 'bg-red-100 text-red-800' :
                    comprobante.estadoComprobante.nombre === 'Parcialmente Anulada' ? 'bg-amber-100 text-amber-800' :
                      'bg-emerald-100 text-emerald-800'
                    }`}>
                    {comprobante.estadoComprobante.nombre}
                  </span>
                ) : (
                  <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-emerald-100 text-emerald-800">
                    Vigente
                  </span>
                )}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm text-slate-900">
                {comprobante.formaPago?.nombre || 'N/A'}
                {comprobante.porcentajeAjuste > 0 && (
                  <span className="ml-1 text-xs text-green-600">
                    (+{comprobante.porcentajeAjuste}%)
                  </span>
                )}
                {comprobante.porcentajeAjuste < 0 && (
                  <span className="ml-1 text-xs text-red-600">
                    ({comprobante.porcentajeAjuste}%)
                  </span>
                )}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm font-semibold text-slate-900">
                {formatCurrency(comprobante.total)}
              </td>
              <td className="px-6 py-4 whitespace-nowrap text-sm font-medium flex space-x-2">
                <button
                  onClick={() => onVerDetalle(comprobante)}
                  className="inline-flex items-center px-3 py-1.5 border border-transparent text-xs font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
                >
                  <svg className="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                  </svg>
                  Ver Detalle
                </button>
                {comprobante.tipoComprobante?.nombre && !comprobante.tipoComprobante.nombre.toLowerCase().includes('nota') && comprobante.estadoComprobante?.nombre !== 'Anulada' && (
                  <button
                    onClick={() => navigate('/comprobantes/nota-credito', { state: { idFacturaOriginal: comprobante.id } })}
                    className="inline-flex items-center px-3 py-1.5 border border-transparent text-xs font-medium rounded-md text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 transition-colors"
                    title="Emitir Nota de Crédito para esta factura"
                  >
                    <svg className="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h10a8 8 0 018 8v2M3 10l6 6m-6-6l6-6" />
                    </svg>
                    Crear NC
                  </button>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

export default ComprobantesTable
