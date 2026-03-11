import React, { useState } from 'react'

function ClienteModal({
  show,
  editingCliente,
  onClose,
  onSubmit,
  submitting,
  condicionesIva
}) {
  const [formData, setFormData] = useState({
    documento: editingCliente?.documento || '',
    nombre: editingCliente?.nombre || '',
    apellido: editingCliente?.apellido || '',
    telefono: editingCliente?.telefono || '',
    correo: editingCliente?.correo || '',
    direccion: editingCliente?.direccion || '',
    idAfipCondicionIva: editingCliente?.idAfipCondicionIva || 5
  })
  const [formErrors, setFormErrors] = useState({})

  React.useEffect(() => {
    if (editingCliente) {
      setFormData({
        documento: editingCliente.documento,
        nombre: editingCliente.nombre,
        apellido: editingCliente.apellido,
        telefono: editingCliente.telefono,
        correo: editingCliente.correo,
        direccion: editingCliente.direccion,
        idAfipCondicionIva: editingCliente.idAfipCondicionIva
      })
    } else {
      setFormData({
        documento: '',
        nombre: '',
        apellido: '',
        telefono: '',
        correo: '',
        direccion: '',
        idAfipCondicionIva: 5
      })
    }
    setFormErrors({})
  }, [editingCliente, show])

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

    // Validar Documento (DNI o CUIT)
    if (!formData.documento.trim()) {
      errors.documento = 'El documento es obligatorio.'
    } else if (formData.documento.trim().length > 13) {
      errors.documento = 'El documento no puede exceder 13 caracteres.'
    } else if (!/^(\d{7,8}|\d{2}-\d{8}-\d{1})$/.test(formData.documento)) {
      errors.documento = 'Debe ser un DNI (7-8 dígitos) o CUIT (XX-XXXXXXXX-X).'
    }

    // Validar Nombre
    if (!formData.nombre.trim()) {
      errors.nombre = 'El nombre es obligatorio.'
    } else if (formData.nombre.trim().length < 2) {
      errors.nombre = 'El nombre debe tener al menos 2 caracteres.'
    } else if (formData.nombre.trim().length > 100) {
      errors.nombre = 'El nombre no puede exceder 100 caracteres.'
    } else if (!/^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$/.test(formData.nombre)) {
      errors.nombre = 'El nombre solo puede contener letras y espacios.'
    }

    // Validar Apellido
    if (!formData.apellido.trim()) {
      errors.apellido = 'El apellido es obligatorio.'
    } else if (formData.apellido.trim().length < 2) {
      errors.apellido = 'El apellido debe tener al menos 2 caracteres.'
    } else if (formData.apellido.trim().length > 100) {
      errors.apellido = 'El apellido no puede exceder 100 caracteres.'
    } else if (!/^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$/.test(formData.apellido)) {
      errors.apellido = 'El apellido solo puede contener letras y espacios.'
    }

    // Validar Teléfono
    if (!formData.telefono.trim()) {
      errors.telefono = 'El teléfono es obligatorio.'
    } else if (formData.telefono.trim().length > 20) {
      errors.telefono = 'El teléfono no puede exceder 20 caracteres.'
    } else if (!/^[0-9\s\-\+\(\)]+$/.test(formData.telefono)) {
      errors.telefono = 'El teléfono solo puede contener números, espacios y los caracteres: + - ( )'
    }

    // Validar Correo
    if (!formData.correo.trim()) {
      errors.correo = 'El correo es obligatorio.'
    } else if (formData.correo.trim().length > 100) {
      errors.correo = 'El correo no puede exceder 100 caracteres.'
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.correo)) {
      errors.correo = 'El correo no tiene un formato válido.'
    }

    // Validar Dirección
    if (!formData.direccion.trim()) {
      errors.direccion = 'La dirección es obligatoria.'
    } else if (formData.direccion.trim().length > 200) {
      errors.direccion = 'La dirección no puede exceder 200 caracteres.'
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
    <div className="fixed z-[60] inset-0 overflow-y-auto">
      <div className="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
        <div
          className="fixed inset-0 bg-slate-900/50 backdrop-blur-sm transition-opacity"
          onClick={onClose}
        ></div>

        <span className="hidden sm:inline-block sm:align-middle sm:h-screen" aria-hidden="true">&#8203;</span>

        <div className="relative z-10 inline-block align-bottom bg-white rounded-2xl text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
          <form onSubmit={handleSubmit}>
            <div className="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
              <div className="sm:flex sm:items-start">
                <div className="mx-auto flex-shrink-0 flex items-center justify-center h-12 w-12 rounded-full bg-blue-100 sm:mx-0 sm:h-10 sm:w-10">
                  <svg className="h-6 w-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                  </svg>
                </div>
                <div className="mt-3 text-center sm:mt-0 sm:ml-4 sm:text-left w-full">
                  <h3 className="text-lg leading-6 font-medium text-slate-900 mb-4">
                    {editingCliente ? 'Editar Cliente' : 'Crear Nuevo Cliente'}
                  </h3>

                  <div className="space-y-4">
                    {/* Documento */}
                    <div>
                      <label htmlFor="documento" className="block text-sm font-medium text-gray-700">
                        Documento (DNI o CUIT) *
                      </label>
                      <input
                        type="text"
                        name="documento"
                        id="documento"
                        value={formData.documento}
                        onChange={handleInputChange}
                        className={`mt-1 block w-full border ${formErrors.documento ? 'border-red-300' : 'border-slate-300'} rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm`}
                        placeholder="12345678 o 20-12345678-9"
                      />
                      {formErrors.documento && (
                        <p className="mt-1 text-sm text-red-600">{formErrors.documento}</p>
                      )}
                    </div>

                    {/* Nombre */}
                    <div>
                      <label htmlFor="nombre" className="block text-sm font-medium text-gray-700">
                        Nombre *
                      </label>
                      <input
                        type="text"
                        name="nombre"
                        id="nombre"
                        value={formData.nombre}
                        onChange={handleInputChange}
                        className={`mt-1 block w-full border ${formErrors.nombre ? 'border-red-300' : 'border-slate-300'} rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm`}
                        placeholder="Juan"
                      />
                      {formErrors.nombre && (
                        <p className="mt-1 text-sm text-red-600">{formErrors.nombre}</p>
                      )}
                    </div>

                    {/* Apellido */}
                    <div>
                      <label htmlFor="apellido" className="block text-sm font-medium text-gray-700">
                        Apellido *
                      </label>
                      <input
                        type="text"
                        name="apellido"
                        id="apellido"
                        value={formData.apellido}
                        onChange={handleInputChange}
                        className={`mt-1 block w-full border ${formErrors.apellido ? 'border-red-300' : 'border-slate-300'} rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm`}
                        placeholder="Pérez"
                      />
                      {formErrors.apellido && (
                        <p className="mt-1 text-sm text-red-600">{formErrors.apellido}</p>
                      )}
                    </div>

                    {/* Teléfono */}
                    <div>
                      <label htmlFor="telefono" className="block text-sm font-medium text-gray-700">
                        Teléfono *
                      </label>
                      <input
                        type="tel"
                        name="telefono"
                        id="telefono"
                        value={formData.telefono}
                        onChange={handleInputChange}
                        className={`mt-1 block w-full border ${formErrors.telefono ? 'border-red-300' : 'border-slate-300'} rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm`}
                        placeholder="+54 11 1234-5678"
                      />
                      {formErrors.telefono && (
                        <p className="mt-1 text-sm text-red-600">{formErrors.telefono}</p>
                      )}
                    </div>

                    {/* Correo */}
                    <div>
                      <label htmlFor="correo" className="block text-sm font-medium text-gray-700">
                        Correo Electrónico *
                      </label>
                      <input
                        type="email"
                        name="correo"
                        id="correo"
                        value={formData.correo}
                        onChange={handleInputChange}
                        className={`mt-1 block w-full border ${formErrors.correo ? 'border-red-300' : 'border-slate-300'} rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm`}
                        placeholder="cliente@ejemplo.com"
                      />
                      {formErrors.correo && (
                        <p className="mt-1 text-sm text-red-600">{formErrors.correo}</p>
                      )}
                    </div>

                    {/* Dirección */}
                    <div>
                      <label htmlFor="direccion" className="block text-sm font-medium text-gray-700">
                        Dirección *
                      </label>
                      <input
                        type="text"
                        name="direccion"
                        id="direccion"
                        value={formData.direccion}
                        onChange={handleInputChange}
                        className={`mt-1 block w-full border ${formErrors.direccion ? 'border-red-300' : 'border-slate-300'} rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm`}
                        placeholder="Calle 123, Ciudad"
                      />
                      {formErrors.direccion && (
                        <p className="mt-1 text-sm text-red-600">{formErrors.direccion}</p>
                      )}
                    </div>

                    {/* Condición IVA */}
                    <div>
                      <label htmlFor="idAfipCondicionIva" className="block text-sm font-medium text-gray-700">
                        Condición IVA *
                      </label>
                      <select
                        name="idAfipCondicionIva"
                        id="idAfipCondicionIva"
                        value={formData.idAfipCondicionIva}
                        onChange={handleInputChange}
                        className="mt-1 block w-full border border-slate-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                      >
                        {condicionesIva?.map((condicion) => (
                          <option key={condicion.id} value={condicion.id}>
                            {condicion.descripcion}
                          </option>
                        ))}
                      </select>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div className="bg-slate-50 border-t border-slate-100 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
              <button
                type="submit"
                disabled={submitting}
                className="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-blue-600 text-base font-medium text-white hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 sm:ml-3 sm:w-auto sm:text-sm disabled:opacity-50 disabled:cursor-not-allowed"
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
                  editingCliente ? 'Actualizar' : 'Crear'
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

export default ClienteModal
