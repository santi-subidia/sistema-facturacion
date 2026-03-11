import React from 'react';

const NotaCreditoConfirmacionModal = ({
    isOpen,
    onClose,
    onConfirm,
    detallesDevolucion,
    total,
    facturaOriginal,
    tipoComprobanteId,
    tiposComprobante
}) => {
    if (!isOpen) return null;

    const tipoComprobante = tiposComprobante.find(t => t.id === tipoComprobanteId);

    return (
        <div className="fixed inset-0 z-50 overflow-y-auto" aria-labelledby="modal-title" role="dialog" aria-modal="true">
            <div className="flex items-end justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
                <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" aria-hidden="true" onClick={onClose}></div>

                <span className="hidden sm:inline-block sm:align-middle sm:h-screen" aria-hidden="true">&#8203;</span>

                <div className="relative z-10 inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-2xl sm:w-full">
                    <div className="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
                        <div className="sm:flex sm:items-start">
                            <div className="mt-3 text-center sm:mt-0 sm:ml-4 sm:text-left w-full">
                                <h3 className="text-lg leading-6 font-medium text-gray-900" id="modal-title">
                                    Confirmar Nota de Crédito
                                </h3>
                                <div className="mt-4 border-t border-gray-200 py-4">

                                    {/* Context */}
                                    <div className="mb-4 grid grid-cols-2 gap-4">
                                        <div>
                                            <h4 className="text-sm font-bold text-gray-700 uppercase mb-1">Tipo Comprobante a Emitir</h4>
                                            <p className="text-sm text-gray-600">{tipoComprobante?.nombre || 'Nota de Crédito'}</p>
                                        </div>
                                        <div>
                                            <h4 className="text-sm font-bold text-gray-700 uppercase mb-1">Referencia Original</h4>
                                            <p className="text-sm text-gray-600">Factura {facturaOriginal.numeroComprobante || facturaOriginal.id}</p>
                                        </div>
                                    </div>

                                    {/* Detalles */}
                                    <div className="mt-4">
                                        <h4 className="text-sm font-bold text-gray-700 uppercase mb-2">Detalle de Devolución</h4>
                                        <div className="max-h-60 overflow-y-auto border border-gray-200 rounded">
                                            <table className="min-w-full divide-y divide-gray-200">
                                                <thead className="bg-gray-50">
                                                    <tr>
                                                        <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase">Producto</th>
                                                        <th className="px-4 py-2 text-right text-xs font-medium text-gray-500 uppercase">Cant. Devuelta</th>
                                                        <th className="px-4 py-2 text-right text-xs font-medium text-gray-500 uppercase">Subtotal Devuelto</th>
                                                    </tr>
                                                </thead>
                                                <tbody className="bg-white divide-y divide-gray-200">
                                                    {detallesDevolucion.map((detalle, index) => (
                                                        <tr key={index}>
                                                            <td className="px-4 py-2 text-sm text-gray-900">
                                                                {detalle.productoNombre || (detalle.producto ? detalle.producto.nombre : '')}
                                                            </td>
                                                            <td className="px-4 py-2 text-sm text-gray-900 text-right">{detalle.cantidadADevolver}</td>
                                                            <td className="px-4 py-2 text-sm text-gray-900 text-right">
                                                                ${(detalle.cantidadADevolver * (detalle.precioUnitario !== undefined ? detalle.precioUnitario : detalle.precio)).toFixed(2)}
                                                            </td>
                                                        </tr>
                                                    ))}
                                                </tbody>
                                            </table>
                                        </div>
                                    </div>

                                    {/* Totales */}
                                    <div className="mt-6 flex justify-end">
                                        <div className="w-full sm:w-1/2 bg-blue-50 rounded-lg p-4 border border-blue-200">
                                            <div className="flex justify-between items-center">
                                                <span className="text-lg font-bold text-gray-900">MONTO ACREDITADO</span>
                                                <span className="text-2xl font-bold text-blue-600">${total.toFixed(2)}</span>
                                            </div>
                                        </div>
                                    </div>

                                </div>
                            </div>
                        </div>
                    </div>
                    <div className="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
                        <button
                            type="button"
                            className="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-blue-600 text-base font-medium text-white hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 sm:ml-3 sm:w-auto sm:text-sm"
                            onClick={onConfirm}
                        >
                            Confirmar y Emitir
                        </button>
                        <button
                            type="button"
                            className="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm"
                            onClick={onClose}
                        >
                            Cancelar
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default NotaCreditoConfirmacionModal;
