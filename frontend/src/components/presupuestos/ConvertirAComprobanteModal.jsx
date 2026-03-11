import React, { useEffect, useState } from 'react'
import { API_BASE_URL } from '../../config'
import { fetchWithAuth } from '../../utils/authHeaders'

function ConvertirAComprobanteModal({ show, presupuesto, onClose, onConfirm }) {
    const [tiposComprobante, setTiposComprobante] = useState([])
    const [idTipoComprobante, setIdTipoComprobante] = useState('')
    const [loading, setLoading] = useState(false)
    const [loadingTipos, setLoadingTipos] = useState(false)
    const [error, setError] = useState(null)
    const [resultado, setResultado] = useState(null)

    useEffect(() => {
        if (show) {
            fetchTiposComprobante()
            setError(null)
            setResultado(null)
            setIdTipoComprobante('')
        }
    }, [show])

    const fetchTiposComprobante = async () => {
        try {
            setLoadingTipos(true)
            const response = await fetchWithAuth(`${API_BASE_URL}/tipocomprobante/habilitados`)
            if (!response.ok) throw new Error('Error al cargar tipos de comprobante')
            const data = await response.json()
            const tipos = data.data || data || []
            setTiposComprobante(tipos)
            if (tipos.length > 0) {
                setIdTipoComprobante(tipos[0].id)
            }
        } catch (err) {
            setError(err.message)
        } finally {
            setLoadingTipos(false)
        }
    }

    const handleConfirm = async () => {
        if (!idTipoComprobante) {
            setError('Debe seleccionar un tipo de comprobante')
            return
        }

        try {
            setLoading(true)
            setError(null)

            const result = await onConfirm(presupuesto.id, {
                idTipoComprobante: parseInt(idTipoComprobante)
            })

            setResultado(result)
        } catch (err) {
            setError(err.message)
        } finally {
            setLoading(false)
        }
    }

    if (!show || !presupuesto) return null

    const formatCurrency = (amount) => {
        return new Intl.NumberFormat('es-AR', {
            style: 'currency',
            currency: 'ARS'
        }).format(amount)
    }

    const clienteNombre = presupuesto.cliente
        ? `${presupuesto.cliente.nombre} ${presupuesto.cliente.apellido}`
        : presupuesto.clienteNombre
            ? `${presupuesto.clienteNombre} ${presupuesto.clienteApellido || ''}`
            : 'Consumidor Final'

    return (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full z-[60]" onClick={onClose}>
            <div className="relative top-20 mx-auto p-6 border w-full max-w-lg shadow-lg rounded-lg bg-white" onClick={(e) => e.stopPropagation()}>

                {/* Header */}
                <div className="flex items-center justify-between pb-4 border-b border-gray-200">
                    <div className="flex items-center gap-3">
                        <div className="flex-shrink-0 w-10 h-10 bg-green-100 rounded-full flex items-center justify-center">
                            <svg className="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                            </svg>
                        </div>
                        <div>
                            <h3 className="text-lg font-bold text-gray-900">Facturar Presupuesto</h3>
                            <p className="text-sm text-gray-500">#{presupuesto.numeroPresupuesto?.toString().padStart(6, '0') || presupuesto.id.toString().padStart(6, '0')}</p>
                        </div>
                    </div>
                    <button onClick={onClose} className="text-gray-400 hover:text-gray-500">
                        <svg className="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                </div>

                {/* Resultado exitoso */}
                {resultado ? (
                    <div className="mt-4 space-y-4">
                        <div className="bg-green-50 border border-green-200 rounded-lg p-4">
                            <div className="flex items-center gap-2 mb-2">
                                <svg className="w-5 h-5 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                                <h4 className="font-semibold text-green-800">¡Comprobante creado exitosamente!</h4>
                            </div>
                            <p className="text-sm text-green-700">{resultado.message}</p>
                            {resultado.comprobante && (
                                <div className="mt-3 space-y-1 text-sm text-green-700">
                                    {resultado.comprobante.cae && (
                                        <p><span className="font-medium">CAE:</span> {resultado.comprobante.cae}</p>
                                    )}
                                    {resultado.comprobante.numeroComprobante && (
                                        <p><span className="font-medium">N° Comprobante:</span> {resultado.comprobante.numeroComprobante}</p>
                                    )}
                                </div>
                            )}
                        </div>
                        <div className="flex justify-end">
                            <button
                                onClick={onClose}
                                className="px-4 py-2 bg-gray-600 text-white rounded-md hover:bg-gray-700 focus:outline-none focus:ring-2 focus:ring-gray-500"
                            >
                                Cerrar
                            </button>
                        </div>
                    </div>
                ) : (
                    <>
                        {/* Resumen del presupuesto */}
                        <div className="mt-4 bg-gray-50 rounded-lg p-4 space-y-2">
                            <div className="flex justify-between text-sm">
                                <span className="text-gray-500">Cliente:</span>
                                <span className="font-medium text-gray-900">{clienteNombre}</span>
                            </div>
                            <div className="flex justify-between text-sm">
                                <span className="text-gray-500">Forma de Pago:</span>
                                <span className="font-medium text-gray-900">{presupuesto.formaPago?.nombre || 'N/A'}</span>
                            </div>
                            <div className="flex justify-between text-sm">
                                <span className="text-gray-500">Condición de Venta:</span>
                                <span className="font-medium text-gray-900">{presupuesto.condicionVenta?.descripcion || 'N/A'}</span>
                            </div>
                            <div className="flex justify-between text-sm pt-2 border-t border-gray-200">
                                <span className="font-semibold text-gray-700">Total:</span>
                                <span className="font-bold text-gray-900 text-base">{formatCurrency(presupuesto.total)}</span>
                            </div>
                        </div>

                        {/* Selector de tipo de comprobante */}
                        <div className="mt-4">
                            <label className="block text-sm font-medium text-gray-700 mb-2">
                                Tipo de Comprobante <span className="text-red-500">*</span>
                            </label>
                            {loadingTipos ? (
                                <div className="flex items-center gap-2 py-2">
                                    <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-blue-600"></div>
                                    <span className="text-sm text-gray-500">Cargando tipos...</span>
                                </div>
                            ) : (
                                <select
                                    value={idTipoComprobante}
                                    onChange={(e) => setIdTipoComprobante(e.target.value)}
                                    className="w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 text-sm"
                                    disabled={loading}
                                >
                                    <option value="">Seleccione un tipo</option>
                                    {tiposComprobante.map((tipo) => (
                                        <option key={tipo.id} value={tipo.id}>
                                            {tipo.nombre} (Cód. AFIP: {tipo.codigoAfip})
                                        </option>
                                    ))}
                                </select>
                            )}
                        </div>

                        {/* Error */}
                        {error && (
                            <div className="mt-4 bg-red-50 border border-red-200 rounded-lg p-3">
                                <p className="text-sm text-red-700">{error}</p>
                            </div>
                        )}

                        {/* Advertencia */}
                        <div className="mt-4 bg-amber-50 border border-amber-200 rounded-lg p-3">
                            <div className="flex items-start gap-2">
                                <svg className="w-5 h-5 text-amber-600 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z" />
                                </svg>
                                <p className="text-sm text-amber-700">
                                    Esta acción generará un comprobante fiscal en AFIP. Una vez emitido, no podrá deshacerse.
                                </p>
                            </div>
                        </div>

                        {/* Actions */}
                        <div className="mt-6 flex justify-end gap-3">
                            <button
                                onClick={onClose}
                                disabled={loading}
                                className="px-4 py-2 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-gray-500 disabled:opacity-50"
                            >
                                Cancelar
                            </button>
                            <button
                                onClick={handleConfirm}
                                disabled={loading || !idTipoComprobante || loadingTipos}
                                className="inline-flex items-center px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 disabled:opacity-50 disabled:cursor-not-allowed"
                            >
                                {loading ? (
                                    <>
                                        <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
                                            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                        </svg>
                                        Procesando...
                                    </>
                                ) : (
                                    <>
                                        <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                        </svg>
                                        Confirmar Facturación
                                    </>
                                )}
                            </button>
                        </div>
                    </>
                )}
            </div>
        </div>
    )
}

export default ConvertirAComprobanteModal
