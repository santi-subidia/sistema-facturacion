import React, { useState } from 'react'

function ProductoModal({
  show,
  editingProduct,
  onClose,
  onSubmit,
  submitting,
  isAdmin
}) {
  const [formData, setFormData] = useState({
    nombre: editingProduct?.nombre || '',
    codigo: editingProduct?.codigo || '',
    precio: editingProduct?.precio?.toString() || '',
    stock: editingProduct?.stock?.toString() || '',
    stockNegro: editingProduct?.stockNegro?.toString() || '0',
    proveedor: editingProduct?.proveedor || ''
  })
  const [formErrors, setFormErrors] = useState({})

  React.useEffect(() => {
    if (editingProduct) {
      setFormData({
        nombre: editingProduct.nombre,
        codigo: editingProduct.codigo,
        precio: editingProduct.precio.toString(),
        stock: editingProduct.stock.toString(),
        stockNegro: editingProduct.stockNegro?.toString() || '0',
        proveedor: editingProduct.proveedor || ''
      })
    } else {
      setFormData({
        nombre: '',
        codigo: '',
        precio: '',
        stock: '',
        stockNegro: '0',
        proveedor: ''
      })
    }
    setFormErrors({})
  }, [editingProduct, show])

  const handleInputChange = (e) => {
    const { name, value } = e.target
    setFormData(prev => ({
      ...prev,
      [name]: value
    }))
    if (formErrors[name]) {
      setFormErrors(prev => ({
        ...prev,
        [name]: ''
      }))
    }
  }

  const validateForm = () => {
    const errors = {}

    if (!formData.nombre.trim()) {
      errors.nombre = 'El nombre es obligatorio.'
    } else if (formData.nombre.trim().length < 3) {
      errors.nombre = 'El nombre debe tener al menos 3 caracteres.'
    } else if (formData.nombre.trim().length > 100) {
      errors.nombre = 'El nombre no puede exceder 100 caracteres.'
    }

    if (!formData.codigo.trim()) {
      errors.codigo = 'El código es obligatorio.'
    } else if (formData.codigo.trim().length > 50) {
      errors.codigo = 'El código no puede exceder 50 caracteres.'
    }

    if (!formData.precio) {
      errors.precio = 'El precio es obligatorio.'
    } else if (isNaN(formData.precio) || parseFloat(formData.precio) < 0) {
      errors.precio = 'El precio debe ser un número positivo.'
    }

    if (!formData.stock) {
      errors.stock = 'El stock es obligatorio.'
    } else if (isNaN(formData.stock) || parseInt(formData.stock) < 0 || !Number.isInteger(parseFloat(formData.stock))) {
      errors.stock = 'El stock debe ser un número entero positivo.'
    }

    if (isAdmin) {
      if (formData.stockNegro && (isNaN(formData.stockNegro) || parseInt(formData.stockNegro) < 0 || !Number.isInteger(parseFloat(formData.stockNegro)))) {
        errors.stockNegro = 'El stock en negro debe ser un número entero positivo.'
      }
    }

    if (formData.proveedor && formData.proveedor.trim().length > 100) {
      errors.proveedor = 'El proveedor no puede exceder 100 caracteres.'
    }

    setFormErrors(errors)
    return Object.keys(errors).length === 0
  }

  const handleSubmit = (e) => {
    e.preventDefault()

    if (!validateForm()) {
      return
    }

    onSubmit(formData)
  }

  if (!show) return null

  return (
    <div className="fixed z-50 inset-0 overflow-y-auto" aria-labelledby="modal-title" role="dialog" aria-modal="true">
      <div className="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
        <div
          className="fixed inset-0 bg-slate-900/50 backdrop-blur-sm transition-opacity"
          aria-hidden="true"
          onClick={onClose}
        ></div>

        <span className="hidden sm:inline-block sm:align-middle sm:h-screen" aria-hidden="true">&#8203;</span>

        <div className="relative z-10 inline-block align-bottom bg-white rounded-2xl text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-2xl sm:w-full">
          <form onSubmit={handleSubmit}>
            <div className="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
              <div className="sm:flex sm:items-start">
                <div className="mx-auto flex-shrink-0 flex items-center justify-center h-12 w-12 rounded-full bg-blue-100 sm:mx-0 sm:h-10 sm:w-10">
                  <svg className="h-6 w-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                  </svg>
                </div>
                <div className="mt-3 text-center sm:mt-0 sm:ml-4 sm:text-left w-full">
                  <h3 className="text-lg leading-6 font-medium text-slate-900 mb-4" id="modal-title">
                    {editingProduct ? 'Editar Producto' : 'Nuevo Producto'}
                  </h3>

                  <div className="space-y-4">
                    {/* Nombre */}
                    <div>
                      <label htmlFor="nombre" className="block text-sm font-medium text-gray-700">
                        Nombre <span className="text-red-500">*</span>
                      </label>
                      <input
                        type="text"
                        name="nombre"
                        id="nombre"
                        value={formData.nombre}
                        onChange={handleInputChange}
                        className={`mt-1 block w-full border ${formErrors.nombre ? 'border-red-300' : 'border-slate-300'} rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm`}
                        placeholder="Ej: Laptop HP"
                      />
                      {formErrors.nombre && (
                        <p className="mt-1 text-sm text-red-600">{formErrors.nombre}</p>
                      )}
                    </div>

                    {/* Código y Proveedor en Grid */}
                    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                      {/* Código */}
                      <div>
                        <label htmlFor="codigo" className="block text-sm font-medium text-gray-700">
                          Código <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="text"
                          name="codigo"
                          id="codigo"
                          value={formData.codigo}
                          onChange={handleInputChange}
                          className={`mt-1 block w-full border ${formErrors.codigo ? 'border-red-300' : 'border-slate-300'} rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm`}
                          placeholder="PROD-001"
                        />
                        {formErrors.codigo && (
                          <p className="mt-1 text-sm text-red-600">{formErrors.codigo}</p>
                        )}
                      </div>

                      {/* Proveedor */}
                      <div>
                        <label htmlFor="proveedor" className="block text-sm font-medium text-gray-700">
                          Proveedor
                        </label>
                        <input
                          type="text"
                          name="proveedor"
                          id="proveedor"
                          value={formData.proveedor}
                          onChange={handleInputChange}
                          className={`mt-1 block w-full border ${formErrors.proveedor ? 'border-red-300' : 'border-slate-300'} rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm`}
                          placeholder="Distribuidora XYZ"
                        />
                        {formErrors.proveedor && (
                          <p className="mt-1 text-sm text-red-600">{formErrors.proveedor}</p>
                        )}
                      </div>
                    </div>

                    {/* Precio y Stock en Grid */}
                    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                      {/* Precio */}
                      <div>
                        <label htmlFor="precio" className="block text-sm font-medium text-gray-700">
                          Precio <span className="text-red-500">*</span>
                        </label>
                        <div className="mt-1 relative rounded-md shadow-sm">
                          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                            <span className="text-slate-500 sm:text-sm">$</span>
                          </div>
                          <input
                            type="number"
                            name="precio"
                            id="precio"
                            step="0.01"
                            min="0"
                            value={formData.precio}
                            onChange={handleInputChange}
                            className={`block w-full pl-7 pr-3 border ${formErrors.precio ? 'border-red-300' : 'border-slate-300'} rounded-md shadow-sm py-2 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm`}
                            placeholder="0.00"
                          />
                        </div>
                        {formErrors.precio && (
                          <p className="mt-1 text-sm text-red-600">{formErrors.precio}</p>
                        )}
                      </div>

                      {/* Stock */}
                      <div>
                        <label htmlFor="stock" className="block text-sm font-medium text-gray-700">
                          {isAdmin ? 'Stock Blanco' : 'Stock'} <span className="text-red-500">*</span>
                        </label>
                        <input
                          type="number"
                          name="stock"
                          id="stock"
                          min="0"
                          step="1"
                          value={formData.stock}
                          onChange={handleInputChange}
                          className={`mt-1 block w-full border ${formErrors.stock ? 'border-red-300' : 'border-slate-300'} rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm`}
                          placeholder="0"
                        />
                        {formErrors.stock && (
                          <p className="mt-1 text-sm text-red-600">{formErrors.stock}</p>
                        )}
                      </div>
                    </div>

                    {/* Stock Negro y Total en Grid (solo admin) */}
                    {isAdmin && (
                      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                        {/* Stock Negro */}
                        <div>
                          <label htmlFor="stockNegro" className="block text-sm font-medium text-gray-700">
                            Stock en Negro
                          </label>
                          <input
                            type="number"
                            name="stockNegro"
                            id="stockNegro"
                            min="0"
                            step="1"
                            value={formData.stockNegro}
                            onChange={handleInputChange}
                            className={`mt-1 block w-full border ${formErrors.stockNegro ? 'border-red-300' : 'border-slate-300'} rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm`}
                            placeholder="0"
                          />
                          {formErrors.stockNegro && (
                            <p className="mt-1 text-sm text-red-600">{formErrors.stockNegro}</p>
                          )}
                        </div>

                        {/* Total Stock */}
                        <div>
                          <label className="block text-sm font-medium text-gray-700">
                            Total de Stock (Blanco + Negro)
                          </label>
                          <div className="mt-1 block w-full border border-slate-200 bg-slate-50 rounded-md shadow-sm py-2 px-3 text-sm text-slate-700 font-semibold">
                            {(parseInt(formData.stock) || 0) + (parseInt(formData.stockNegro) || 0)}
                          </div>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
              </div>
            </div>
            <div className="bg-slate-50 border-t border-slate-100 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
              <button
                type="submit"
                disabled={submitting}
                className={`w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 text-base font-medium text-white sm:ml-3 sm:w-auto sm:text-sm ${submitting
                  ? 'bg-blue-400 cursor-not-allowed'
                  : 'bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500'
                  }`}
              >
                {submitting ? (
                  <>
                    <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    {editingProduct ? 'Actualizando...' : 'Guardando...'}
                  </>
                ) : (
                  editingProduct ? 'Actualizar Producto' : 'Crear Producto'
                )}
              </button>
              <button
                type="button"
                onClick={onClose}
                disabled={submitting}
                className="mt-3 w-full inline-flex justify-center rounded-md border border-slate-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-slate-700 hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm disabled:opacity-50 disabled:cursor-not-allowed"
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

export default ProductoModal
