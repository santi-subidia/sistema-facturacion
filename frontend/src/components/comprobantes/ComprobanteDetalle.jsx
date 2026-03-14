
import React, { useState, useEffect } from 'react'
import { API_BASE_URL } from '../../config'
import { fetchWithAuth } from '../../utils/authHeaders'
import PdfViewer from '../shared/PdfViewer'

function ComprobanteDetalle({ show, comprobante: comprobanteProp, factura, onClose }) {
  const comprobante = comprobanteProp || factura;
  const [pdfUrl, setPdfUrl] = useState(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(null)
  const [comprobanteCompleto, setComprobanteCompleto] = useState(null) // Para tener datos del cliente para compartir

  useEffect(() => {
    if (show && comprobante) {
      fetchPdfAndData()
    } else {
      // Cleanup URL when modal closes
      if (pdfUrl) {
        window.URL.revokeObjectURL(pdfUrl)
        setPdfUrl(null)
      }
      setComprobanteCompleto(null)
    }
  }, [show, comprobante])

  const fetchPdfAndData = async () => {
    setLoading(true)
    setError(null)
    try {
      // 1. Fetch Comprobante Data (para obtener datos del cliente: tel, email)
      const id = comprobante.id || comprobante.Id
      const dataResponse = await fetchWithAuth(`${API_BASE_URL}/comprobantes/${id}`)
      if (dataResponse.ok) {
        const data = await dataResponse.json()
        setComprobanteCompleto(data)
      }

      // 2. Fetch PDF Blob
      const pdfResponse = await fetchWithAuth(`${API_BASE_URL}/comprobantes/${id}/pdf`)

      if (!pdfResponse.ok) {
        let errorMsg = 'Error al cargar el PDF del comprobante'
        try {
          const errorData = await pdfResponse.json()
          if (errorData.message) errorMsg = errorData.message
          if (errorData.Errors) errorMsg = Array.isArray(errorData.Errors) ? errorData.Errors.join(', ') : errorData.Errors
        } catch (e) {
          console.error('Error parsing error response:', e)
        }
        throw new Error(errorMsg)
      }

      const blob = await pdfResponse.blob()
      // Crear un blob con el nombre correcto del archivo
      const contentDisposition = pdfResponse.headers.get('content-disposition')
      let fileName = `Comprobante_${comprobante.id || comprobante.Id}.pdf`
      if (contentDisposition) {
        const fileNameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/)
        if (fileNameMatch && fileNameMatch[1]) {
          fileName = fileNameMatch[1].replace(/['"]/g, '')
        }
      }
      const blobWithName = new File([blob], fileName, { type: 'application/pdf' })
      const url = window.URL.createObjectURL(blobWithName)
      setPdfUrl(url)

    } catch (err) {
      console.error('Error fetching comprobante:', err)
      setError('No se pudo cargar la visualización del comprobante.')
    } finally {
      setLoading(false)
    }
  }

  const getClientContact = () => {
    // Intenta obtener telefono y correo del objeto comprobanteCompleto (ya sea cliente registrado o snapshot)
    const tel = comprobanteCompleto?.cliente?.telefono || comprobanteCompleto?.clienteTelefono
    const email = comprobanteCompleto?.cliente?.correo || comprobanteCompleto?.clienteCorreo
    return { tel, email }
  }

  const handleWhatsApp = () => {
    const { tel } = getClientContact()
    if (!tel) {
      alert('Esta factura no tiene un número de teléfono asociado.')
      return
    }

    // Limpiar numero (dejar solo digitos)
    const cleanPhone = tel.replace(/\D/g, '')
    const message = encodeURIComponent(`Hola, te envío adjunto la Factura #${comprobante?.id}. Saludos.`)
    window.open(`https://wa.me/${cleanPhone}?text=${message}`, '_blank')
  }

  const handleEmail = async () => {
    const { email } = getClientContact()
    if (!email) {
      alert('Esta factura no tiene un correo electrónico asociado al cliente para enviarla automáticamente.')
      return
    }

    if (!window.confirm(`¿Enviar la factura automáticamente a ${email}?`)) {
      return;
    }

    setLoading(true) // Reutilizamos el estado de loading
    try {
      const id = comprobante.id || comprobante.Id
      const response = await fetchWithAuth(`${API_BASE_URL}/comprobantes/${id}/enviar-correo`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ email })
      })

      const data = await response.json()

      if (response.ok) {
        alert('Correo enviado exitosamente!')
      } else {
        alert(`Error al enviar el correo: ${data.message || 'Error desconocido'}`)
      }
    } catch (err) {
      console.error('Error sending email:', err)
      alert('Error de red al intentar enviar el correo. Por favor, revisa la consola.')
    } finally {
      setLoading(false)
    }
  }

  const handleDownload = async () => {
    if (!comprobanteCompleto) return

    try {
      // Descargar directamente desde el backend para obtener el nombre correcto
      const id = comprobante.id || comprobante.Id
      const pdfResponse = await fetchWithAuth(`${API_BASE_URL}/comprobantes/${id}/pdf`)

      if (!pdfResponse.ok) throw new Error('Error al descargar PDF')

      const blob = await pdfResponse.blob()
      const url = window.URL.createObjectURL(blob)

      // Obtener el nombre del archivo desde el header Content-Disposition
      const contentDisposition = pdfResponse.headers.get('content-disposition')
      let fileName = `Comprobante_${String(comprobanteCompleto.puntoVenta || 0).padStart(5, '0')}_${String(comprobanteCompleto.numeroComprobante || 0).padStart(8, '0')}.pdf`

      if (contentDisposition) {
        const fileNameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/)
        if (fileNameMatch && fileNameMatch[1]) {
          fileName = fileNameMatch[1].replace(/['"]/g, '')
        }
      }

      const a = document.createElement('a')
      a.href = url
      a.download = fileName
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      window.URL.revokeObjectURL(url)
    } catch (err) {
      console.error('Error downloading PDF:', err)
      alert('Error al descargar el PDF')
    }
  }

  if (!show) return null

  return (
    <div className="fixed inset-0 z-50 overflow-hidden" aria-labelledby="modal-title" role="dialog" aria-modal="true">
      {/* Background overlay */}
      <div
        className="absolute inset-0 bg-gray-900 bg-opacity-75 transition-opacity"
        aria-hidden="true"
        onClick={onClose}
      ></div>

      {/* Modal Container */}
      <div className="absolute inset-0 flex items-center justify-center p-4 pointer-events-none">
        <div className="w-full h-full max-w-6xl max-h-[95vh] bg-white rounded-lg shadow-2xl flex flex-col pointer-events-auto overflow-hidden">

          {/* Header */}
          <div className="bg-gradient-to-r from-indigo-600 to-indigo-700 px-6 py-4 flex justify-between items-center shrink-0">
            <div className="flex items-center space-x-3">
              <div className="bg-white bg-opacity-20 p-2 rounded-lg">
                <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
              </div>
              <div>
                <h3 className="text-xl font-bold text-white">
                  Visualizar Factura #{comprobante?.id}
                </h3>
              </div>
            </div>
            <button
              onClick={onClose}
              className="text-white hover:text-gray-200 p-2 hover:bg-white hover:bg-opacity-20 rounded-lg transition-colors"
            >
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>

          {/* Content (PDF Preview) */}
          <div className="flex-1 bg-gray-100 relative overflow-hidden">
            {loading ? (
              <div className="absolute inset-0 flex items-center justify-center">
                <div className="flex flex-col items-center">
                  <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mb-4"></div>
                  <p className="text-gray-600 font-medium">Generando vista previa...</p>
                </div>
              </div>
            ) : error ? (
              <div className="absolute inset-0 flex items-center justify-center p-8 text-center">
                <div className="bg-red-50 p-6 rounded-xl border border-red-200 max-w-md">
                  <svg className="w-12 h-12 text-red-500 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  <h4 className="text-lg font-bold text-red-800 mb-2">Error</h4>
                  <p className="text-red-600">{error}</p>
                  <button
                    onClick={fetchPdfAndData}
                    className="mt-4 px-4 py-2 bg-white border border-red-300 text-red-700 rounded-md hover:bg-red-50 font-medium transition-colors"
                  >
                    Reintentar
                  </button>
                </div>
              </div>
            ) : (
              <PdfViewer url={pdfUrl} title="Vista Previa Factura" />
            )}
          </div>

          {/* Footer (Actions) */}
          <div className="bg-white border-t border-gray-200 px-6 py-4 shrink-0 flex flex-wrap justify-between items-center gap-4">
            <div className="text-sm text-gray-500 hidden md:block">
              * Para compartir, recuerda adjuntar el archivo descargado manualmente.
            </div>

            <div className="flex items-center space-x-3 w-full md:w-auto justify-end">
              {/* Email Share */}
              <button
                onClick={handleEmail}
                className="inline-flex items-center px-4 py-2 bg-blue-50 border border-blue-200 rounded-md text-blue-700 hover:bg-blue-100 hover:border-blue-300 transition-colors font-medium focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                title={getClientContact().email ? `Enviar a ${getClientContact().email}` : 'No hay email registrado'}
              >
                <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                </svg>
                Correo
              </button>

              {/* WhatsApp Share */}
              <button
                onClick={handleWhatsApp}
                className="inline-flex items-center px-4 py-2 bg-green-50 border border-green-200 rounded-md text-green-700 hover:bg-green-100 hover:border-green-300 transition-colors font-medium focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500"
                title={getClientContact().tel ? `Enviar a ${getClientContact().tel}` : 'No hay teléfono registrado'}
              >
                <svg className="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893a11.821 11.821 0 00-3.48-8.413z" />
                </svg>
                WhatsApp
              </button>

              <div className="h-6 w-px bg-gray-300 mx-2 hidden md:block"></div>

              {/* Download */}
              <button
                onClick={handleDownload}
                className="inline-flex items-center px-4 py-2 bg-indigo-600 border border-transparent rounded-md text-white hover:bg-indigo-700 transition-colors font-medium shadow-sm focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
              >
                <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
                </svg>
                Descargar
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default ComprobanteDetalle
