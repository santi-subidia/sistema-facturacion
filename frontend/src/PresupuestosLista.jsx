import React, { useState } from 'react'
import { usePresupuestosList } from './hooks/usePresupuestosList'
import PresupuestosTable from './components/presupuestos/PresupuestosTable'
import PresupuestoDetalle from './components/presupuestos/PresupuestoDetalle'
import PresupuestosFiltros from './components/presupuestos/PresupuestosFiltros'
import ConvertirAComprobanteModal from './components/presupuestos/ConvertirAComprobanteModal'
import Pagination from './components/shared/Pagination'

function PresupuestosLista() {
  const {
    presupuestos,
    pagination,
    currentPage,
    loading,
    error,
    handlePageChange,
    fetchPresupuestos,
    convertirAComprobante,
    cambiarEstado,
    estados,
    pageSize,
    setPageSize
  } = usePresupuestosList()

  const [filtros, setFiltros] = useState({})

  const handleFilter = (nuevosFiltros) => {
    setFiltros(nuevosFiltros)
    fetchPresupuestos(1, nuevosFiltros)
  }

  const handlePageChangeWithFilters = (newPage) => {
    handlePageChange(newPage)
    fetchPresupuestos(newPage, filtros)
  }

  const [presupuestoSeleccionado, setPresupuestoSeleccionado] = useState(null)
  const [showDetalle, setShowDetalle] = useState(false)

  // Estado para el modal de facturación desde la tabla
  const [presupuestoAFacturar, setPresupuestoAFacturar] = useState(null)
  const [showConvertirModal, setShowConvertirModal] = useState(false)

  const handleVerDetalle = (presupuesto) => {
    setPresupuestoSeleccionado(presupuesto)
    setShowDetalle(true)
  }

  const handleCloseDetalle = () => {
    setShowDetalle(false)
    setPresupuestoSeleccionado(null)
  }

  // Facturar desde la tabla (abre modal directamente)
  const handleFacturarDesdeTabla = (presupuesto) => {
    setPresupuestoAFacturar(presupuesto)
    setShowConvertirModal(true)
  }

  // Función compartida de facturación (usada por detalle y modal de tabla)
  const handleConvertir = async (presupuestoId, dto) => {
    const result = await convertirAComprobante(presupuestoId, dto)
    // Refrescar la lista después de facturar
    fetchPresupuestos(currentPage, filtros)
    return result
  }

  // Cambiar estado del presupuesto
  const handleCambiarEstado = async (presupuestoId, nuevoEstado) => {
    const result = await cambiarEstado(presupuestoId, nuevoEstado)
    // Refrescar la lista después de cambiar estado
    fetchPresupuestos(currentPage, filtros)
    return result
  }

  const handleCloseConvertirModal = () => {
    setShowConvertirModal(false)
    setPresupuestoAFacturar(null)
  }

  return (
    <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-700">
      <div className="glass-panel rounded-3xl overflow-hidden shadow-xl border border-white/40 ring-1 ring-black/5">
        <div className="absolute top-0 left-0 w-full h-1.5 bg-gradient-to-r from-amber-400 via-orange-500 to-orange-400 opacity-70"></div>

        <div className="px-8 py-6 border-b border-slate-100/50">
          <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
            <div>
              <h2 className="text-2xl font-bold text-slate-800 tracking-tight font-outfit uppercase">
                Presupuestos <span className="text-orange-600">Generados</span>
              </h2>
              <p className="text-[10px] font-bold text-slate-400 mt-1 uppercase tracking-[0.2em]">
                Control de cotizaciones emitidas
              </p>
            </div>
            <div className="flex items-center gap-4">
              <div className="flex items-center space-x-2">
                <label htmlFor="pageSize" className="text-sm font-medium text-slate-700">Mostrar:</label>
                <select
                  id="pageSize"
                  value={pageSize}
                  onChange={(e) => setPageSize(parseInt(e.target.value))}
                  className="block w-full pl-3 pr-8 py-1.5 text-sm border-slate-300 focus:outline-none focus:ring-orange-500 focus:border-orange-500 rounded-md"
                >
                  <option value={10}>10</option>
                  <option value={20}>20</option>
                  <option value={50}>50</option>
                  <option value={100}>100</option>
                </select>
              </div>
              {pagination && (
                <div className="px-4 py-2 bg-orange-50 rounded-2xl border border-orange-100/50">
                  <span className="text-[10px] font-bold text-orange-700 uppercase tracking-widest">
                    Total: {pagination.totalItems} presupuestos
                  </span>
                </div>
              )}
            </div>
          </div>
        </div>

        <PresupuestosFiltros onFilter={handleFilter} loading={loading} estados={estados} />

        <div className="px-1">
          <PresupuestosTable
            presupuestos={presupuestos}
            loading={loading}
            error={error}
            onVerDetalle={handleVerDetalle}
            onFacturar={handleFacturarDesdeTabla}
          />
        </div>

        {pagination && (
          <div className="px-8 py-6 border-t border-slate-100/50 bg-white/40">
            <Pagination
              pagination={pagination}
              currentPage={currentPage}
              onPageChange={handlePageChangeWithFilters}
              itemName="presupuestos"
            />
          </div>
        )}
      </div>

      <PresupuestoDetalle
        show={showDetalle}
        presupuesto={presupuestoSeleccionado}
        onClose={handleCloseDetalle}
        onFacturar={handleConvertir}
        onCambiarEstado={handleCambiarEstado}
        estados={estados}
      />

      {/* Modal de conversión cuando se factura desde la tabla */}
      {showConvertirModal && presupuestoAFacturar && (
        <ConvertirAComprobanteModal
          show={showConvertirModal}
          presupuesto={presupuestoAFacturar}
          onClose={handleCloseConvertirModal}
          onConfirm={handleConvertir}
        />
      )}
    </div>
  )
}

export default PresupuestosLista
