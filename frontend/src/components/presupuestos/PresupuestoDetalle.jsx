import React, { useEffect, useState } from 'react'
import { API_BASE_URL } from '../../config'
import { fetchWithAuth } from '../../utils/authHeaders'
import { ESTADOS_PRESUPUESTO_COLORS, DEFAULT_ESTADO_COLOR } from '../../utils/constants'
import { useConnectivity } from '../../hooks/useConnectivity'
import ConvertirAComprobanteModal from './ConvertirAComprobanteModal'

// Nombres de estados terminales que no pueden cambiar
const NOMBRES_ESTADOS_TERMINALES = ['Venta en Negro', 'Facturado']

function PresupuestoDetalle({ show, presupuesto, onClose, onFacturar, onCambiarEstado, estados = [] }) {
  const [detalles, setDetalles] = useState([])
  const [loading, setLoading] = useState(false)
  const [loadingPdf, setLoadingPdf] = useState(false)
  const [loadingEmail, setLoadingEmail] = useState(false)
  const [loadingEstado, setLoadingEstado] = useState(false)
  const [showPdfPreview, setShowPdfPreview] = useState(false)
  const [pdfUrl, setPdfUrl] = useState(null)
  const [showConvertirModal, setShowConvertirModal] = useState(false)
  const [presupuestoCompleto, setPresupuestoCompleto] = useState(null)
  const [estadoLocal, setEstadoLocal] = useState(null)
  const { isAfipOnline } = useConnectivity()

  useEffect(() => {
    if (show && presupuesto?.id) {
      fetchDetalles()
      setEstadoLocal(presupuesto.idPresupuestoEstado)
    }
  }, [show, presupuesto])

  const fetchDetalles = async () => {
    try {
      setLoading(true)
      // Fetch detalles
      const response = await fetchWithAuth(`${API_BASE_URL}/presupuesto/${presupuesto.id}/detalle`)

      if (response.ok) {
        const data = await response.json()
        setDetalles(data.detalles || [])
      }

      // Fetch datos completos del presupuesto (para contacto del cliente)
      const dataResponse = await fetchWithAuth(`${API_BASE_URL}/presupuesto/${presupuesto.id}`)
      if (dataResponse.ok) {
        const data = await dataResponse.json()
        setPresupuestoCompleto(data)
      }
    } catch (err) {
      console.error('Error al cargar detalles:', err)
    } finally {
      setLoading(false)
    }
  }

  const handleCambiarEstado = async (nuevoEstadoId) => {
    const nuevoEstado = parseInt(nuevoEstadoId)
    if (nuevoEstado === estadoLocal) return

    const estadoObj = estados.find(e => e.id === nuevoEstado)
    const estadoLabel = estadoObj?.nombre || 'Desconocido'
    if (!window.confirm(`¿Cambiar el estado del presupuesto a "${estadoLabel}"?`)) return

    setLoadingEstado(true)
    try {
      await onCambiarEstado(presupuesto.id, nuevoEstado)
      setEstadoLocal(nuevoEstado)
    } catch (err) {
      alert(`Error al cambiar estado: ${err.message}`)
    } finally {
      setLoadingEstado(false)
    }
  }

  const handleDownloadPdf = async () => {
    try {
      setLoadingPdf(true)
      const response = await fetchWithAuth(`${API_BASE_URL}/presupuesto/${presupuesto.id}/pdf`)

      if (!response.ok) {
        throw new Error('Error al generar el PDF')
      }

      const blob = await response.blob()
      const url = window.URL.createObjectURL(blob)
      setPdfUrl(url)
      setShowPdfPreview(true)
    } catch (err) {
      console.error('Error al cargar PDF:', err)
      alert('Error al generar el PDF. Por favor, intente nuevamente.')
    } finally {
      setLoadingPdf(false)
    }
  }

  const handleClosePdfPreview = () => {
    setShowPdfPreview(false)
    if (pdfUrl) {
      window.URL.revokeObjectURL(pdfUrl)
      setPdfUrl(null)
    }
  }

  const handleActualDownload = () => {
    if (pdfUrl) {
      const a = document.createElement('a')
      a.href = pdfUrl
      a.download = `Presupuesto_${presupuesto.id.toString().padStart(6, '0')}.pdf`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
    }
  }

  const getClientContact = () => {
    const tel = presupuestoCompleto?.cliente?.telefono || presupuesto?.cliente?.telefono
    const email = presupuestoCompleto?.cliente?.correo || presupuesto?.cliente?.correo
    return { tel, email }
  }

  const handleWhatsApp = () => {
    const { tel } = getClientContact()
    if (!tel) {
      alert('Este presupuesto no tiene un número de teléfono asociado al cliente.')
      return
    }

    const cleanPhone = tel.replace(/\D/g, '')
    const message = encodeURIComponent(`Hola, te envío adjunto el Presupuesto #${presupuesto?.id}. Saludos.`)
    window.open(`https://wa.me/${cleanPhone}?text=${message}`, '_blank')
  }

  const handleEmail = async () => {
    const { email } = getClientContact()
    if (!email) {
      alert('Este presupuesto no tiene un correo electrónico asociado al cliente para enviarlo automáticamente.')
      return
    }

    if (!window.confirm(`¿Enviar el presupuesto automáticamente a ${email}?`)) {
      return
    }

    setLoadingEmail(true)
    try {
      const id = presupuesto.id
      const response = await fetchWithAuth(`${API_BASE_URL}/presupuesto/${id}/enviar-correo`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ email })
      })

      const data = await response.json()

      if (response.ok) {
        alert('Correo enviado exitosamente!')
      } else {
        alert(`Error al enviar el correo: ${data.message || 'Error desconocido'}`)
      }
    } catch (err) {
      console.error('Error sending email:', err)
      alert('Error de red al intentar enviar el correo. Por favor, revisa la consola.')
    } finally {
      setLoadingEmail(false)
    }
  }

  if (!show || !presupuesto) return null

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

  const calcularTotal = () => {
    return detalles.reduce((total, detalle) => total + (detalle.subtotal || 0), 0)
  }

  const estadoActual = estadoLocal || presupuesto.idPresupuestoEstado
  const estadoActualObj = estados.find(e => e.id === estadoActual)
  const esEstadoTerminal = estadoActualObj ? NOMBRES_ESTADOS_TERMINALES.includes(estadoActualObj.nombre) : false
  const esAceptado = estadoActualObj?.nombre === 'Aceptado'

  return (
    <div className="fixed inset-0 z-50 overflow-hidden" aria-labelledby="modal-title" role="dialog" aria-modal="true">
      {/* Background overlay */}
      <div className="absolute inset-0 bg-slate-900/75 transition-opacity" aria-hidden="true" onClick={onClose}></div>

      {/* Modal Container */}
      <div className="absolute inset-0 flex items-center justify-center p-4 pointer-events-none">
        <div className="w-full max-w-4xl max-h-[95vh] bg-white rounded-xl shadow-2xl flex flex-col pointer-events-auto overflow-hidden">

          {/* Header */}
          <div className="bg-gradient-to-r from-slate-800 to-slate-900 px-6 py-4 flex justify-between items-center shrink-0">
            <div className="flex items-center gap-3">
              <div className="bg-white/10 p-2 rounded-lg">
                <svg className="w-6 h-6 text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
              </div>
              <h3 className="text-xl font-bold text-white">
                Presupuesto #{presupuesto.numeroPresupuesto?.toString().padStart(6, '0') || presupuesto.id.toString().padStart(6, '0')}
              </h3>
              {estadoActual && (() => {
                const estadoInfo = estados.find(e => e.id === estadoActual)
                const nombre = estadoInfo?.nombre || 'Desconocido'
                const colorClass = ESTADOS_PRESUPUESTO_COLORS[nombre] || DEFAULT_ESTADO_COLOR
                return (
                  <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${colorClass} bg-opacity-90 border border-white/20 ml-2`}>
                    {nombre}
                  </span>
                )
              })()}
            </div>
            <button
              onClick={onClose}
              className="text-slate-300 hover:text-white p-2 hover:bg-white/10 rounded-lg transition-colors focus:outline-none"
            >
              <svg className="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          {/* Content Body */}
          <div className="overflow-y-auto flex-1 p-6 bg-slate-50/50">

            {/* Cambiar Estado */}
            {onCambiarEstado && !esEstadoTerminal && (
              <div className="mt-4 bg-gray-50 border border-gray-200 rounded-lg p-3">
                <div className="flex items-center gap-3">
                  <label className="text-sm font-medium text-gray-700 whitespace-nowrap">Cambiar estado:</label>
                  <select
                    value={estadoActual}
                    onChange={(e) => handleCambiarEstado(e.target.value)}
                    disabled={loadingEstado}
                    className="block w-full max-w-xs rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 text-sm disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {estados
                      .filter((e) => !NOMBRES_ESTADOS_TERMINALES.includes(e.nombre))
                      .map((e) => (
                        <option key={e.id} value={e.id}>{e.nombre}</option>
                      ))
                    }
                  </select>
                  {loadingEstado && (
                    <svg className="animate-spin h-5 w-5 text-indigo-600" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                  )}
                </div>
              </div>
            )}

            {/* Información del Presupuesto */}
            <div className="mt-4 grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <h4 className="text-sm font-medium text-gray-500">Fecha</h4>
                <p className="mt-1 text-sm text-gray-900">{formatDate(presupuesto.fecha)}</p>
              </div>

              <div>
                <h4 className="text-sm font-medium text-gray-500">Fecha de Vencimiento</h4>
                <p className="mt-1 text-sm text-gray-900">{formatDate(presupuesto.fechaVencimiento)}</p>
              </div>

              <div>
                <h4 className="text-sm font-medium text-gray-500">Cliente</h4>
                <p className="mt-1 text-sm text-gray-900">
                  {presupuesto.cliente ? (
                    `${presupuesto.cliente.nombre} ${presupuesto.cliente.apellido}`
                  ) : presupuesto.clienteNombre ? (
                    `${presupuesto.clienteNombre} ${presupuesto.clienteApellido || ''}`
                  ) : (
                    <span className="text-gray-500 italic">Consumidor Final</span>
                  )}
                </p>
              </div>

              <div>
                <h4 className="text-sm font-medium text-gray-500">Forma de Pago</h4>
                <p className="mt-1 text-sm text-gray-900">
                  {presupuesto.formaPago?.nombre || 'N/A'}
                  {presupuesto.porcentajeAjuste !== 0 && (
                    <span className="ml-2 text-xs text-gray-600">
                      ({presupuesto.porcentajeAjuste > 0 ? '+' : ''}{presupuesto.porcentajeAjuste}%)
                    </span>
                  )}
                </p>
              </div>

              <div>
                <h4 className="text-sm font-medium text-gray-500">Condición de Venta</h4>
                <p className="mt-1 text-sm text-gray-900">
                  {presupuesto.condicionVenta?.descripcion || 'N/A'}
                </p>
              </div>
            </div>

            {/* Detalles de Productos */}
            <div className="mt-6">
              <h4 className="text-lg font-medium text-gray-900 mb-3">Productos</h4>

              {loading ? (
                <div className="flex justify-center py-8">
                  <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
                </div>
              ) : (
                <div className="overflow-x-auto">
                  <table className="min-w-full divide-y divide-gray-200">
                    <thead className="bg-gray-50">
                      <tr>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Producto</th>
                        <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Código</th>
                        <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Cantidad</th>
                        <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Precio Unit.</th>
                        <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Subtotal</th>
                      </tr>
                    </thead>
                    <tbody className="bg-white divide-y divide-gray-200">
                      {detalles.map((detalle, index) => (
                        <tr key={index}>
                          <td className="px-6 py-4 text-sm text-gray-900">
                            {detalle.producto?.nombre || detalle.productoNombre || 'N/A'}
                          </td>
                          <td className="px-6 py-4 text-sm text-gray-500">
                            {detalle.producto?.codigo || detalle.productoCodigo || '-'}
                          </td>
                          <td className="px-6 py-4 text-sm text-gray-900 text-right">
                            {detalle.cantidad}
                          </td>
                          <td className="px-6 py-4 text-sm text-gray-900 text-right">
                            {formatCurrency(detalle.precioUnitario || detalle.precio)}
                          </td>
                          <td className="px-6 py-4 text-sm font-medium text-gray-900 text-right">
                            {formatCurrency(detalle.subtotal || (detalle.cantidad * detalle.precio))}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>

            {/* Total */}
            <div className="mt-6 border-t border-gray-200 pt-4">
              <div className="flex justify-end">
                <div className="w-64">
                  <div className="flex justify-between text-lg font-bold">
                    <span className="text-gray-900">TOTAL:</span>
                    <span className="text-gray-900">{formatCurrency(presupuesto.total || calcularTotal())}</span>
                  </div>
                </div>
              </div>
            </div>

            {/* Comprobante asociado */}
            {presupuesto.idComprobanteGenerado && (
              <div className="mt-4 bg-indigo-50 border border-indigo-200 rounded-lg p-3">
                <div className="flex items-center gap-2">
                  <svg className="w-5 h-5 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <p className="text-sm text-indigo-700">
                    <span className="font-medium">Comprobante generado:</span> ID #{presupuesto.idComprobanteGenerado}
                  </p>
                </div>
              </div>
            )}

          </div>

          {/* Footer */}
          <div className="bg-white border-t border-slate-200 px-6 py-4 shrink-0 flex justify-end gap-3">
            {/* Botón Facturar - solo si es Aceptado y no tiene comprobante */}
            {esAceptado && !presupuesto.idComprobanteGenerado && onFacturar && (
              <button
                onClick={() => setShowConvertirModal(true)}
                disabled={!isAfipOnline}
                title={!isAfipOnline ? "Conexión a AFIP no disponible" : "Facturar"}
                className={`inline-flex items-center px-4 py-2 bg-emerald-600 text-white rounded-md hover:bg-emerald-700 font-medium focus:outline-none focus:ring-2 focus:ring-emerald-500 shadow-sm transition-colors ${!isAfipOnline ? 'opacity-50 cursor-not-allowed hidden bg-slate-400 hover:bg-slate-400' : ''}`}
              >
                <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
                Facturar
              </button>
            )}
            <button
              onClick={handleDownloadPdf}
              disabled={loadingPdf}
              className="inline-flex items-center px-4 py-2 bg-blue-600 border border-transparent text-white rounded-md hover:bg-blue-700 font-medium shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors disabled:opacity-50"
            >
              {loadingPdf ? (
                <>
                  <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  Generando...
                </>
              ) : (
                <>
                  <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                  </svg>
                  Ver PDF
                </>
              )}
            </button>
            <button
              onClick={onClose}
              className="px-4 py-2 bg-white border border-slate-300 text-slate-700 font-medium rounded-md hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-blue-500 shadow-sm transition-colors"
            >
              Cerrar
            </button>
          </div>

          {/* Modal de conversión a comprobante */}
          {showConvertirModal && (
            <ConvertirAComprobanteModal
              show={showConvertirModal}
              presupuesto={presupuesto}
              onClose={() => setShowConvertirModal(false)}
              onConfirm={async (id, dto) => {
                const result = await onFacturar(id, dto)
                return result
              }}
            />
          )}
        </div>
      </div>

      {/* PDF Preview Modal */}
      {showPdfPreview && (
        <div className="fixed inset-0 z-[60] overflow-hidden" aria-labelledby="pdf-modal-title" role="dialog" aria-modal="true">
          <div className="absolute inset-0 bg-slate-900/80 transition-opacity" aria-hidden="true" onClick={handleClosePdfPreview}></div>

          <div className="absolute inset-0 flex items-center justify-center p-4 pointer-events-none">
            <div className="w-full h-full max-w-6xl max-h-[95vh] bg-white rounded-xl shadow-2xl flex flex-col pointer-events-auto overflow-hidden">

              {/* Header */}
              <div className="bg-gradient-to-r from-slate-800 to-slate-900 px-6 py-4 flex justify-between items-center shrink-0">
                <div className="flex items-center gap-3">
                  <div className="bg-white/10 p-2 rounded-lg">
                    <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                    </svg>
                  </div>
                  <h3 className="text-xl font-bold text-white">
                    Previsualización del PDF - Presupuesto #{presupuesto.id.toString().padStart(6, '0')}
                  </h3>
                </div>
                <button
                  onClick={handleClosePdfPreview}
                  className="text-slate-300 hover:text-white p-2 hover:bg-white/10 rounded-lg transition-colors focus:outline-none"
                >
                  <svg className="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>

              {/* Content (PDF Preview) */}
              <div className="flex-1 bg-slate-100 relative overflow-hidden">
                <iframe
                  src={pdfUrl}
                  className="w-full h-full border-none"
                  title="Vista Previa Presupuesto"
                />
              </div>

              {/* Footer */}
              <div className="bg-white border-t border-slate-200 px-6 py-4 shrink-0 flex flex-wrap justify-between items-center gap-4">
                <div className="text-sm text-slate-500 hidden md:block">
                  * Para compartir por WhatsApp, recuerda adjuntar el archivo descargado manualmente.
                </div>

                <div className="flex items-center space-x-3 w-full md:w-auto justify-end">
                  {/* Email Share */}
                  <button
                    onClick={handleEmail}
                    disabled={loadingEmail}
                    className="inline-flex items-center px-4 py-2 bg-blue-50 border border-blue-200 text-blue-700 rounded-md hover:bg-blue-100 hover:border-blue-300 font-medium focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 shadow-sm transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    title={getClientContact().email ? `Enviar a ${getClientContact().email}` : 'No hay email registrado'}
                  >
                    {loadingEmail ? (
                      <svg className="animate-spin -ml-1 mr-2 h-5 w-5 text-blue-700" fill="none" viewBox="0 0 24 24">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                      </svg>
                    ) : (
                      <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                      </svg>
                    )}
                    Correo
                  </button>

                  {/* WhatsApp Share */}
                  <button
                    onClick={handleWhatsApp}
                    className="inline-flex items-center px-4 py-2 bg-emerald-50 border border-emerald-200 text-emerald-700 rounded-md hover:bg-emerald-100 hover:border-emerald-300 font-medium focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-emerald-500 shadow-sm transition-colors"
                    title={getClientContact().tel ? `Enviar a ${getClientContact().tel}` : 'No hay teléfono registrado'}
                  >
                    <svg className="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 24 24">
                      <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893a11.821 11.821 0 00-3.48-8.413z" />
                    </svg>
                    WhatsApp
                  </button>

                  <div className="h-6 w-px bg-slate-300 mx-2 hidden md:block"></div>

                  {/* Download */}
                  <button
                    onClick={handleActualDownload}
                    className="inline-flex items-center px-4 py-2 bg-blue-600 border border-transparent text-white rounded-md hover:bg-blue-700 font-medium focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 shadow-sm transition-colors"
                  >
                    <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
                    </svg>
                    Descargar
                  </button>
                </div>
              </div>

            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default PresupuestoDetalle

