import React, { useState } from 'react'

function ProductosFilters({ filters, onApplyFilters, onClearFilters }) {
  const [localFilters, setLocalFilters] = useState(filters)
  const [showFilters, setShowFilters] = useState(false)

  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target
    setLocalFilters(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }))
  }

  const handleApply = () => {
    onApplyFilters(localFilters)
  }

  const handleClear = () => {
    const emptyFilters = {
      search: '',
      proveedor: '',
      sinStock: false,
      pageSize: localFilters.pageSize || 10
    }
    setLocalFilters(emptyFilters)
    onClearFilters()
  }

  const hasActiveFilters = localFilters.search || localFilters.proveedor || localFilters.sinStock

  return (
    <div className="border-b border-gray-200">
      <div className="px-6 py-3 flex justify-between items-center flex-wrap gap-4">
        <button
          onClick={() => setShowFilters(!showFilters)}
          className="inline-flex items-center text-sm font-medium text-gray-700 hover:text-gray-900"
        >
          <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z" />
          </svg>
          {showFilters ? 'Ocultar Filtros' : 'Mostrar Filtros'}
          {hasActiveFilters && (
            <span className="ml-2 inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
              Activos
            </span>
          )}
        </button>

        <div className="flex items-center space-x-2">
          <label htmlFor="pageSize" className="text-sm font-medium text-gray-700">Mostrar:</label>
          <select
            id="pageSize"
            name="pageSize"
            value={localFilters.pageSize || 10}
            onChange={(e) => {
              const newSize = parseInt(e.target.value);
              setLocalFilters(prev => {
                const updated = { ...prev, pageSize: newSize };
                onApplyFilters(updated);
                return updated;
              });
            }}
            className="block w-full pl-3 pr-10 py-1.5 text-sm border-gray-300 focus:outline-none focus:ring-blue-500 focus:border-blue-500 rounded-md"
          >
            <option value={10}>10</option>
            <option value={20}>20</option>
            <option value={50}>50</option>
            <option value={100}>100</option>
          </select>
        </div>
      </div>

      {showFilters && (
        <div className="px-6 pb-4 bg-gray-50">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {/* Búsqueda general */}
            <div>
              <label htmlFor="search" className="block text-sm font-medium text-gray-700 mb-1">
                Buscar por nombre o código
              </label>
              <input
                type="text"
                id="search"
                name="search"
                value={localFilters.search}
                onChange={handleInputChange}
                placeholder="Ej: Laptop, PR001..."
                className="block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
              />
            </div>

            {/* Filtro por proveedor */}
            <div>
              <label htmlFor="proveedor" className="block text-sm font-medium text-gray-700 mb-1">
                Proveedor
              </label>
              <input
                type="text"
                id="proveedor"
                name="proveedor"
                value={localFilters.proveedor}
                onChange={handleInputChange}
                placeholder="Nombre del proveedor"
                className="block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
              />
            </div>

            {/* Filtro sin stock */}
            <div className="flex items-end">
              <label className="flex items-center">
                <input
                  type="checkbox"
                  name="sinStock"
                  checked={localFilters.sinStock}
                  onChange={handleInputChange}
                  className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                />
                <span className="ml-2 text-sm text-gray-700">Solo productos sin stock</span>
              </label>
            </div>
          </div>

          {/* Botones */}
          <div className="mt-4 flex gap-2">
            <button
              onClick={handleApply}
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
            >
              <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
              Aplicar Filtros
            </button>
            {hasActiveFilters && (
              <button
                onClick={handleClear}
                className="inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              >
                <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
                Limpiar Filtros
              </button>
            )}
          </div>
        </div>
      )}
    </div>
  )
}

export default ProductosFilters
