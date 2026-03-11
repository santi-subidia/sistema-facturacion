import React, { useState, useEffect } from 'react';
import { useCaja } from '../../hooks/useCaja';

export default function MovimientosCaja({ sesionActiva }) {
    const { obtenerMovimientos, agregarMovimiento, movimientos, loading, error } = useCaja();
    const [tipo, setTipo] = useState('Ingreso');
    const [monto, setMonto] = useState('');
    const [concepto, setConcepto] = useState('');
    const [localError, setLocalError] = useState('');

    useEffect(() => {
        if (sesionActiva) {
            obtenerMovimientos(sesionActiva.id);
        }
    }, [sesionActiva, obtenerMovimientos]);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLocalError('');
        if (monto === '' || isNaN(monto) || Number(monto) <= 0) {
            setLocalError('Debe ingresar un monto válido mayor a 0.');
            return;
        }
        if (!concepto.trim()) {
            setLocalError('El concepto es obligatorio.');
            return;
        }

        try {
            await agregarMovimiento(sesionActiva.id, tipo, Number(monto), concepto);
            setMonto('');
            setConcepto('');
            setTipo('Ingreso');
            obtenerMovimientos(sesionActiva.id);
        } catch (err) {
            setLocalError(err.message || 'Error al agregar el movimiento.');
        }
    };

    return (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mt-4">
            {/* Formulario */}
            <div className="md:col-span-1 bg-white p-6 rounded-lg shadow-sm border border-gray-200">
                <h3 className="text-lg font-semibold mb-4 text-gray-800">Registrar Movimiento</h3>

                {(error || localError) && (
                    <div className="mb-4 bg-red-50 p-3 rounded-md border border-red-200 text-red-600 text-sm">
                        {error || localError}
                    </div>
                )}

                <form onSubmit={handleSubmit}>
                    <div className="mb-4">
                        <label className="block text-sm font-medium text-gray-700 mb-1">Tipo de Movimiento</label>
                        <div className="flex space-x-4">
                            <label className="inline-flex items-center">
                                <input type="radio" className="form-radio text-slate-600" value="Ingreso" checked={tipo === 'Ingreso'} onChange={(e) => setTipo(e.target.value)} />
                                <span className="ml-2 text-sm text-gray-700">Ingreso</span>
                            </label>
                            <label className="inline-flex items-center">
                                <input type="radio" className="form-radio text-red-600" value="Egreso" checked={tipo === 'Egreso'} onChange={(e) => setTipo(e.target.value)} />
                                <span className="ml-2 text-sm text-gray-700">Egreso</span>
                            </label>
                        </div>
                    </div>

                    <div className="mb-4">
                        <label className="block text-sm font-medium text-gray-700 mb-1">Monto</label>
                        <div className="relative">
                            <span className="absolute inset-y-0 left-0 pl-3 flex items-center text-gray-500">$</span>
                            <input
                                type="number"
                                step="0.01"
                                min="0.01"
                                value={monto}
                                onChange={(e) => setMonto(e.target.value)}
                                className="w-full text-slate-800 pl-8 pr-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-slate-500 focus:border-slate-500"
                                placeholder="0.00"
                                required
                                disabled={loading}
                            />
                        </div>
                    </div>

                    <div className="mb-6">
                        <label className="block text-sm font-medium text-gray-700 mb-1">Concepto</label>
                        <input
                            type="text"
                            value={concepto}
                            onChange={(e) => setConcepto(e.target.value)}
                            className="w-full text-slate-800 p-2 border border-gray-300 rounded-md shadow-sm focus:ring-slate-500 focus:border-slate-500"
                            placeholder="Ej. Pago a proveedor"
                            required
                            disabled={loading}
                        />
                    </div>

                    <button
                        type="submit"
                        className="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-slate-600 hover:bg-slate-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-slate-500 disabled:opacity-50"
                        disabled={loading}
                    >
                        {loading ? 'Guardando...' : 'Registrar'}
                    </button>
                </form>
            </div>

            {/* Historial */}
            <div className="md:col-span-2 bg-white p-6 rounded-lg shadow-sm border border-gray-200 flex flex-col">
                <h3 className="text-lg font-semibold mb-4 text-gray-800">Historial de Movimientos</h3>

                <div className="flex-1 overflow-auto">
                    {movimientos.length === 0 ? (
                        <p className="text-gray-500 text-sm italic">No hay movimientos registrados en esta sesión.</p>
                    ) : (
                        <table className="min-w-full divide-y divide-gray-200">
                            <thead className="bg-gray-50">
                                <tr>
                                    <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Fecha Hora</th>
                                    <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Tipo</th>
                                    <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Concepto</th>
                                    <th scope="col" className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Monto</th>
                                </tr>
                            </thead>
                            <tbody className="bg-white divide-y divide-gray-200">
                                {movimientos.map((m) => (
                                    <tr key={m.id}>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                            {new Date(m.fecha).toLocaleString()}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${m.tipo === 'Ingreso' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                                                {m.tipo}
                                            </span>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900 truncate max-w-xs" title={m.concepto}>
                                            {m.concepto}
                                        </td>
                                        <td className={`px-6 py-4 whitespace-nowrap text-sm text-right font-medium ${m.tipo === 'Ingreso' ? 'text-green-600' : 'text-red-600'}`}>
                                            {m.tipo === 'Ingreso' ? '+' : '-'}${m.monto.toFixed(2)}
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    )}
                </div>
            </div>
        </div>
    );
}
