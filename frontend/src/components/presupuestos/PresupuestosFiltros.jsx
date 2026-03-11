import React, { useState } from 'react'

function PresupuestosFiltros({ onFilter, loading, estados = [] }) {
  const [isExpanded, setIsExpanded] = useState(false)
  const [filtros, setFiltros] = useState({
    numeroPresupuesto: '',
    cliente: '',
    estado: '',
    fechaDesde: '',
    fechaHasta: '',
    facturado: '',
    totalMinimo: '',
    totalMaximo: ''
  })

  const handleChange = (e) => {
    const { name, value } = e.target
    setFiltros(prev => ({
      ...prev,
      [name]: value
    }))
  }

  const handleApply = () => {
    onFilter(filtros)
  }

  const handleClear = () => {
    const defaultFiltros = {
      numeroPresupuesto: '',
      cliente: '',
      estado: '',
      fechaDesde: '',
      fechaHasta: '',
      facturado: '',
      totalMinimo: '',
      totalMaximo: ''
    }
    setFiltros(defaultFiltros)
    onFilter(defaultFiltros)
  }

  const hasActiveFilters = filtros.numeroPresupuesto || filtros.cliente || filtros.estado || filtros.fechaDesde || filtros.fechaHasta || filtros.facturado || filtros.totalMinimo || filtros.totalMaximo;

  return (
    <div className="border-b border-slate-200">
      <div className="px-6 py-3">
        <button
          onClick={() => setIsExpanded(!isExpanded)}
          className="inline-flex items-center text-sm font-medium text-slate-700 hover:text-slate-900"
        >
          <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z" />
          </svg>
          {isExpanded ? 'Ocultar Filtros' : 'Mostrar Filtros'}
          {hasActiveFilters && (
            <span className="ml-2 inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
              Activos
            </span>
          )}
        </button>
      </div>

      {isExpanded && (
        <div className="px-6 pb-4 bg-slate-50/50">
          <div className="grid grid-cols-1 gap-y-4 gap-x-4 sm:grid-cols-6 mb-4">

            {/* Fechas */}
            <div className="sm:col-span-2">
              <label className="block text-sm font-medium text-slate-700">Fecha Desde</label>
              <input
                type="date"
                name="fechaDesde"
                value={filtros.fechaDesde}
                onChange={handleChange}
                className="mt-1 focus:ring-blue-500 focus:border-blue-500 block w-full shadow-sm sm:text-sm border-slate-300 rounded-md"
              />
            </div>
            <div className="sm:col-span-2">
              <label className="block text-sm font-medium text-slate-700">Fecha Hasta</label>
              <input
                type="date"
                name="fechaHasta"
                value={filtros.fechaHasta}
                onChange={handleChange}
                className="mt-1 focus:ring-blue-500 focus:border-blue-500 block w-full shadow-sm sm:text-sm border-slate-300 rounded-md"
              />
            </div>
            <div className="sm:col-span-2"></div>

            {/* Montos */}
            <div className="sm:col-span-2">
              <label className="block text-sm font-medium text-slate-700">Monto Mínimo</label>
              <div className="mt-1 relative rounded-md shadow-sm">
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <span className="text-slate-500 sm:text-sm">$</span>
                </div>
                <input
                  type="number"
                  name="totalMinimo"
                  value={filtros.totalMinimo}
                  onChange={handleChange}
                  className="focus:ring-blue-500 focus:border-blue-500 block w-full pl-7 sm:text-sm border-slate-300 rounded-md"
                  placeholder="0.00"
                />
              </div>
            </div>
            <div className="sm:col-span-2">
              <label className="block text-sm font-medium text-slate-700">Monto Máximo</label>
              <div className="mt-1 relative rounded-md shadow-sm">
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <span className="text-slate-500 sm:text-sm">$</span>
                </div>
                <input
                  type="number"
                  name="totalMaximo"
                  value={filtros.totalMaximo}
                  onChange={handleChange}
                  className="focus:ring-blue-500 focus:border-blue-500 block w-full pl-7 sm:text-sm border-slate-300 rounded-md"
                  placeholder="0.00"
                />
              </div>
            </div>
            <div className="sm:col-span-2"></div>

            {/* Búsqueda Principal */}
            <div className="sm:col-span-3">
              <label className="block text-sm font-medium text-slate-700">Cliente o Documento</label>
              <input
                type="text"
                name="cliente"
                value={filtros.cliente}
                onChange={handleChange}
                placeholder="Buscar cliente o número doc..."
                className="mt-1 block w-full py-2 px-3 border border-slate-300 bg-white rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                onKeyDown={(e) => e.key === 'Enter' && handleApply()}
              />
            </div>

            <div className="sm:col-span-3">
              <label className="block text-sm font-medium text-slate-700">Número de Presupuesto</label>
              <input
                type="text"
                name="numeroPresupuesto"
                value={filtros.numeroPresupuesto}
                onChange={handleChange}
                placeholder="Ej: 123"
                className="mt-1 block w-full py-2 px-3 border border-slate-300 bg-white rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                onKeyDown={(e) => e.key === 'Enter' && handleApply()}
              />
            </div>

            {/* Estados y Filtros Extras */}
            <div className="sm:col-span-3">
              <label className="block text-sm font-medium text-slate-700">Estado</label>
              <select
                name="estado"
                value={filtros.estado}
                onChange={handleChange}
                className="mt-1 block w-full py-2 px-3 border border-slate-300 bg-white rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
              >
                <option value="">Todos los Estados</option>
                {estados.map((estado) => (
                  <option key={estado.id} value={estado.id}>{estado.nombre}</option>
                ))}
              </select>
            </div>

            <div className="sm:col-span-3">
              <label className="block text-sm font-medium text-slate-700">¿Ya Facturado?</label>
              <select
                name="facturado"
                value={filtros.facturado}
                onChange={handleChange}
                className="mt-1 block w-full py-2 px-3 border border-slate-300 bg-white rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
              >
                <option value="">Todos</option>
                <option value="true">Sí</option>
                <option value="false">No</option>
              </select>
            </div>

          </div>

          <div className="flex justify-end gap-3 mt-4 pt-4 border-t border-slate-200">
            {hasActiveFilters && (
              <button
                type="button"
                onClick={handleClear}
                disabled={loading}
                className="bg-white py-2 px-4 border border-slate-300 rounded-md shadow-sm text-sm font-medium text-slate-700 hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              >
                Limpiar Filtros
              </button>
            )}
            <button
              type="button"
              onClick={handleApply}
              disabled={loading}
              className="inline-flex justify-center py-2 px-4 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50"
            >
              <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
              {loading ? 'Aplicando...' : 'Aplicar Filtros'}
            </button>
          </div>
        </div>
      )}
    </div>
  )
}

export default PresupuestosFiltros
