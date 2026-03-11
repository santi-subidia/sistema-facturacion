import React, { useState, useEffect } from 'react';
import { useCaja } from '../../hooks/useCaja';

export default function AdministrarCajas() {
    const { cajas, puntosVentaAfip, fetchCajas, fetchPuntosVentaAfip, crearCaja, actualizarCaja, loading, error } = useCaja();
    const [isEditing, setIsEditing] = useState(false);
    const [currentCajaId, setCurrentCajaId] = useState(null);
    const [formData, setFormData] = useState({
        nombre: '',
        activa: true,
        puntoVenta: '' // changed to empty string initially to prompt selection
    });
    const [formError, setFormError] = useState(null);
    const [successMessage, setSuccessMessage] = useState(null);

    useEffect(() => {
        fetchCajas();
        fetchPuntosVentaAfip();
    }, [fetchCajas, fetchPuntosVentaAfip]);

    const handleEdit = (caja) => {
        setIsEditing(true);
        setCurrentCajaId(caja.id);
        setFormData({
            nombre: caja.nombre,
            activa: caja.activa,
            puntoVenta: caja.puntoVenta || ''
        });
        setFormError(null);
        setSuccessMessage(null);
    };

    const handleCancel = () => {
        setIsEditing(false);
        setCurrentCajaId(null);
        setFormData({
            nombre: '',
            activa: true,
            puntoVenta: ''
        });
        setFormError(null);
        setSuccessMessage(null);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setFormError(null);
        setSuccessMessage(null);

        if (!formData.nombre.trim()) {
            setFormError('El nombre es obligatorio');
            return;
        }

        if (!formData.puntoVenta) {
            setFormError('Debe seleccionar un punto de venta válido');
            return;
        }

        try {
            if (isEditing) {
                await actualizarCaja(currentCajaId, formData);
                setSuccessMessage('Caja actualizada exitosamente');
            } else {
                await crearCaja(formData);
                setSuccessMessage('Caja creada exitosamente');
            }
            handleCancel();
        } catch (err) {
            setFormError(err.message || 'Error al guardar la caja');
        }
    };

    return (
        <div className="bg-white shadow rounded-lg p-6">
            <h2 className="text-xl font-semibold mb-4 text-gray-800">Administrar Cajas y Puntos de Venta</h2>

            {error && (
                <div className="bg-red-50 border-l-4 border-red-400 p-4 mb-4 text-red-700">
                    <p>{error}</p>
                </div>
            )}

            {successMessage && (
                <div className="bg-green-50 border-l-4 border-green-400 p-4 mb-4 text-green-700">
                    <p>{successMessage}</p>
                </div>
            )}

            {formError && (
                <div className="bg-red-50 border-l-4 border-red-400 p-4 mb-4 text-red-700">
                    <p>{formError}</p>
                </div>
            )}

            <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                {/* Formulario */}
                <div>
                    <h3 className="text-lg font-medium text-gray-700 mb-3">
                        {isEditing ? 'Editar Caja' : 'Crear Nueva Caja'}
                    </h3>
                    <form onSubmit={handleSubmit} className="space-y-4">
                        <div>
                            <label className="block text-sm font-medium text-gray-700">Nombre de la Caja</label>
                            <input
                                type="text"
                                value={formData.nombre}
                                onChange={(e) => setFormData({ ...formData, nombre: e.target.value })}
                                className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm p-2 border"
                                placeholder="Ej: Caja Mostrador 1"
                                required
                            />
                        </div>

                        <div>
                            <label className="block text-sm font-medium text-gray-700">Punto de Venta (AFIP)</label>
                            <div className="mt-1 flex items-center">
                                <select
                                    value={formData.puntoVenta}
                                    onChange={(e) => setFormData({ ...formData, puntoVenta: e.target.value ? parseInt(e.target.value) : '' })}
                                    className="block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm p-2 border bg-white"
                                    required
                                >
                                    <option value="" disabled>Seleccione un punto de venta</option>
                                    {puntosVentaAfip.map((pv) => (
                                        <option key={pv.numero} value={pv.numero}>
                                            Nº {String(pv.numero).padStart(5, '0')} - Emisión: {pv.emisionTipo || 'Desconocido'} {pv.bloqueado === 'S' ? '(Bloqueado)' : ''}
                                        </option>
                                    ))}
                                </select>
                            </div>
                            <p className="mt-1 text-xs text-gray-500">
                                Las facturas generadas en esta sesión de caja usarán este punto de venta.
                            </p>
                        </div>

                        <div className="flex items-center">
                            <input
                                id="activa"
                                type="checkbox"
                                checked={formData.activa}
                                onChange={(e) => setFormData({ ...formData, activa: e.target.checked })}
                                className="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
                            />
                            <label htmlFor="activa" className="ml-2 block text-sm text-gray-900">
                                Caja Activa (habilita su apertura)
                            </label>
                        </div>

                        <div className="flex justify-end space-x-3 pt-2">
                            {isEditing && (
                                <button
                                    type="button"
                                    onClick={handleCancel}
                                    className="bg-white py-2 px-4 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none"
                                >
                                    Cancelar
                                </button>
                            )}
                            <button
                                type="submit"
                                disabled={loading}
                                className={`inline-flex justify-center py-2 px-4 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none ${loading ? 'opacity-50 cursor-not-allowed' : ''}`}
                            >
                                {loading ? 'Guardando...' : (isEditing ? 'Actualizar Caja' : 'Crear Caja')}
                            </button>
                        </div>
                    </form>
                </div>

                {/* Listado de Cajas */}
                <div>
                    <h3 className="text-lg font-medium text-gray-700 mb-3">Cajas Existentes</h3>
                    {loading && cajas.length === 0 ? (
                        <p className="text-gray-500 text-sm">Cargando cajas...</p>
                    ) : cajas.length === 0 ? (
                        <p className="text-gray-500 text-sm">No hay cajas creadas en el sistema.</p>
                    ) : (
                        <div className="overflow-hidden border border-gray-200 sm:rounded-md">
                            <ul role="list" className="divide-y divide-gray-200">
                                {cajas.map((caja) => (
                                    <li key={caja.id} className="px-4 py-4 sm:px-6 hover:bg-gray-50">
                                        <div className="flex items-center justify-between">
                                            <div className="flex flex-col">
                                                <p className="text-sm font-medium text-indigo-600 truncate">{caja.nombre}</p>
                                                <p className="flex items-center text-sm text-gray-500 mt-1">
                                                    Punto de Venta: <span className="ml-1 font-semibold">{String(caja.puntoVenta || 1).padStart(5, '0')}</span>
                                                </p>
                                            </div>
                                            <div className="flex flex-col items-end space-y-2">
                                                <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${caja.activa ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                                                    {caja.activa ? 'Activa' : 'Inactiva'}
                                                </span>
                                                <button
                                                    onClick={() => handleEdit(caja)}
                                                    className="text-xs text-indigo-600 hover:text-indigo-900 font-medium"
                                                >
                                                    Editar
                                                </button>
                                            </div>
                                        </div>
                                    </li>
                                ))}
                            </ul>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}
