import React from 'react'

function SelectionBar({ 
  selectedCount, 
  onAplicarAumento, 
  onCancelar,
  modoSeleccion = 'precio'
}) {
  if (selectedCount === 0) return null

  const config = {
    precio: {
      bgColor: 'bg-blue-100',
      textColor: 'text-blue-600',
      buttonColor: 'bg-blue-600 hover:bg-blue-700 focus:ring-blue-500',
      icon: (
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
      ),
      buttonText: 'Ajustar Precios',
      buttonIcon: (
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" />
      )
    },
    stock: {
      bgColor: 'bg-indigo-100',
      textColor: 'text-indigo-600',
      buttonColor: 'bg-indigo-600 hover:bg-indigo-700 focus:ring-indigo-500',
      icon: (
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" />
      ),
      buttonText: 'Ajustar Stock',
      buttonIcon: (
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" />
      )
    }
  }

  const currentConfig = config[modoSeleccion] || config.precio

  return (
    <div className="fixed bottom-6 left-1/2 transform -translate-x-1/2 z-50">
      <div className="bg-white rounded-lg shadow-2xl border border-gray-200 px-6 py-4 flex items-center space-x-4">
        <div className="flex items-center">
          <div className={`flex-shrink-0 h-10 w-10 rounded-full ${currentConfig.bgColor} flex items-center justify-center`}>
            <svg className={`h-5 w-5 ${currentConfig.textColor}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
              {currentConfig.icon}
            </svg>
          </div>
          <div className="ml-3">
            <p className="text-sm font-medium text-gray-900">
              {selectedCount} producto{selectedCount !== 1 ? 's' : ''} seleccionado{selectedCount !== 1 ? 's' : ''}
            </p>
          </div>
        </div>

        <div className="flex space-x-2">
          <button
            onClick={onAplicarAumento}
            className={`inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white ${currentConfig.buttonColor} focus:outline-none focus:ring-2 focus:ring-offset-2 transition-colors`}
          >
            <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              {currentConfig.buttonIcon}
            </svg>
            {currentConfig.buttonText}
          </button>
          <button
            onClick={onCancelar}
            className="inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
          >
            <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
            Cancelar
          </button>
        </div>
      </div>
    </div>
  )
}

export default SelectionBar
