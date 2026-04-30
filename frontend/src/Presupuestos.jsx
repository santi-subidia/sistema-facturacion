import React, { useState, useEffect } from 'react'
import { usePresupuestos, useNotification } from './hooks/usePresupuestos'
import Notification from './components/shared/Notification'
import PresupuestoForm from './components/presupuestos/PresupuestoForm'
import PresupuestoDetalle from './components/presupuestos/PresupuestoDetalle'
import { API_BASE_URL } from './config'
import { fetchWithAuth } from './utils/authHeaders'

function Presupuestos() {
  const {
    clientes,
    productos,
    formasPago,
    condicionesVenta,
    loading,
    error,
    createPresupuesto,
    refreshData
  } = usePresupuestos()

  const { notification, showNotification, hideNotification } = useNotification()
  const [submitting, setSubmitting] = useState(false)
  const [presupuestoCreado, setPresupuestoCreado] = useState(null)
  const [showDetalle, setShowDetalle] = useState(false)
  const [estados, setEstados] = useState([])

  useEffect(() => {
    const fetchEstados = async () => {
      try {
        const response = await fetchWithAuth(`${API_BASE_URL}/presupuesto/estados`)
        if (response.ok) {
          setEstados(await response.json())
        }
      } catch (err) {
        console.error('Error al cargar estados:', err)
      }
    }
    fetchEstados()
  }, [])

  const handleSubmit = async (presupuestoData) => {
    setSubmitting(true)

    try {
      const resultado = await createPresupuesto(presupuestoData)

      showNotification('success', '¡Presupuesto creado exitosamente!')

      // Refrescar datos para actualizar stock
      await refreshData()

      // Mostrar el detalle del presupuesto creado
      setPresupuestoCreado(resultado.presupuesto || resultado)
      setShowDetalle(true)

    } catch (err) {
      console.error('Error al crear presupuesto:', err)

      let errorMessage = 'Error al crear el presupuesto'

      if (err.message) {
        errorMessage = err.message
      }

      if (errorMessage.includes('stock')) {
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
        <div className="absolute top-0 left-0 w-full h-1.5 bg-gradient-to-r from-amber-400 via-orange-500 to-amber-600 opacity-70"></div>

        <div className="px-8 py-6 border-b border-slate-100/50 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div>
            <h2 className="text-2xl font-bold text-slate-800 tracking-tight font-outfit uppercase">
              Nuevo <span className="text-orange-600">Presupuesto</span>
            </h2>
            <p className="text-[10px] font-bold text-slate-400 mt-1 uppercase tracking-[0.2em]">
              Cotizaciones y estimaciones para clientes
            </p>
          </div>
        </div>

        <div className="p-8">
          <PresupuestoForm
            clientes={clientes}
            productos={productos}
            formasPago={formasPago}
            condicionesVenta={condicionesVenta}
            onSubmit={handleSubmit}
            submitting={submitting}
            showNotification={showNotification}
          />
        </div>
      </div>

      <PresupuestoDetalle
        show={showDetalle}
        presupuesto={presupuestoCreado}
        estados={estados}
        onClose={() => {
          setShowDetalle(false)
          setPresupuestoCreado(null)
        }}
      />
    </div>
  )
}

export default Presupuestos
