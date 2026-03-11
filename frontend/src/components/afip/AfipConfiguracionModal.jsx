import React, { useState, useEffect } from 'react'

function AfipConfiguracionModal({
  show,
  editingConfig,
  condicionesIva,
  onClose,
  onSubmit,
  submitting,
  isOnboarding = false
}) {
  const [formData, setFormData] = useState({
    cuit: '',
    razonSocial: '',
    idAfipCondicionIva: 1,
    inicioActividades: new Date().toISOString().split('T')[0],
    activa: true,
    esProduccion: false,
    certificado: null,
    certificadoPassword: ''
  })
  const [formErrors, setFormErrors] = useState({})

  useEffect(() => {
    if (editingConfig) {
      setFormData({
        cuit: editingConfig.cuit,
        razonSocial: editingConfig.razonSocial,
        idAfipCondicionIva: editingConfig.idAfipCondicionIva,
        inicioActividades: editingConfig.inicioActividades?.split('T')[0] || new Date().toISOString().split('T')[0],
        activa: editingConfig.activa,
        esProduccion: editingConfig.esProduccion || false,
        certificado: null, // No file selected by default
        certificadoPassword: '' // Password not populated for security
      })
    } else {
      setFormData({
        cuit: '',
        razonSocial: '',
        idAfipCondicionIva: 1,
        inicioActividades: new Date().toISOString().split('T')[0],
        activa: true,
        esProduccion: false,
        certificado: null,
        certificadoPassword: ''
      })
    }
    setFormErrors({})
  }, [editingConfig, show])

  const handleInputChange = (e) => {
    const { name, value, type, checked, files } = e.target
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : (type === 'file' ? files[0] : value)
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

    if (!formData.cuit.trim()) {
      errors.cuit = 'El CUIT es obligatorio.'
    } else if (!/^\d{11}$/.test(formData.cuit.replace(/-/g, ''))) {
      errors.cuit = 'El CUIT debe tener 11 dígitos numéricos.'
    }

    if (!formData.razonSocial.trim()) {
      errors.razonSocial = 'La Razón Social es obligatoria.'
    } else if (!/^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$/.test(formData.razonSocial)) {
      errors.razonSocial = 'La Razón Social solo puede contener caracteres alfabéticos.'
    }

    if (!formData.inicioActividades) {
      errors.inicioActividades = 'La fecha de inicio es obligatoria.'
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
    <div className="fixed z-50 inset-0 overflow-y-auto">
      <div className="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
        <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" onClick={onClose}></div>

        <span className="hidden sm:inline-block sm:align-middle sm:h-screen" aria-hidden="true">&#8203;</span>

        <div className="relative z-10 inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-3xl sm:w-full">
          <form onSubmit={handleSubmit}>
            <div className="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
              <div className="sm:flex sm:items-start">
                <div className="mx-auto flex-shrink-0 flex items-center justify-center h-12 w-12 rounded-full bg-indigo-100 sm:mx-0 sm:h-10 sm:w-10">
                  <svg className="h-6 w-6 text-indigo-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                  </svg>
                </div>
                <div className="mt-3 text-center sm:mt-0 sm:ml-4 sm:text-left w-full">
                  <h3 className="text-lg leading-6 font-medium text-gray-900 mb-4">
                    {isOnboarding ? 'Configuración Inicial AFIP' : (editingConfig ? 'Editar Configuración' : 'Nueva Configuración AFIP')}
                  </h3>

                  <div className="space-y-4">
                    {/* CUIT y Razón Social */}
                    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                      <div>
                        <label className="block text-sm font-medium text-gray-700">CUIT *</label>
                        <input
                          type="text"
                          name="cuit"
                          value={formData.cuit}
                          onChange={handleInputChange}
                          maxLength="13"
                          className={`mt-1 block w-full border ${formErrors.cuit ? 'border-red-300' : 'border-gray-300'} rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm`}
                          placeholder="20123456789"
                        />
                        {formErrors.cuit && (
                          <p className="mt-1 text-sm text-red-600">{formErrors.cuit}</p>
                        )}
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-700">Razón Social *</label>
                        <input
                          type="text"
                          name="razonSocial"
                          value={formData.razonSocial}
                          onChange={handleInputChange}
                          maxLength="200"
                          className={`mt-1 block w-full border ${formErrors.razonSocial ? 'border-red-300' : 'border-gray-300'} rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm`}
                        />
                        {formErrors.razonSocial && (
                          <p className="mt-1 text-sm text-red-600">{formErrors.razonSocial}</p>
                        )}
                      </div>
                    </div>

                    {/* Condición IVA */}
                    <div>
                      <label className="block text-sm font-medium text-gray-700">Condición IVA *</label>
                      <select
                        name="idAfipCondicionIva"
                        value={formData.idAfipCondicionIva}
                        onChange={handleInputChange}
                        className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                      >
                        {condicionesIva.map(condicion => (
                          <option key={condicion.id} value={condicion.id}>
                            {condicion.descripcion}
                          </option>
                        ))}
                      </select>
                    </div>

                    {/* Inicio Actividades */}
                    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                      <div>
                        <label className="block text-sm font-medium text-gray-700">Inicio de Actividades *</label>
                        <input
                          type="date"
                          name="inicioActividades"
                          value={formData.inicioActividades}
                          onChange={handleInputChange}
                          className={`mt-1 block w-full border ${formErrors.inicioActividades ? 'border-red-300' : 'border-gray-300'} rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm`}
                        />
                        {formErrors.inicioActividades && (
                          <p className="mt-1 text-sm text-red-600">{formErrors.inicioActividades}</p>
                        )}
                      </div>
                    </div>


                    {/* Certificado y Entorno */}
                    <div className="space-y-4 border-t border-gray-200 pt-4 mt-4">

                      {/* Es Producción */}
                      <div className="flex items-center">
                        <input
                          type="checkbox"
                          name="esProduccion"
                          id="esProduccion"
                          checked={formData.esProduccion}
                          onChange={handleInputChange}
                          className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                        />
                        <label htmlFor="esProduccion" className="ml-2 block text-sm font-medium text-gray-700">
                          Ambiente de Producción (Desmarcar para Homologación/Testing)
                        </label>
                      </div>

                      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                        <div>
                          <label className="block text-sm font-medium text-gray-700">Certificado (.pfx)</label>
                          <input
                            type="file"
                            name="certificado"
                            onChange={handleInputChange}
                            accept=".pfx"
                            className="mt-1 block w-full text-sm text-gray-500
                              file:mr-4 file:py-2 file:px-4
                              file:rounded-md file:border-0
                              file:text-sm file:font-semibold
                              file:bg-indigo-50 file:text-indigo-700
                              hover:file:bg-indigo-100"
                          />
                          {editingConfig && editingConfig.certificadoNombre && (
                            <p className="mt-1 text-xs text-gray-500">
                              Actual: {editingConfig.certificadoNombre}
                            </p>
                          )}
                        </div>
                        <div>
                          <label className="block text-sm font-medium text-gray-700">Contraseña del Certificado</label>
                          <input
                            type="password"
                            name="certificadoPassword"
                            value={formData.certificadoPassword}
                            onChange={handleInputChange}
                            placeholder={editingConfig?.hasPassword ? "•••••••• (Dejar vacío para mantener)" : ""}
                            className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                          />
                        </div>
                      </div>
                    </div>

                    {/* Checkbox Activa */}
                    <div className="space-y-2">
                      <div className="flex items-center">
                        <input
                          type="checkbox"
                          name="activa"
                          id="activa"
                          checked={formData.activa}
                          onChange={handleInputChange}
                          className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                        />
                        <label htmlFor="activa" className="ml-2 block text-sm text-gray-900">
                          Configuración Activa (desactiva las demás automáticamente)
                        </label>
                      </div>
                    </div>
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
                    <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    Guardando...
                  </>
                ) : (
                  editingConfig ? 'Actualizar' : 'Crear'
                )}
              </button>
              {!isOnboarding && (
                <button
                  type="button"
                  onClick={onClose}
                  disabled={submitting}
                  className="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Cancelar
                </button>
              )}
            </div>
          </form>
        </div>
      </div>
    </div>
  )
}

export default AfipConfiguracionModal
