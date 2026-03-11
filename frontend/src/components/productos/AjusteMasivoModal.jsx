import React, { useState, useEffect } from 'react'

function AjusteMasivoModal({
  show,
  productosSeleccionados,
  onClose,
  onConfirm,
  submitting
}) {
  const [porcentaje, setPorcentaje] = useState('')
  const [redondeo, setRedondeo] = useState('')
  const [ejemplos, setEjemplos] = useState([])

  useEffect(() => {
    if (show && productosSeleccionados.length > 0) {
      calcularEjemplos()
    }
  }, [porcentaje, redondeo, show, productosSeleccionados])

  const calcularEjemplos = () => {
    if (!porcentaje || parseFloat(porcentaje) === 0) {
      setEjemplos([])
      return
    }

    const porcentajeNum = parseFloat(porcentaje)
    const redondeoNum = redondeo ? parseInt(redondeo) : null

    // Tomar hasta 5 productos como ejemplo
    const ejemplosCalculados = productosSeleccionados.slice(0, 5).map(producto => {
      let precioNuevo = producto.precio * (1 + porcentajeNum / 100)

      if (redondeoNum && redondeoNum > 0) {
        precioNuevo = Math.round(precioNuevo / redondeoNum) * redondeoNum
      }

      return {
        ...producto,
        precioNuevo
      }
    })

    setEjemplos(ejemplosCalculados)
  }

  const handleSubmit = (e) => {
    e.preventDefault()

    if (!porcentaje || parseFloat(porcentaje) === 0) {
      return
    }

    const redondeoNum = redondeo && parseInt(redondeo) > 0 ? parseInt(redondeo) : null
    onConfirm(parseFloat(porcentaje), redondeoNum)
  }

  const handleClose = () => {
    setPorcentaje('')
    setRedondeo('')
    setEjemplos([])
    onClose()
  }

  if (!show) return null

  return (
    <div className="fixed z-[60] inset-0 overflow-y-auto">
      <div className="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
        <div
          className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity"
          onClick={handleClose}
        ></div>

        <div className="relative z-10 inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-2xl sm:w-full">
          <form onSubmit={handleSubmit}>
            <div className="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
              <div className="sm:flex sm:items-start">
                <div className="mx-auto flex-shrink-0 flex items-center justify-center h-12 w-12 rounded-full bg-blue-100 sm:mx-0 sm:h-10 sm:w-10">
                  <svg className="h-6 w-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 12l3-3 3 3 4-4M8 21l4-4 4 4M3 4h18M4 4h16v12a1 1 0 01-1 1H5a1 1 0 01-1-1V4z" />
                  </svg>
                </div>
                <div className="mt-3 text-center sm:mt-0 sm:ml-4 sm:text-left w-full">
                  <h3 className="text-lg leading-6 font-medium text-gray-900 mb-4">
                    Ajuste Masivo de Precios
                  </h3>

                  <div className="mb-4">
                    <div className="bg-blue-50 border border-blue-200 rounded-lg p-3">
                      <p className="text-sm text-blue-800">
                        Se aplicará el ajuste a <span className="font-bold">{productosSeleccionados.length}</span> producto(s) seleccionado(s)
                      </p>
                      <p className="text-xs text-blue-600 mt-1">
                        Usa valores positivos para aumentar precios o negativos para descuentos
                      </p>
                    </div>
                  </div>

                  <div className="space-y-4">
                    {/* Porcentaje */}
                    <div>
                      <label htmlFor="porcentaje" className="block text-sm font-medium text-gray-700">
                        Porcentaje de Ajuste *
                      </label>
                      <div className="mt-1 relative rounded-md shadow-sm">
                        <input
                          type="number"
                          name="porcentaje"
                          id="porcentaje"
                          step="0.01"
                          value={porcentaje}
                          onChange={(e) => setPorcentaje(e.target.value)}
                          className="block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                          placeholder="15.50 o -10.00"
                          required
                        />
                        <div className="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
                          <span className="text-gray-500 sm:text-sm">%</span>
                        </div>
                      </div>
                    </div>

                    {/* Redondeo */}
                    <div>
                      <label htmlFor="redondeo" className="block text-sm font-medium text-gray-700">
                        Redondeo (opcional)
                      </label>
                      <select
                        name="redondeo"
                        id="redondeo"
                        value={redondeo}
                        onChange={(e) => setRedondeo(e.target.value)}
                        className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                      >
                        <option value="">Sin redondeo</option>
                        <option value="10">Redondear a múltiplos de 10</option>
                        <option value="50">Redondear a múltiplos de 50</option>
                        <option value="100">Redondear a múltiplos de 100</option>
                      </select>
                      <p className="mt-1 text-xs text-gray-500">
                        El redondeo se aplica después de calcular el aumento
                      </p>
                    </div>

                    {/* Preview de ejemplos */}
                    {ejemplos.length > 0 && (
                      <div>
                        <label className="block text-sm font-medium text-gray-700 mb-2">
                          Vista Previa
                        </label>
                        <div className="border border-gray-200 rounded-lg overflow-hidden">
                          <table className="min-w-full divide-y divide-gray-200">
                            <thead className="bg-gray-50">
                              <tr>
                                <th className="px-3 py-2 text-left text-xs font-medium text-gray-500 uppercase">Producto</th>
                                <th className="px-3 py-2 text-right text-xs font-medium text-gray-500 uppercase">Precio Actual</th>
                                <th className="px-3 py-2 text-right text-xs font-medium text-gray-500 uppercase">Precio Nuevo</th>
                              </tr>
                            </thead>
                            <tbody className="bg-white divide-y divide-gray-200">
                              {ejemplos.map((ejemplo) => (
                                <tr key={ejemplo.id}>
                                  <td className="px-3 py-2 text-sm text-gray-900">{ejemplo.nombre}</td>
                                  <td className="px-3 py-2 text-sm text-gray-900 text-right">${ejemplo.precio.toFixed(2)}</td>
                                  <td className="px-3 py-2 text-sm font-medium text-green-600 text-right">${ejemplo.precioNuevo.toFixed(2)}</td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                          {productosSeleccionados.length > 5 && (
                            <div className="px-3 py-2 bg-gray-50 text-xs text-gray-500 text-center">
                              ... y {productosSeleccionados.length - 5} producto(s) más
                            </div>
                          )}
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              </div>
            </div>

            <div className="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
              <button
                type="submit"
                disabled={submitting || !porcentaje || parseFloat(porcentaje) === 0}
                className="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-blue-600 text-base font-medium text-white hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 sm:ml-3 sm:w-auto sm:text-sm disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {submitting ? (
                  <>
                    <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    Aplicando...
                  </>
                ) : (
                  'Confirmar Ajuste'
                )}
              </button>
              <button
                type="button"
                onClick={handleClose}
                disabled={submitting}
                className="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm disabled:opacity-50 disabled:cursor-not-allowed"
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

export default AjusteMasivoModal
