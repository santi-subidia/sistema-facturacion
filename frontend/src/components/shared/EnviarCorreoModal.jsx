import React, { useState, useEffect } from 'react'

function EnviarCorreoModal({ show, onClose, onSend, clienteEmail, loading, tipoDocumento = 'documento', success = false, sentEmail = '' }) {
  const [modo, setModo] = useState(clienteEmail ? 'cliente' : 'custom')
  const [customEmail, setCustomEmail] = useState('')
  const [emailError, setEmailError] = useState('')

  // Auto-close after success
  useEffect(() => {
    if (success && show) {
      const timer = setTimeout(() => {
        onClose()
      }, 2500)
      return () => clearTimeout(timer)
    }
  }, [success, show])

  // Reset state when modal opens/closes
  useEffect(() => {
    if (show) {
      setModo(clienteEmail ? 'cliente' : 'custom')
      setCustomEmail('')
      setEmailError('')
    }
  }, [show, clienteEmail])

  const validateEmail = (email) => {
    if (!email || !email.trim()) return 'Ingrese un correo electrónico'
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    if (!regex.test(email.trim())) return 'El formato del correo no es válido'
    return ''
  }

  const getSelectedEmail = () => {
    if (modo === 'cliente' && clienteEmail) return clienteEmail
    return customEmail.trim()
  }

  const handleSend = () => {
    const email = getSelectedEmail()
    const error = validateEmail(email)
    if (error) {
      setEmailError(error)
      return
    }
    setEmailError('')
    onSend(email)
  }

  const handleCustomEmailChange = (e) => {
    setCustomEmail(e.target.value)
    if (emailError) setEmailError('')
  }

  const handleKeyDown = (e) => {
    if (e.key === 'Enter' && !loading) {
      handleSend()
    }
  }

  if (!show) return null

  const label = tipoDocumento === 'factura' ? 'la factura' : 'el presupuesto'

  return (
    <div className="fixed inset-0 z-[70] overflow-y-auto" aria-labelledby="email-modal-title" role="dialog" aria-modal="true">
      {/* Overlay */}
      <div
        className="fixed inset-0 bg-slate-900/60 backdrop-blur-sm transition-opacity"
        aria-hidden="true"
        onClick={!loading ? onClose : undefined}
      ></div>

      {/* Modal */}
      <div className="flex min-h-full items-center justify-center p-4">
        <div className="relative w-full max-w-md bg-white rounded-xl shadow-2xl overflow-hidden transform transition-all">

          {/* Header */}
          <div className="bg-gradient-to-r from-blue-600 to-blue-700 px-6 py-4 flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="bg-white/20 p-2 rounded-lg">
                <svg className="w-5 h-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                </svg>
              </div>
              <h3 id="email-modal-title" className="text-lg font-bold text-white">
                Enviar por correo
              </h3>
            </div>
            <button
              onClick={onClose}
              disabled={loading}
              className="text-white/80 hover:text-white p-1.5 hover:bg-white/10 rounded-lg transition-colors disabled:opacity-50"
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          {/* Body */}
          <div className="px-6 py-5">
            {success ? (
              /* Vista de éxito */
              <div className="flex flex-col items-center py-4">
                <div className="w-16 h-16 bg-emerald-100 rounded-full flex items-center justify-center mb-4 animate-[scale-in_0.3s_ease-out]">
                  <svg className="w-8 h-8 text-emerald-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M5 13l4 4L19 7" />
                  </svg>
                </div>
                <h4 className="text-lg font-semibold text-slate-900 mb-1">¡Correo enviado!</h4>
                <p className="text-sm text-slate-500 text-center">
                  Se envió {label} exitosamente a
                </p>
                <p className="text-sm font-medium text-blue-600 mt-1">{sentEmail}</p>
                <p className="text-xs text-slate-400 mt-3">Este mensaje se cerrará automáticamente...</p>
              </div>
            ) : (
              <>
                <p className="text-sm text-slate-600 mb-4">
                  Seleccione a qué dirección de correo desea enviar {label}.
                </p>

                {/* Si hay email del cliente: mostrar radio buttons */}
                {clienteEmail ? (
                  <div className="space-y-3">
                    {/* Opción 1: Email del cliente */}
                    <label
                      className={`flex items-center gap-3 p-3 rounded-lg border-2 cursor-pointer transition-all ${
                        modo === 'cliente'
                          ? 'border-blue-500 bg-blue-50/50'
                          : 'border-slate-200 hover:border-slate-300 bg-white'
                      }`}
                    >
                      <input
                        type="radio"
                        name="emailOption"
                        value="cliente"
                        checked={modo === 'cliente'}
                        onChange={() => { setModo('cliente'); setEmailError('') }}
                        className="w-4 h-4 text-blue-600 focus:ring-blue-500"
                      />
                      <div className="flex-1 min-w-0">
                        <div className="text-sm font-medium text-slate-900">Email del cliente</div>
                        <div className="text-sm text-slate-500 truncate">{clienteEmail}</div>
                      </div>
                      <svg className="w-5 h-5 text-slate-400 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                      </svg>
                    </label>

                    {/* Opción 2: Otro email */}
                    <label
                      className={`flex items-start gap-3 p-3 rounded-lg border-2 cursor-pointer transition-all ${
                        modo === 'custom'
                          ? 'border-blue-500 bg-blue-50/50'
                          : 'border-slate-200 hover:border-slate-300 bg-white'
                      }`}
                    >
                      <input
                        type="radio"
                        name="emailOption"
                        value="custom"
                        checked={modo === 'custom'}
                        onChange={() => setModo('custom')}
                        className="w-4 h-4 text-blue-600 focus:ring-blue-500 mt-1"
                      />
                      <div className="flex-1">
                        <div className="text-sm font-medium text-slate-900 mb-2">Otro correo</div>
                        <input
                          type="email"
                          value={customEmail}
                          onChange={handleCustomEmailChange}
                          onFocus={() => setModo('custom')}
                          onKeyDown={handleKeyDown}
                          placeholder="ejemplo@correo.com"
                          disabled={loading}
                          className={`w-full px-3 py-2 text-sm border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors disabled:opacity-50 disabled:bg-slate-50 ${
                            emailError && modo === 'custom' ? 'border-red-300 bg-red-50' : 'border-slate-300'
                          }`}
                        />
                      </div>
                    </label>
                  </div>
                ) : (
                  /* Si NO hay email del cliente: solo campo de texto */
                  <div>
                    <div className="flex items-center gap-2 mb-3 p-3 bg-amber-50 border border-amber-200 rounded-lg">
                      <svg className="w-5 h-5 text-amber-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                      <p className="text-xs text-amber-700">
                        Este cliente no tiene un correo registrado. Ingrese uno a continuación.
                      </p>
                    </div>
                    <label className="block text-sm font-medium text-slate-700 mb-1.5">
                      Correo electrónico
                    </label>
                    <input
                      type="email"
                      value={customEmail}
                      onChange={handleCustomEmailChange}
                      onKeyDown={handleKeyDown}
                      placeholder="ejemplo@correo.com"
                      disabled={loading}
                      autoFocus
                      className={`w-full px-3 py-2.5 text-sm border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors disabled:opacity-50 disabled:bg-slate-50 ${
                        emailError ? 'border-red-300 bg-red-50' : 'border-slate-300'
                      }`}
                    />
                  </div>
                )}

                {/* Error message */}
                {emailError && (
                  <p className="mt-2 text-xs text-red-600 flex items-center gap-1">
                    <svg className="w-3.5 h-3.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    {emailError}
                  </p>
                )}
              </>
            )}
          </div>

          {/* Footer - only show when not in success state */}
          {!success && (
            <div className="bg-slate-50 px-6 py-4 flex justify-end gap-3 border-t border-slate-200">
              <button
                onClick={onClose}
                disabled={loading}
                className="px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-slate-500 transition-colors disabled:opacity-50"
              >
                Cancelar
              </button>
              <button
                onClick={handleSend}
                disabled={loading}
                className="inline-flex items-center px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 shadow-sm transition-colors disabled:opacity-70 disabled:cursor-not-allowed"
              >
                {loading ? (
                  <>
                    <svg className="animate-spin -ml-0.5 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    Enviando...
                  </>
                ) : (
                  <>
                    <svg className="w-4 h-4 mr-1.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8" />
                    </svg>
                    Enviar
                  </>
                )}
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}

export default EnviarCorreoModal
