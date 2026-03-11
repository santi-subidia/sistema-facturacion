import React, { useState } from 'react'
import { useComprobantes, useNotification } from './hooks/useComprobantes'
import { useConnectivity } from './hooks/useConnectivity'
import Notification from './components/shared/Notification'
import ComprobanteForm from './components/comprobantes/ComprobanteForm'
import ComprobanteDetalle from './components/comprobantes/ComprobanteDetalle'

function Comprobantes() {
  const {
    clientes,
    productos,
    tiposComprobante,
    formasPago,
    condicionesVenta,
    afipConfig,
    loading,
    error,
    createComprobante,
    refreshData
  } = useComprobantes()

  const { notification, showNotification, hideNotification } = useNotification()
  const { isAfipOnline } = useConnectivity()
  const [submitting, setSubmitting] = useState(false)
  const [comprobanteCreado, setComprobanteCreado] = useState(null)
  const [showDetalle, setShowDetalle] = useState(false)

  const handleSubmit = async (comprobanteData) => {
    setSubmitting(true)

    try {
      // Obtener el usuario actual del localStorage
      const currentUser = JSON.parse(localStorage.getItem('user') || '{}')

      // Agregar el ID del usuario creador
      const comprobanteCompleto = {
        ...comprobanteData,
        idCreado_por: currentUser.id || 1, // Por defecto 1 si no hay usuario
        fecha: new Date().toISOString()
      }

      const resultado = await createComprobante(comprobanteCompleto)

      // Mostrar notificación de éxito con datos AFIP si existen
      const afipMessage = resultado.message

      showNotification('success', afipMessage)

      // Refrescar datos para actualizar stock
      await refreshData()

      // Mostrar el detalle del comprobante creado
      setComprobanteCreado(resultado.comprobante || resultado)
      setShowDetalle(true)

      // El formulario se limpiará automáticamente
    } catch (err) {
      console.error('Error al crear comprobante:', err)

      // Extraer mensaje de error específico del backend
      let errorMessage = 'Error al crear el comprobante'

      if (err.message) {
        errorMessage = err.message
      }

      // Mensajes específicos para errores comunes de AFIP
      if (errorMessage.includes('AFIP')) {
        errorMessage = `Error de AFIP: ${errorMessage}`
      } else if (errorMessage.includes('stock')) {
        errorMessage = `Error de stock: ${errorMessage}`
      } else if (errorMessage.includes('validación') || errorMessage.includes('validacion')) {
        errorMessage = `Error de validación: ${errorMessage}`
      }

      showNotification('error', errorMessage)
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) {
    return (
      <div className="flex flex-col justify-center items-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mb-4"></div>
        <p className="text-gray-600 text-sm">Cargando datos del sistema...</p>
      </div>
    )
  }

  if (error) {
    return (
      <div className="bg-red-50 border border-red-200 rounded-lg p-4">
        <p className="text-red-800">Error: {error}</p>
      </div>
    )
  }

  return (
    <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-700">
      <Notification
        notification={notification}
        onClose={hideNotification}
      />

      <div className="glass-panel rounded-3xl overflow-hidden shadow-xl border border-white/40 group relative">
        <div className="absolute top-0 left-0 w-full h-1.5 bg-gradient-to-r from-blue-400 via-blue-600 to-sky-400 opacity-70"></div>

        <div className="px-8 py-6 border-b border-slate-100/50 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div>
            <h2 className="text-2xl font-bold text-slate-800 tracking-tight font-outfit uppercase">
              Nueva <span className="text-blue-600">Factura</span>
            </h2>
            <p className="text-xs font-medium text-slate-400 mt-1 uppercase tracking-widest">
              Generación de facturas
            </p>
          </div>

          <div className="flex items-center gap-3">
            <span className={`h-2.5 w-2.5 rounded-full animate-pulse ${isAfipOnline ? 'bg-emerald-500 shadow-[0_0_10px_rgba(16,185,129,0.5)]' : 'bg-rose-500 shadow-[0_0_10px_rgba(244,63,94,0.5)]'}`}></span>
            <span className="text-[10px] font-bold uppercase tracking-widest text-slate-500">
              {isAfipOnline ? 'Servicio AFIP Activo' : 'Servicio AFIP Offline'}
            </span>
          </div>
        </div>

        <div className="p-8 relative">
          {!isAfipOnline && (
            <div className="absolute inset-0 bg-white/40 backdrop-blur-md z-20 flex flex-col items-center justify-center p-8 text-center rounded-b-3xl transition-all duration-500">
              <div className="glass-panel bg-white/80 p-8 rounded-3xl shadow-2xl border border-rose-100 max-w-md animate-in zoom-in-95 duration-300">
                <div className="h-16 w-16 bg-rose-50 rounded-2xl flex items-center justify-center mx-auto mb-6">
                  <svg className="h-8 w-8 text-rose-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2.5" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                  </svg>
                </div>
                <h3 className="text-lg font-bold text-rose-900 font-outfit uppercase tracking-tight mb-2">Conexión Interrumpida</h3>
                <p className="text-sm text-rose-700/80 leading-relaxed font-medium">
                  La comunicación con los servidores de AFIP no está disponible. Los comprobantes electrónicos requieren conexión activa para ser validados legalmente.
                </p>
                <button
                  onClick={refreshData}
                  className="mt-6 px-6 py-2.5 bg-rose-600 text-white text-[10px] font-bold uppercase tracking-[0.2em] rounded-xl hover:bg-rose-500 transition-all shadow-lg shadow-rose-200"
                >
                  Reintentar Conexión
                </button>
              </div>
            </div>
          )}
          <ComprobanteForm
            clientes={clientes}
            productos={productos}
            tiposComprobante={tiposComprobante}
            formasPago={formasPago}
            condicionesVenta={condicionesVenta}
            onSubmit={handleSubmit}
            submitting={submitting}
            showNotification={showNotification}
          />
        </div>
      </div>

      <ComprobanteDetalle
        show={showDetalle}
        comprobante={comprobanteCreado}
        afipConfig={afipConfig}
        onClose={() => {
          setShowDetalle(false)
          setComprobanteCreado(null)
        }}
      />
    </div>
  )
}

export default Comprobantes
