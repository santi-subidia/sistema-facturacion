import React, { useState, useEffect } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { useComprobantes, useNotification } from './hooks/useComprobantes'
import Notification from './components/shared/Notification'
import NotaCreditoForm from './components/comprobantes/NotaCreditoForm'
import ComprobanteDetalle from './components/comprobantes/ComprobanteDetalle'
import { API_BASE_URL } from './config'
import { fetchWithAuth } from './utils/authHeaders'

function NotaCredito() {
    const location = useLocation()
    const navigate = useNavigate()
    const idFacturaOriginal = location.state?.idFacturaOriginal

    const {
        clientes,
        productos,
        tiposComprobante,
        formasPago,
        condicionesVenta,
        afipConfig,
        loading: loadingContext,
        error: errorContext,
        createComprobante,
        refreshData
    } = useComprobantes()

    const { notification, showNotification, hideNotification } = useNotification()
    const [submitting, setSubmitting] = useState(false)
    const [comprobanteCreado, setComprobanteCreado] = useState(null)
    const [showDetalle, setShowDetalle] = useState(false)

    const [facturaOriginal, setFacturaOriginal] = useState(null)
    const [loadingFactura, setLoadingFactura] = useState(false)

    useEffect(() => {
        if (!idFacturaOriginal) {
            navigate('/comprobantes/lista', { replace: true })
            return
        }

        const fetchFacturaOriginal = async () => {
            setLoadingFactura(true)
            try {
                const response = await fetchWithAuth(`${API_BASE_URL}/comprobantes/${idFacturaOriginal}/saldos`)
                if (!response.ok) {
                    throw new Error('Error al cargar la factura original')
                }
                const data = await response.json()
                // The endpoint returns { comprobante, detalles }
                // We merge them or just trust that backend will soon have navigation props, but let's merge manually to be safe.
                const fullFactura = {
                    ...data.comprobante,
                    detalles: data.detalles
                }
                setFacturaOriginal(fullFactura)
            } catch (err) {
                console.error(err)
                showNotification('error', 'No se pudo cargar la factura original para generar la Nota de Crédito.')
            } finally {
                setLoadingFactura(false)
            }
        }

        fetchFacturaOriginal()
    }, [idFacturaOriginal, navigate])

    const handleSubmit = async (comprobanteData) => {
        setSubmitting(true)

        try {
            const currentUser = JSON.parse(localStorage.getItem('user') || '{}')

            const comprobanteCompleto = {
                ...comprobanteData,
                idCreado_por: currentUser.id || 1,
                fecha: new Date().toISOString()
            }

            const resultado = await createComprobante(comprobanteCompleto)
            showNotification('success', resultado.message || 'Nota de Crédito generada con éxito')
            await refreshData()

            setComprobanteCreado(resultado.comprobante || resultado)
            setShowDetalle(true)
        } catch (err) {
            console.error('Error al crear Nota de Crédito:', err)
            let errorMessage = 'Error al crear la Nota de Crédito'
            if (err.message) errorMessage = err.message
            if (errorMessage.includes('AFIP')) errorMessage = `Error de AFIP: ${errorMessage}`
            showNotification('error', errorMessage)
        } finally {
            setSubmitting(false)
        }
    }

    const isLoading = loadingContext || loadingFactura

    if (isLoading) {
        return (
            <div className="flex flex-col justify-center items-center h-64">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mb-4"></div>
                <p className="text-gray-600 text-sm">Cargando datos para la Nota de Crédito...</p>
            </div>
        )
    }

    if (errorContext) {
        return (
            <div className="bg-red-50 border border-red-200 rounded-lg p-4">
                <p className="text-red-800">Error: {errorContext}</p>
            </div>
        )
    }

    return (
        <div className="space-y-6">
            <Notification
                notification={notification}
                onClose={hideNotification}
            />

            <div className="bg-white rounded-2xl shadow-sm border border-slate-200 overflow-hidden">
                <div className="px-6 py-4 border-b border-slate-200">
                    <h2 className="text-xl font-semibold text-slate-800">
                        Emitir Nota de Crédito
                    </h2>
                    <p className="text-sm text-slate-600 mt-1">
                        {facturaOriginal
                            ? `Generando devolución para la factura #${facturaOriginal.numeroComprobante || facturaOriginal.id}`
                            : 'Seleccione los productos a devolver'}
                    </p>
                </div>

                <div className="p-6">
                    {facturaOriginal && (
                        <NotaCreditoForm
                            facturaOriginal={facturaOriginal}
                            tiposComprobante={tiposComprobante}
                            onSubmit={handleSubmit}
                            submitting={submitting}
                            showNotification={showNotification}
                        />
                    )}
                </div>
            </div>

            <ComprobanteDetalle
                show={showDetalle}
                comprobante={comprobanteCreado}
                afipConfig={afipConfig}
                onClose={() => {
                    setShowDetalle(false)
                    setComprobanteCreado(null)
                    navigate('/comprobantes/lista') // Navigate back to list after closing
                }}
            />
        </div>
    )
}

export default NotaCredito
