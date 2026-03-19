import React, { useState, useEffect } from 'react'

function EnviarWhatsAppModal({ show, onClose, clienteTelefono, tipoDocumento = 'documento', documentoId }) {
  const [modo, setModo] = useState(clienteTelefono ? 'cliente' : 'custom')
  const [customPhone, setCustomPhone] = useState('')
  const [phoneError, setPhoneError] = useState('')

  // Reset state when modal opens
  useEffect(() => {
    if (show) {
      setModo(clienteTelefono ? 'cliente' : 'custom')
      setCustomPhone('')
      setPhoneError('')
    }
  }, [show, clienteTelefono])

  const validatePhone = (phone) => {
    if (!phone || !phone.trim()) return 'Ingrese un número de teléfono'
    const cleanPhone = phone.replace(/\D/g, '')
    if (cleanPhone.length < 8) return 'El número debe tener al menos 8 dígitos'
    return ''
  }

  const getSelectedPhone = () => {
    if (modo === 'cliente' && clienteTelefono) return clienteTelefono
    return customPhone.trim()
  }

  const handleSend = () => {
    const phone = getSelectedPhone()
    const error = validatePhone(phone)
    if (error) {
      setPhoneError(error)
      return
    }
    setPhoneError('')

    const cleanPhone = phone.replace(/\D/g, '')
    const label = tipoDocumento === 'factura' ? 'la Factura' : 'el Presupuesto'
    const message = encodeURIComponent(`Hola, te envío adjunto ${label} #${documentoId}. Saludos.`)
    window.open(`https://wa.me/${cleanPhone}?text=${message}`, '_blank')
    onClose()
  }

  const handlePhoneChange = (e) => {
    setCustomPhone(e.target.value)
    if (phoneError) setPhoneError('')
  }

  const handleKeyDown = (e) => {
    if (e.key === 'Enter') {
      handleSend()
    }
  }

  if (!show) return null

  const label = tipoDocumento === 'factura' ? 'la factura' : 'el presupuesto'

  return (
    <div className="fixed inset-0 z-[70] overflow-y-auto" aria-labelledby="whatsapp-modal-title" role="dialog" aria-modal="true">
      {/* Overlay */}
      <div
        className="fixed inset-0 bg-slate-900/60 backdrop-blur-sm transition-opacity"
        aria-hidden="true"
        onClick={onClose}
      ></div>

      {/* Modal */}
      <div className="flex min-h-full items-center justify-center p-4">
        <div className="relative w-full max-w-md bg-white rounded-xl shadow-2xl overflow-hidden transform transition-all">

          {/* Header */}
          <div className="bg-gradient-to-r from-emerald-600 to-emerald-700 px-6 py-4 flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="bg-white/20 p-2 rounded-lg">
                <svg className="w-5 h-5 text-white" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893a11.821 11.821 0 00-3.48-8.413z" />
                </svg>
              </div>
              <h3 id="whatsapp-modal-title" className="text-lg font-bold text-white">
                Enviar por WhatsApp
              </h3>
            </div>
            <button
              onClick={onClose}
              className="text-white/80 hover:text-white p-1.5 hover:bg-white/10 rounded-lg transition-colors"
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          {/* Body */}
          <div className="px-6 py-5">
            <p className="text-sm text-slate-600 mb-4">
              Seleccione a qué número desea enviar {label} por WhatsApp.
            </p>

            {clienteTelefono ? (
              <div className="space-y-3">
                {/* Opción 1: Teléfono del cliente */}
                <label
                  className={`flex items-center gap-3 p-3 rounded-lg border-2 cursor-pointer transition-all ${
                    modo === 'cliente'
                      ? 'border-emerald-500 bg-emerald-50/50'
                      : 'border-slate-200 hover:border-slate-300 bg-white'
                  }`}
                >
                  <input
                    type="radio"
                    name="phoneOption"
                    value="cliente"
                    checked={modo === 'cliente'}
                    onChange={() => { setModo('cliente'); setPhoneError('') }}
                    className="w-4 h-4 text-emerald-600 focus:ring-emerald-500"
                  />
                  <div className="flex-1 min-w-0">
                    <div className="text-sm font-medium text-slate-900">Teléfono del cliente</div>
                    <div className="text-sm text-slate-500 truncate">{clienteTelefono}</div>
                  </div>
                  <svg className="w-5 h-5 text-slate-400 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                  </svg>
                </label>

                {/* Opción 2: Otro número */}
                <label
                  className={`flex items-start gap-3 p-3 rounded-lg border-2 cursor-pointer transition-all ${
                    modo === 'custom'
                      ? 'border-emerald-500 bg-emerald-50/50'
                      : 'border-slate-200 hover:border-slate-300 bg-white'
                  }`}
                >
                  <input
                    type="radio"
                    name="phoneOption"
                    value="custom"
                    checked={modo === 'custom'}
                    onChange={() => setModo('custom')}
                    className="w-4 h-4 text-emerald-600 focus:ring-emerald-500 mt-1"
                  />
                  <div className="flex-1">
                    <div className="text-sm font-medium text-slate-900 mb-2">Otro número</div>
                    <input
                      type="tel"
                      value={customPhone}
                      onChange={handlePhoneChange}
                      onFocus={() => setModo('custom')}
                      onKeyDown={handleKeyDown}
                      placeholder="Ej: 5491123456789"
                      className={`w-full px-3 py-2 text-sm border rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors ${
                        phoneError && modo === 'custom' ? 'border-red-300 bg-red-50' : 'border-slate-300'
                      }`}
                    />
                    <p className="text-xs text-slate-400 mt-1">Incluir código de país sin el +</p>
                  </div>
                </label>
              </div>
            ) : (
              <div>
                <div className="flex items-center gap-2 mb-3 p-3 bg-amber-50 border border-amber-200 rounded-lg">
                  <svg className="w-5 h-5 text-amber-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <p className="text-xs text-amber-700">
                    Este cliente no tiene un teléfono registrado. Ingrese uno a continuación.
                  </p>
                </div>
                <label className="block text-sm font-medium text-slate-700 mb-1.5">
                  Número de teléfono
                </label>
                <input
                  type="tel"
                  value={customPhone}
                  onChange={handlePhoneChange}
                  onKeyDown={handleKeyDown}
                  placeholder="Ej: 5491123456789"
                  autoFocus
                  className={`w-full px-3 py-2.5 text-sm border rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 transition-colors ${
                    phoneError ? 'border-red-300 bg-red-50' : 'border-slate-300'
                  }`}
                />
                <p className="text-xs text-slate-400 mt-1">Incluir código de país sin el +</p>
              </div>
            )}

            {/* Error message */}
            {phoneError && (
              <p className="mt-2 text-xs text-red-600 flex items-center gap-1">
                <svg className="w-3.5 h-3.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                {phoneError}
              </p>
            )}
          </div>

          {/* Footer */}
          <div className="bg-slate-50 px-6 py-4 flex justify-end gap-3 border-t border-slate-200">
            <button
              onClick={onClose}
              className="px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-slate-500 transition-colors"
            >
              Cancelar
            </button>
            <button
              onClick={handleSend}
              className="inline-flex items-center px-4 py-2 text-sm font-medium text-white bg-emerald-600 border border-transparent rounded-lg hover:bg-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-500 shadow-sm transition-colors"
            >
              <svg className="w-4 h-4 mr-1.5" fill="currentColor" viewBox="0 0 24 24">
                <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893a11.821 11.821 0 00-3.48-8.413z" />
              </svg>
              Abrir WhatsApp
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}

export default EnviarWhatsAppModal
