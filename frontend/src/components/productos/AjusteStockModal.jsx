import React, { useState, useEffect } from 'react'

function AjusteStockModal({
  show,
  productosSeleccionados,
  onClose,
  onConfirm,
  submitting,
  isAdmin
}) {
  const [ajustes, setAjustes] = useState([])

  useEffect(() => {
    if (show && productosSeleccionados.length > 0) {
      // Inicializar ajustes para cada producto
      const ajustesIniciales = productosSeleccionados.map(producto => ({
        id: producto.id,
        nombre: producto.nombre,
        codigo: producto.codigo,
        stock: producto.stock,
        stockNegro: producto.stockNegro || 0,
        tipoAjuste: 'ingreso', // 'ingreso', 'egreso', 'fisico'
        cantidad: 0,
        esStockNegro: false,
        stockNuevo: producto.stock
      }))
      setAjustes(ajustesIniciales)
    }
  }, [show, productosSeleccionados])

  const calcularStockNuevo = (stockActual, tipoAjuste, cantidad) => {
    const cantidadNum = parseInt(cantidad) || 0

    switch (tipoAjuste) {
      case 'ingreso':
        return stockActual + cantidadNum
      case 'egreso':
        return Math.max(0, stockActual - cantidadNum) // No permitir stock negativo
      case 'fisico':
        return cantidadNum
      default:
        return stockActual
    }
  }

  const handleTipoAjusteChange = (productoId, nuevoTipo) => {
    setAjustes(prev => prev.map(ajuste => {
      if (ajuste.id === productoId) {
        const stockBase = ajuste.esStockNegro ? ajuste.stockNegro : ajuste.stock;
        const stockNuevo = calcularStockNuevo(stockBase, nuevoTipo, ajuste.cantidad)
        return { ...ajuste, tipoAjuste: nuevoTipo, stockNuevo }
      }
      return ajuste
    }))
  }

  const handleCantidadChange = (productoId, nuevaCantidad) => {
    setAjustes(prev => prev.map(ajuste => {
      if (ajuste.id === productoId) {
        const stockBase = ajuste.esStockNegro ? ajuste.stockNegro : ajuste.stock;
        const stockNuevo = calcularStockNuevo(stockBase, ajuste.tipoAjuste, nuevaCantidad)
        return { ...ajuste, cantidad: nuevaCantidad, stockNuevo }
      }
      return ajuste
    }))
  }

  const handleEsStockNegroChange = (productoId, valor) => {
    setAjustes(prev => prev.map(ajuste => {
      if (ajuste.id === productoId) {
        const stockBase = valor ? ajuste.stockNegro : ajuste.stock;
        const stockNuevo = calcularStockNuevo(stockBase, ajuste.tipoAjuste, ajuste.cantidad)
        return { ...ajuste, esStockNegro: valor, stockNuevo }
      }
      return ajuste
    }))
  }

  const handleSubmit = (e) => {
    e.preventDefault()

    // Filtrar solo los ajustes que tienen cantidad > 0 (excepto físico que puede ser 0)
    const ajustesValidos = ajustes.filter(ajuste =>
      ajuste.tipoAjuste === 'fisico' || (parseInt(ajuste.cantidad) || 0) > 0
    )

    if (ajustesValidos.length === 0) {
      return
    }

    // Preparar datos para enviar
    const ajustesParaEnviar = ajustesValidos.map(ajuste => ({
      id: ajuste.id,
      tipoAjuste: ajuste.tipoAjuste,
      cantidad: parseInt(ajuste.cantidad) || 0,
      stockNuevo: ajuste.stockNuevo,
      esStockNegro: ajuste.esStockNegro
    }))

    onConfirm(ajustesParaEnviar)
  }

  const handleClose = () => {
    setAjustes([])
    onClose()
  }

  if (!show) return null

  const getTipoAjusteLabel = (tipo) => {
    switch (tipo) {
      case 'ingreso': return 'Ingreso (+)'
      case 'egreso': return 'Egreso (-)'
      case 'fisico': return 'Físico (=)'
      default: return ''
    }
  }

  const getTipoAjusteColor = (tipo) => {
    switch (tipo) {
      case 'ingreso': return 'text-green-600'
      case 'egreso': return 'text-red-600'
      case 'fisico': return 'text-blue-600'
      default: return 'text-gray-600'
    }
  }

  return (
    <div className="fixed z-[60] inset-0 overflow-y-auto">
      <div className="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
        <div
          className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity"
          onClick={handleClose}
        ></div>

        <div className="relative z-10 inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-5xl sm:w-full">
          <form onSubmit={handleSubmit}>
            <div className="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
              <div className="sm:flex sm:items-start">
                <div className="mx-auto flex-shrink-0 flex items-center justify-center h-12 w-12 rounded-full bg-indigo-100 sm:mx-0 sm:h-10 sm:w-10">
                  <svg className="h-6 w-6 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" />
                  </svg>
                </div>
                <div className="mt-3 text-center sm:mt-0 sm:ml-4 sm:text-left w-full">
                  <h3 className="text-lg leading-6 font-medium text-gray-900 mb-4">
                    Ajuste de Stock - Modo Inventario
                  </h3>

                  <div className="mb-4">
                    <div className="bg-indigo-50 border border-indigo-200 rounded-lg p-3">
                      <p className="text-sm text-indigo-800">
                        Ajustando stock de <span className="font-bold">{productosSeleccionados.length}</span> producto(s) seleccionado(s)
                      </p>
                      <p className="text-xs text-indigo-600 mt-1">
                        <span className="font-semibold">Ingreso (+):</span> suma al stock |
                        <span className="font-semibold"> Egreso (-):</span> resta del stock |
                        <span className="font-semibold"> Físico (=):</span> reemplaza el valor
                      </p>
                    </div>
                  </div>

                  {/* Tabla de ajustes */}
                  <div className="overflow-x-auto border border-gray-200 rounded-lg">
                    <table className="min-w-full divide-y divide-gray-200">
                      <thead className="bg-gray-50">
                        <tr>
                          <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                            Producto
                          </th>
                          <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                            Stock Actual
                          </th>
                          {isAdmin && (
                            <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                              Tipo de Stock
                            </th>
                          )}
                          <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                            Tipo de Ajuste
                          </th>
                          <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                            Cantidad
                          </th>
                          <th className="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">
                            Nuevo Stock
                          </th>
                        </tr>
                      </thead>
                      <tbody className="bg-white divide-y divide-gray-200">
                        {ajustes.map((ajuste) => (
                          <tr key={ajuste.id} className="hover:bg-gray-50">
                            <td className="px-4 py-3">
                              <div className="text-sm font-medium text-gray-900">{ajuste.nombre}</div>
                              <div className="text-xs text-gray-500">{ajuste.codigo}</div>
                            </td>
                            <td className="px-4 py-3 text-center">
                              <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-sm font-medium bg-gray-100 text-gray-800">
                                {ajuste.esStockNegro ? ajuste.stockNegro : ajuste.stock}
                              </span>
                            </td>
                            {isAdmin && (
                              <td className="px-4 py-3">
                                <select
                                  value={ajuste.esStockNegro ? 'negro' : 'blanco'}
                                  onChange={(e) => handleEsStockNegroChange(ajuste.id, e.target.value === 'negro')}
                                  className="block w-full border border-gray-300 rounded-md shadow-sm py-1.5 px-2 text-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                                >
                                  <option value="blanco">Blanco</option>
                                  <option value="negro">Negro</option>
                                </select>
                              </td>
                            )}
                            <td className="px-4 py-3">
                              <select
                                value={ajuste.tipoAjuste}
                                onChange={(e) => handleTipoAjusteChange(ajuste.id, e.target.value)}
                                className="block w-full border border-gray-300 rounded-md shadow-sm py-1.5 px-2 text-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                              >
                                <option value="ingreso">Ingreso (+)</option>
                                <option value="egreso">Egreso (-)</option>
                                <option value="fisico">Físico (=)</option>
                              </select>
                            </td>
                            <td className="px-4 py-3">
                              <input
                                type="number"
                                min="0"
                                value={ajuste.cantidad}
                                onChange={(e) => handleCantidadChange(ajuste.id, e.target.value)}
                                className="block w-full border border-gray-300 rounded-md shadow-sm py-1.5 px-2 text-sm text-center focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                                placeholder={ajuste.tipoAjuste === 'fisico' ? 'Ej: 8' : 'Ej: 5'}
                              />
                            </td>
                            <td className="px-4 py-3 text-center">
                              <span className={`inline-flex items-center px-3 py-1 rounded-full text-sm font-bold ${getTipoAjusteColor(ajuste.tipoAjuste)}`}>
                                {ajuste.stockNuevo}
                              </span>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>

                  <div className="mt-4 bg-yellow-50 border border-yellow-200 rounded-lg p-3">
                    <p className="text-xs text-yellow-800">
                      <span className="font-semibold">⚠️ Nota:</span> Los cambios se guardarán al presionar "Confirmar Ajuste".
                      Verifica los valores antes de confirmar.
                    </p>
                  </div>
                </div>
              </div>
            </div>

            <div className="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
              <button
                type="submit"
                disabled={submitting}
                className="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-indigo-600 text-base font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:ml-3 sm:w-auto sm:text-sm disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {submitting ? (
                  <>
                    <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    Guardando...
                  </>
                ) : (
                  'Confirmar Ajuste'
                )}
              </button>
              <button
                type="button"
                onClick={handleClose}
                disabled={submitting}
                className="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm disabled:opacity-50 disabled:cursor-not-allowed"
              >
                Cancelar
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  )
}

export default AjusteStockModal
