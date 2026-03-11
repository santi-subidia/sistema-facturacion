import React, { useState, useEffect } from 'react'
import { useAfipConfiguracion, useNotification } from './hooks/useAfipConfiguracion'
import Notification from './components/shared/Notification'
import BackupConfiguracion from './components/configuracion/BackupConfiguracion'
import { useAuth } from './hooks/useAuth'

function AfipConfiguracion() {
  const {
    configuraciones,
    condicionesIva,
    configuracionActiva,
    loading,
    fetchConfiguraciones,
    createConfiguracion,
    updateConfiguracion
  } = useAfipConfiguracion()

  const { notification, showNotification, hideNotification } = useNotification()
  const { checkAfipConfig } = useAuth()

  const [submitting, setSubmitting] = useState(false)
  const [syncing, setSyncing] = useState(false)

  const [formData, setFormData] = useState({
    cuit: '',
    razonSocial: '',
    nombreFantasia: '',
    idAfipCondicionIva: 1,
    ingresosBrutosNumero: '',
    inicioActividades: new Date().toISOString().split('T')[0],
    limiteMontoConsumidorFinal: '',
    direccionFiscal: '',
    emailContacto: '',
    emailPassword: '',
    activa: true,
    esProduccion: false,
    certificado: null,
    certificadoPassword: '',
    logo: null,
    smtpHost: '',
    smtpPort: ''
  })
  const [formErrors, setFormErrors] = useState({})

  // Load active configuration into local state if it exists
  useEffect(() => {
    const config = configuracionActiva || (configuraciones.length > 0 ? configuraciones[0] : null)
    if (config) {
      setFormData({
        cuit: config.cuit || '',
        razonSocial: config.razonSocial || '',
        nombreFantasia: config.nombreFantasia || '',
        idAfipCondicionIva: config.idAfipCondicionIva || 1,
        ingresosBrutosNumero: config.ingresosBrutosNumero || '',
        inicioActividades: config.inicioActividades?.split('T')[0] || new Date().toISOString().split('T')[0],
        limiteMontoConsumidorFinal: config.limiteMontoConsumidorFinal || '',
        direccionFiscal: config.direccionFiscal || '',
        emailContacto: config.emailContacto || '',
        emailPassword: '',
        activa: config.activa !== undefined ? config.activa : true,
        esProduccion: config.esProduccion || false,
        certificado: null,
        certificadoPassword: '',
        logo: null,
        smtpHost: config.smtpHost || '',
        smtpPort: config.smtpPort || ''
      })
    }
    setFormErrors({})
  }, [configuracionActiva, configuraciones])

  const handleSincronizar = async () => {
    setSyncing(true)
    try {
      const res = await fetch('/api/afip/parametros/sincronizar', { method: 'POST' })
      const json = await res.json()
      if (json.success) {
        showNotification('success', json.message || '¡Parámetros sincronizados correctamente!')
      } else {
        showNotification('error', json.error || json.message || 'Error al sincronizar')
      }
    } catch (err) {
      showNotification('error', 'Error de conexión al sincronizar')
    } finally {
      setSyncing(false)
    }
  }

  const handleInputChange = (e) => {
    const { name, value, type, checked, files } = e.target
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : (type === 'file' ? files[0] : value)
    }))
    if (formErrors[name]) {
      setFormErrors(prev => ({ ...prev, [name]: '' }))
    }
  }

  const validateForm = () => {
    const errors = {}
    if (!formData.cuit?.trim()) {
      errors.cuit = 'El CUIT es obligatorio.'
    } else if (!/^\d{11}$/.test(formData.cuit.replace(/-/g, ''))) {
      errors.cuit = 'El CUIT debe tener 11 dígitos numéricos.'
    }
    if (!formData.razonSocial?.trim()) {
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

  const handleSubmit = async (e) => {
    e.preventDefault()
    if (!validateForm()) return

    setSubmitting(true)

    try {
      const existingConfig = configuracionActiva || (configuraciones.length > 0 ? configuraciones[0] : null)
      if (existingConfig) {
        await updateConfiguracion(existingConfig.id, formData)
        showNotification('success', '¡Configuración actualizada exitosamente!')
      } else {
        await createConfiguracion(formData)
        showNotification('success', '¡Configuración creada exitosamente!')
      }
      fetchConfiguraciones()
      checkAfipConfig()
    } catch (err) {
      console.error('Error en handleSubmit:', err)
      showNotification('error', err.message)
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    )
  }

  const existingConfig = configuracionActiva || (configuraciones.length > 0 ? configuraciones[0] : null)

  return (
    <div className="space-y-6">
      <Notification notification={notification} onClose={hideNotification} />

      <div className="bg-white rounded-2xl shadow-sm border border-slate-200 overflow-hidden">
        <div className="px-6 py-4 border-b border-slate-200 flex justify-between items-center bg-slate-50">
          <div>
            <h2 className="text-xl font-semibold text-slate-800">Configuración Única de AFIP</h2>
            <p className="mt-1 text-sm text-slate-500">Gestiona los parámetros de facturación electrónica de la cuenta.</p>
          </div>
          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={handleSincronizar}
              disabled={syncing}
              className="inline-flex items-center px-4 py-2 border border-slate-300 text-sm font-medium rounded-md shadow-sm text-slate-700 bg-white hover:bg-slate-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <svg className={`w-5 h-5 mr-2 ${syncing ? 'animate-spin' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
              </svg>
              {syncing ? 'Sincronizando...' : 'Sincronizar Parámetros AFIP'}
            </button>
          </div>
        </div>

        <form onSubmit={handleSubmit} className="px-6 py-6">
          <div className="space-y-6 max-w-4xl mx-auto">

            {/* CUIT y Razón Social */}
            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 bg-white p-4 border border-slate-100 rounded-md shadow-sm">
              <div>
                <label className="block text-sm font-medium text-slate-700">CUIT *</label>
                <input
                  type="text"
                  name="cuit"
                  value={formData.cuit}
                  onChange={handleInputChange}
                  maxLength="13"
                  className={`mt-1 block w-full border ${formErrors.cuit ? 'border-red-300' : 'border-slate-300'} rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm`}
                  placeholder="20123456789"
                />
                {formErrors.cuit && (
                  <p className="mt-1 text-sm text-red-600">{formErrors.cuit}</p>
                )}
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700">Razón Social *</label>
                <input
                  type="text"
                  name="razonSocial"
                  value={formData.razonSocial}
                  onChange={handleInputChange}
                  maxLength="200"
                  className={`mt-1 block w-full border ${formErrors.razonSocial ? 'border-red-300' : 'border-slate-300'} rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm`}
                />
                {formErrors.razonSocial && (
                  <p className="mt-1 text-sm text-red-600">{formErrors.razonSocial}</p>
                )}
              </div>
            </div>

            {/* Nombre Fantasía y Condición IVA */}
            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 bg-white p-4 border border-slate-100 rounded-md shadow-sm">
              <div>
                <label className="block text-sm font-medium text-slate-700">Nombre de Fantasía</label>
                <input
                  type="text"
                  name="nombreFantasia"
                  value={formData.nombreFantasia}
                  onChange={handleInputChange}
                  maxLength="100"
                  className="mt-1 block w-full border border-slate-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                  placeholder="Nombre comercial (opcional)"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700">Condición IVA *</label>
                <select
                  name="idAfipCondicionIva"
                  value={formData.idAfipCondicionIva}
                  onChange={handleInputChange}
                  className="mt-1 block w-full border border-slate-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                >
                  {condicionesIva.map(condicion => (
                    <option key={condicion.id} value={condicion.id}>
                      {condicion.descripcion}
                    </option>
                  ))}
                </select>
              </div>
            </div>



            {/* Ingresos Brutos y Límite Monto */}
            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 bg-white p-4 border border-slate-100 rounded-md shadow-sm">
              <div>
                <label className="block text-sm font-medium text-slate-700">N° Ingresos Brutos</label>
                <input
                  type="text"
                  name="ingresosBrutosNumero"
                  value={formData.ingresosBrutosNumero}
                  onChange={handleInputChange}
                  maxLength="50"
                  className="mt-1 block w-full border border-slate-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700">Límite Monto Consumidor Final</label>
                <div className="mt-1 relative rounded-md shadow-sm">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <span className="text-slate-500 sm:text-sm">$</span>
                  </div>
                  <input
                    type="number"
                    name="limiteMontoConsumidorFinal"
                    value={formData.limiteMontoConsumidorFinal}
                    onChange={handleInputChange}
                    step="0.01"
                    className="block w-full pl-7 pr-3 border border-slate-300 rounded-md shadow-sm py-2 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                    placeholder="Opcional"
                  />
                </div>
              </div>
            </div>

            {/* Inicio Actividades y Dirección */}
            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 bg-white p-4 border border-slate-100 rounded-md shadow-sm">
              <div>
                <label className="block text-sm font-medium text-slate-700">Inicio de Actividades *</label>
                <input
                  type="date"
                  name="inicioActividades"
                  value={formData.inicioActividades}
                  onChange={handleInputChange}
                  className={`mt-1 block w-full border ${formErrors.inicioActividades ? 'border-red-300' : 'border-slate-300'} rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm`}
                />
                {formErrors.inicioActividades && (
                  <p className="mt-1 text-sm text-red-600">{formErrors.inicioActividades}</p>
                )}
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700">Dirección Fiscal</label>
                <input
                  type="text"
                  name="direccionFiscal"
                  value={formData.direccionFiscal}
                  onChange={handleInputChange}
                  maxLength="200"
                  className="mt-1 block w-full border border-slate-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                />
              </div>
            </div>

            {/* Email y Contraseña */}
            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 bg-white p-4 border border-slate-100 rounded-md shadow-sm">
              <div>
                <label className="block text-sm font-medium text-slate-700">Email de Contacto</label>
                <input
                  type="email"
                  name="emailContacto"
                  value={formData.emailContacto}
                  onChange={handleInputChange}
                  maxLength="100"
                  className="mt-1 block w-full border border-slate-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700">Contraseña de Aplicación (Gmail)</label>
                <input
                  type="password"
                  name="emailPassword"
                  value={formData.emailPassword}
                  onChange={handleInputChange}
                  placeholder={existingConfig?.hasEmailPassword ? "•••••••• (Dejar vacío para mantener)" : "Generada en Seguridad de Google"}
                  className="mt-1 block w-full border border-slate-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                />
                <p className="mt-1 text-xs text-gray-500">
                  Necesaria para enviar comprobantes por correo automáticamente.
                </p>
              </div>
            </div>

            {/* Configuración SMTP Avanzada */}
            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 bg-slate-50 p-4 border border-slate-200 rounded-md shadow-sm">
              <div className="sm:col-span-2">
                <h3 className="text-sm font-bold text-slate-800 border-b border-slate-200 pb-2">Avanzado: Servidor de Correo (SMTP)</h3>
                <p className="mt-1 text-xs text-slate-500">
                  Si este recuadro se deja vacío, el sistema utilizará por defecto Gmail (`smtp.gmail.com:587`). Completa esto solo si usas servidores propios u Office365.
                </p>
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700">Host STMP</label>
                <input
                  type="text"
                  name="smtpHost"
                  value={formData.smtpHost}
                  onChange={handleInputChange}
                  placeholder="Ej: smtp.office365.com"
                  className="mt-1 block w-full border border-slate-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700">Puerto SMTP</label>
                <input
                  type="number"
                  name="smtpPort"
                  value={formData.smtpPort}
                  onChange={handleInputChange}
                  placeholder="Ej: 587"
                  className="mt-1 block w-full border border-slate-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                />
              </div>
            </div>

            {/* Archivos: Logo y Certificado */}
            <div className="grid grid-cols-1 gap-8 sm:grid-cols-2 bg-slate-50 p-6 border border-slate-200 rounded-md">
              <div className="bg-white p-4 shadow-sm border border-slate-100 rounded">
                <label className="block text-sm font-bold text-slate-700 mb-2">Logo de la Empresa</label>
                <input
                  type="file"
                  name="logo"
                  onChange={handleInputChange}
                  accept="image/png,image/jpeg,image/jpg"
                  className="block w-full text-sm text-slate-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100"
                />
                <p className="mt-2 text-xs text-slate-500">
                  Formatos aceptados: PNG, JPG, JPEG. (PDF de presupuestos)
                </p>
                {existingConfig?.logo_Url && (
                  <div className="mt-3 bg-slate-50 p-2 rounded">
                    <p className="text-xs text-slate-500 mb-2 font-medium">Logo actual:</p>
                    <img
                      src={existingConfig.logo_Url}
                      alt="Logo actual"
                      className="h-16 w-auto border border-slate-300 rounded bg-white p-1"
                      onError={(e) => { e.target.style.display = 'none' }}
                    />
                  </div>
                )}
              </div>

              <div className="bg-white p-4 shadow-sm border border-slate-100 rounded">
                <label className="block text-sm font-bold text-slate-700 mb-2">Certificado (.pfx)</label>
                <input
                  type="file"
                  name="certificado"
                  onChange={handleInputChange}
                  accept=".pfx"
                  className="block w-full text-sm text-slate-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100"
                />
                {existingConfig?.certificadoNombre && (
                  <p className="mt-2 text-xs font-medium text-slate-700 bg-slate-100 px-2 py-1 rounded inline-block">
                    Actual: {existingConfig.certificadoNombre}
                  </p>
                )}

                <div className="mt-4 pt-4 border-t border-slate-100">
                  <label className="block text-sm font-medium text-slate-700">Contraseña del Certificado</label>
                  <input
                    type="password"
                    name="certificadoPassword"
                    value={formData.certificadoPassword}
                    onChange={handleInputChange}
                    placeholder={existingConfig?.hasPassword ? "•••••••• (Dejar vacío para mantener)" : ""}
                    className="mt-1 block w-full border border-slate-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                  />
                </div>
              </div>
            </div>

            {/* Configuración del Entorno */}
            <div className="bg-orange-50 p-4 border border-orange-200 rounded-md">
              <div className="flex items-center">
                <input
                  type="checkbox"
                  name="esProduccion"
                  id="esProduccion"
                  checked={formData.esProduccion}
                  onChange={handleInputChange}
                  className="h-5 w-5 text-blue-600 focus:ring-blue-500 border-slate-300 rounded cursor-pointer"
                />
                <label htmlFor="esProduccion" className="ml-3 block text-sm font-bold text-slate-900 cursor-pointer">
                  Ambiente de Producción (Desmarcar para Entorno de Testing/Homologación)
                </label>
              </div>
              <p className="mt-2 text-xs text-slate-600 ml-8">
                Peligro: Cuando esta casilla está marcada, las facturas que emitas serán reales y enviadas a AFIP Producción.
              </p>
            </div>

            {/* Acciones */}
            <div className="flex justify-end pt-5 border-t border-slate-200">
              <button
                type="submit"
                disabled={submitting}
                className="inline-flex justify-center rounded-md border border-transparent shadow-sm px-6 py-3 bg-blue-600 text-base font-medium text-white hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 sm:text-sm disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {submitting ? (
                  <>
                    <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    Guardando Cambios...
                  </>
                ) : (
                  existingConfig ? 'Guardar Cambios de Configuración' : 'Registrar Configuración Inicial'
                )}
              </button>
            </div>

          </div>
        </form>
      </div>

      <BackupConfiguracion />
    </div>
  )
}

export default AfipConfiguracion
