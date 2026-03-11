import React, { useState, useEffect } from 'react'
import NotaCreditoConfirmacionModal from './NotaCreditoConfirmacionModal'

function NotaCreditoForm({
    facturaOriginal,
    tiposComprobante,
    onSubmit,
    submitting,
    showNotification
}) {
    const [formData, setFormData] = useState({
        idCliente: facturaOriginal.idCliente || '',
        idTipoComprobante: '',
        idCondicionVenta: facturaOriginal.idCondicionVenta || '',
        idFormaPago: facturaOriginal.idFormaPago || '',
        porcentajeAjuste: facturaOriginal.porcentajeAjuste || 0,
        detalles: []
    })

    const [detallesDevolucion, setDetallesDevolucion] = useState([])
    const [showConfirmModal, setShowConfirmModal] = useState(false)

    // Initialize return items based on original invoice
    useEffect(() => {
        if (facturaOriginal && facturaOriginal.detalles) {
            const initialDetalles = facturaOriginal.detalles.map(d => ({
                ...d,
                cantidadADevolver: 0, // Default to 0, user must explicitly choose what to return
                maxCantidad: d.cantidad
            }))
            setDetallesDevolucion(initialDetalles)
        }
    }, [facturaOriginal])

    // Automatically select the correct Credit Note type based on original Invoice type
    useEffect(() => {
        if (facturaOriginal && facturaOriginal.tipoComprobante && tiposComprobante) {
            const originalName = facturaOriginal.tipoComprobante.nombre || ''
            if (originalName.toLowerCase().includes('factura')) {
                const isA = originalName.includes('A')
                const isB = originalName.includes('B')
                const isC = originalName.includes('C')

                const ncTipo = tiposComprobante.find(tc =>
                    tc.nombre.toLowerCase().includes('nota de') &&
                    ((isA && tc.nombre.includes('A')) ||
                        (isB && tc.nombre.includes('B')) ||
                        (isC && tc.nombre.includes('C')))
                )
                if (ncTipo) {
                    setFormData(prev => ({ ...prev, idTipoComprobante: ncTipo.id }))
                }
            }
        }
    }, [facturaOriginal, tiposComprobante])

    const handleCantidadChange = (index, value) => {
        const cantidad = parseFloat(value) || 0
        const detalle = detallesDevolucion[index]

        if (cantidad > detalle.maxCantidad) {
            showNotification('error', `Cantidad no puede superar la factura original (${detalle.maxCantidad})`)
            return
        }

        const nuevosDetalles = [...detallesDevolucion]
        nuevosDetalles[index].cantidadADevolver = cantidad >= 0 ? cantidad : 0
        setDetallesDevolucion(nuevosDetalles)
    }

    const calcularTotal = () => {
        return detallesDevolucion.reduce((acc, d) => {
            const precioACobrar = d.precioUnitario !== undefined ? d.precioUnitario : d.precio;
            return acc + (d.cantidadADevolver * precioACobrar);
        }, 0)
    }

    const hasItemsToReturn = detallesDevolucion.some(d => d.cantidadADevolver > 0)

    const handlePreSubmit = (e) => {
        e.preventDefault()
        if (!hasItemsToReturn) {
            showNotification('error', 'Debe seleccionar al menos un producto para devolver y especificar su cantidad.')
            return
        }
        if (!formData.idTipoComprobante) {
            showNotification('error', 'No se ha podido determinar el tipo de Nota de Crédito correspondiente.')
            return
        }
        setShowConfirmModal(true)
    }

    const handleConfirmSubmit = () => {
        setShowConfirmModal(false)

        // Filter out items with 0 quantity and map to API format
        const detallesFinales = detallesDevolucion
            .filter(d => d.cantidadADevolver > 0)
            .map(d => ({
                idProducto: d.idProducto,
                productoNombre: d.productoNombre || (d.producto ? d.producto.nombre : ''),
                productoCodigo: d.productoCodigo || (d.producto ? d.producto.codigo : ''),
                cantidad: d.cantidadADevolver,
                precio: d.precio,
            }))

        const payload = {
            ...formData,
            idCliente: formData.idCliente === '' ? null : parseInt(formData.idCliente, 10),
            detalles: detallesFinales,
            comprobantesAsociados: [{
                tipo: parseInt(facturaOriginal.tipoComprobante ? facturaOriginal.tipoComprobante.codigoAfip : facturaOriginal.idTipoComprobante, 10),
                ptoVta: parseInt(facturaOriginal.puntoVenta || 1, 10),
                nro: parseInt(facturaOriginal.numeroComprobante || facturaOriginal.id, 10)
            }]
        }

        onSubmit(payload)
    }

    // Derived client name for display
    const clienteDisplay = facturaOriginal.clienteNombre
        ? `${facturaOriginal.clienteNombre} ${facturaOriginal.clienteApellido}`
        : (facturaOriginal.cliente ? `${facturaOriginal.cliente.nombre} ${facturaOriginal.cliente.apellido}` : 'Consumidor Final')

    return (
        <div className="space-y-6">
            <div className="bg-blue-50 border-l-4 border-blue-500 p-4 rounded-md shadow-sm">
                <h3 className="text-sm font-medium text-blue-800">
                    Contexto de Devolución
                </h3>
                <p className="mt-2 text-sm text-blue-700">
                    Cliente: <strong>{clienteDisplay}</strong><br />
                    Factura Original: <strong>{facturaOriginal.numeroComprobante || facturaOriginal.id}</strong><br />
                    Método de Pago: <strong>{facturaOriginal.formaPago?.nombre || '-'}</strong>
                </p>
            </div>

            <div className="bg-white border border-gray-200 rounded-lg overflow-hidden">
                <table className="min-w-full divide-y divide-gray-200">
                    <thead className="bg-gray-50">
                        <tr>
                            <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Producto</th>
                            <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Cant. Original</th>
                            <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Precio Unit.</th>
                            <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">A Devolver</th>
                            <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Subtotal Dev.</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-200">
                        {detallesDevolucion.map((detalle, idx) => (
                            <tr key={idx} className={detalle.cantidadADevolver > 0 ? 'bg-blue-50/30' : ''}>
                                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                                    {detalle.productoNombre || (detalle.producto ? detalle.producto.nombre : '')}
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 text-right">
                                    {detalle.maxCantidad}
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500 text-right">
                                    ${(detalle.precioUnitario !== undefined ? detalle.precioUnitario : detalle.precio).toFixed(2)}
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900 text-right">
                                    <input
                                        type="number"
                                        min="0"
                                        max={detalle.maxCantidad}
                                        value={detalle.cantidadADevolver}
                                        onChange={(e) => handleCantidadChange(idx, e.target.value)}
                                        className="w-24 text-right px-2 py-1 border border-gray-300 rounded focus:ring-blue-500 focus:border-blue-500 disabled:opacity-50"
                                        disabled={submitting}
                                    />
                                </td>
                                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900 text-right">
                                    ${(detalle.cantidadADevolver * (detalle.precioUnitario !== undefined ? detalle.precioUnitario : detalle.precio)).toFixed(2)}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            <div className="flex justify-end pt-4 border-t border-gray-200">
                <div className="w-full md:w-1/3 space-y-3">
                    <div className="flex justify-between text-lg font-bold">
                        <span>TOTAL DEVOLUCIÓN:</span>
                        <span className="text-blue-600">${calcularTotal().toFixed(2)}</span>
                    </div>
                    <button
                        type="button"
                        onClick={handlePreSubmit}
                        disabled={submitting || !hasItemsToReturn}
                        className="w-full px-4 py-3 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:bg-gray-400 font-medium transition-colors"
                    >
                        {submitting ? 'Procesando...' : 'Revisar y Emitir NC'}
                    </button>
                </div>
            </div>

            <NotaCreditoConfirmacionModal
                isOpen={showConfirmModal}
                onClose={() => setShowConfirmModal(false)}
                onConfirm={handleConfirmSubmit}
                detallesDevolucion={detallesDevolucion.filter(d => d.cantidadADevolver > 0)}
                total={calcularTotal()}
                facturaOriginal={facturaOriginal}
                tipoComprobanteId={formData.idTipoComprobante}
                tiposComprobante={tiposComprobante}
            />
        </div>
    )
}

export default NotaCreditoForm
