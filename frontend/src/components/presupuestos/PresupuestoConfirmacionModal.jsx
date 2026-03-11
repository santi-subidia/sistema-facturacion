
import React from 'react';

const PresupuestoConfirmacionModal = ({
    isOpen,
    onClose,
    onConfirm,
    formData,
    clienteSeleccionado,
    tipoCliente,
    formasPago,
    condicionesVenta,
    esVentaEnNegro,
    setEsVentaEnNegro
}) => {
    if (!isOpen) return null;

    const formaPago = formasPago.find(f => f.id === formData.idFormaPago);
    const condicionVenta = condicionesVenta.find(c => c.id === formData.idCondicionVenta);

    const calcularTotal = () => {
        const subtotal = formData.detalles.reduce((total, detalle) => total + detalle.subtotal, 0);
        const ajuste = subtotal * (formData.porcentajeAjuste / 100);
        return subtotal + ajuste;
    };

    const calcularSubtotal = () => {
        return formData.detalles.reduce((total, detalle) => total + detalle.subtotal, 0);
    };

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
                                    Confirmar Presupuesto
                                </h3>
                                <div className="mt-4 border-t border-gray-200 py-4">

                                    {/* Cliente */}
                                    <div className="mb-4">
                                        <h4 className="text-sm font-bold text-gray-700 uppercase mb-2">Cliente</h4>
                                        {tipoCliente === "cliente_habitual" && clienteSeleccionado ? (
                                            <p className="text-sm text-gray-600">
                                                {clienteSeleccionado.nombre} {clienteSeleccionado.apellido} <br />
                                                {clienteSeleccionado.documento} <br />
                                                {clienteSeleccionado.direccion}
                                            </p>
                                        ) : tipoCliente === "consumidor_con_datos" && formData.clienteNombre ? (
                                            <p className="text-sm text-gray-600">
                                                {formData.clienteNombre} {formData.clienteApellido} <br />
                                                {formData.clienteDocumento} <br />
                                                {formData.clienteDireccion}
                                            </p>
                                        ) : (
                                            <p className="text-sm text-gray-600 font-italic">Consumidor Final</p>
                                        )}
                                    </div>

                                    {/* Datos de Presupuesto */}
                                    <div className="mb-4 grid grid-cols-2 gap-4">
                                        <div>
                                            <h4 className="text-sm font-bold text-gray-700 uppercase mb-1">Fecha</h4>
                                            <p className="text-sm text-gray-600">{formData.fecha || '-'}</p>
                                        </div>
                                        <div>
                                            <h4 className="text-sm font-bold text-gray-700 uppercase mb-1">Vencimiento</h4>
                                            <p className="text-sm text-gray-600">{formData.fechaVencimiento || 'Sin vencimiento'}</p>
                                        </div>
                                        <div>
                                            <h4 className="text-sm font-bold text-gray-700 uppercase mb-1">Condición Venta</h4>
                                            <p className="text-sm text-gray-600">{condicionVenta?.descripcion || '-'}</p>
                                        </div>
                                        <div>
                                            <h4 className="text-sm font-bold text-gray-700 uppercase mb-1">Forma Pago</h4>
                                            <p className="text-sm text-gray-600">{formaPago?.nombre || '-'}</p>
                                        </div>
                                        <div>
                                            <h4 className="text-sm font-bold text-gray-700 uppercase mb-1">Ajuste</h4>
                                            <p className="text-sm text-gray-600">{formData.porcentajeAjuste}%</p>
                                        </div>
                                    </div>

                                    {/* Detalles */}
                                    <div className="mt-4">
                                        <h4 className="text-sm font-bold text-gray-700 uppercase mb-2">Detalle</h4>
                                        <div className="max-h-60 overflow-y-auto border border-gray-200 rounded">
                                            <table className="min-w-full divide-y divide-gray-200">
                                                <thead className="bg-gray-50">
                                                    <tr>
                                                        <th className="px-4 py-2 text-left text-xs font-medium text-gray-500 uppercase">Producto</th>
                                                        <th className="px-4 py-2 text-right text-xs font-medium text-gray-500 uppercase">Cant.</th>
                                                        <th className="px-4 py-2 text-right text-xs font-medium text-gray-500 uppercase">Precio</th>
                                                        <th className="px-4 py-2 text-right text-xs font-medium text-gray-500 uppercase">Subtotal</th>
                                                    </tr>
                                                </thead>
                                                <tbody className="bg-white divide-y divide-gray-200">
                                                    {formData.detalles.map((detalle, index) => (
                                                        <tr key={index}>
                                                            <td className="px-4 py-2 text-sm text-gray-900">
                                                                {detalle.esGenerico ? detalle.productoNombre : detalle.producto.nombre}
                                                                {detalle.esGenerico && <span className="ml-1 text-xs text-gray-500">(Genérico)</span>}
                                                            </td>
                                                            <td className="px-4 py-2 text-sm text-gray-900 text-right">{detalle.cantidad}</td>
                                                            <td className="px-4 py-2 text-sm text-gray-900 text-right">
                                                                ${(detalle.precio * (1 + formData.porcentajeAjuste / 100)).toFixed(2)}
                                                            </td>
                                                            <td className="px-4 py-2 text-sm text-gray-900 text-right">
                                                                ${(detalle.subtotal * (1 + formData.porcentajeAjuste / 100)).toFixed(2)}
                                                            </td>
                                                        </tr>
                                                    ))}
                                                </tbody>
                                            </table>
                                        </div>
                                    </div>

                                    {/* Totales */}
                                    <div className="mt-6 flex justify-end">
                                        <div className="w-full sm:w-1/2 bg-gray-50 rounded-lg p-4 border border-gray-200">
                                            <div className="flex justify-between items-center">
                                                <span className="text-lg font-bold text-gray-900">TOTAL</span>
                                                <span className="text-2xl font-bold text-blue-600">${calcularTotal().toFixed(2)}</span>
                                            </div>
                                        </div>
                                    </div>

                                    {/* Es Venta en Negro (Switch) */}
                                    <div className="mt-6 p-4 bg-orange-50 border border-orange-200 rounded-md">
                                        <div className="flex items-center">
                                            <input
                                                id="esVentaEnNegro"
                                                name="esVentaEnNegro"
                                                type="checkbox"
                                                checked={esVentaEnNegro}
                                                onChange={(e) => setEsVentaEnNegro(e.target.checked)}
                                                className="h-5 w-5 text-orange-600 focus:ring-orange-500 border-gray-300 rounded"
                                            />
                                            <label htmlFor="esVentaEnNegro" className="ml-3 block text-sm font-medium text-gray-900">
                                                Registrar como Venta en Negro
                                            </label>
                                        </div>
                                        <p className="mt-1 text-xs text-gray-500 ml-8">
                                            Si marcas esta opción, se descontará el stock inmediatamente (priorizando stock en negro) y el presupuesto quedará registrado como venta.
                                        </p>
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
                            Confirmar y Crear
                        </button>
                        <button
                            type="button"
                            className="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm"
                            onClick={onClose}
                        >
                            Volver Atrás
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default PresupuestoConfirmacionModal;
