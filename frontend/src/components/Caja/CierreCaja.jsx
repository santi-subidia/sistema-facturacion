import React, { useState, useEffect } from 'react';
import { useCaja } from '../../hooks/useCaja';
import { useConfirm } from '../../context/ConfirmContext';

export default function CierreCaja({ sesionActiva, onCierreExitoso }) {
    const { calcularArqueo, cerrarCaja, loading, error } = useCaja();
    const { confirm } = useConfirm();
    const [montoCierreReal, setMontoCierreReal] = useState('');
    const [montoSistema, setMontoSistema] = useState(null);
    const [localError, setLocalError] = useState('');

    useEffect(() => {
        if (sesionActiva) {
            cargarArqueo();
        }
    }, [sesionActiva]);

    const cargarArqueo = async () => {
        try {
            const arqueo = await calcularArqueo(sesionActiva.id);
            setMontoSistema(arqueo);
        } catch (err) {
            setLocalError('No se pudo calcular el arqueo del sistema.');
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLocalError('');
        if (montoCierreReal === '' || isNaN(montoCierreReal) || Number(montoCierreReal) < 0) {
            setLocalError('Debe ingresar el monto real en caja.');
            return;
        }

        // Confirmación personalizada
        const isConfirmed = await confirm({
            title: 'Cerrar Caja',
            message: '¿Está seguro que desea cerrar la caja? Esta acción no se puede deshacer.',
            confirmText: 'Cerrar Caja',
            type: 'danger'
        });

        if (!isConfirmed) return;

        try {
            await cerrarCaja(sesionActiva.id, Number(montoCierreReal));
            if (onCierreExitoso) onCierreExitoso();
        } catch (err) {
            setLocalError(err.message || 'Error al cerrar la caja');
        }
    };

    const diferencia = montoSistema !== null && montoCierreReal !== ''
        ? Number(montoCierreReal) - montoSistema
        : 0;

    return (
        <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200 mt-4 max-w-lg mx-auto">
            <h2 className="text-xl font-semibold mb-4 text-gray-800">Cierre de Caja</h2>

            <div className="mb-4 bg-slate-50 p-4 rounded border border-slate-200">
                <p className="text-sm text-slate-600 mb-1">Caja: <span className="font-semibold text-slate-800">{sesionActiva?.cajaNombre}</span></p>
                <p className="text-sm text-slate-600 mb-1">Fecha de Apertura: <span className="font-semibold text-slate-800">{new Date(sesionActiva?.fechaApertura).toLocaleString()}</span></p>
                <p className="text-sm text-slate-600">Monto Inicial: <span className="font-semibold text-slate-800">${sesionActiva?.montoApertura?.toFixed(2)}</span></p>
            </div>

            {(error || localError) && (
                <div className="mb-4 bg-red-50 p-3 rounded-md border border-red-200 text-red-600 text-sm">
                    {error || localError}
                </div>
            )}

            {montoSistema !== null ? (
                <div className="mb-4 p-4 rounded-lg flex justify-between items-center bg-blue-50 border border-blue-100">
                    <span className="text-blue-800 font-medium">Monto Esperado (Sistema):</span>
                    <span className="text-2xl font-bold text-blue-900">${montoSistema.toFixed(2)}</span>
                </div>
            ) : (
                <div className="mb-4 text-sm text-gray-500">Calculando montos esperados...</div>
            )}

            <form onSubmit={handleSubmit}>
                <div className="mb-4">
                    <label className="block text-sm font-medium text-gray-700 mb-1">Monto Real en Caja (Efectivo y otros)</label>
                    <div className="relative">
                        <span className="absolute inset-y-0 left-0 pl-3 flex items-center text-gray-500">$</span>
                        <input
                            type="number"
                            step="0.01"
                            min="0"
                            value={montoCierreReal}
                            onChange={(e) => setMontoCierreReal(e.target.value)}
                            className="w-full text-slate-800 pl-8 pr-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-slate-500 focus:border-slate-500"
                            placeholder="0.00"
                            required
                            disabled={loading}
                        />
                    </div>
                </div>

                {montoCierreReal !== '' && montoSistema !== null && (
                    <div className={`mb-6 p-3 rounded-md border ${diferencia === 0 ? 'bg-green-50 border-green-200 text-green-800' : 'bg-orange-50 border-orange-200 text-orange-800'}`}>
                        <span className="font-medium">Diferencia: </span>
                        <span className="font-bold">${Math.abs(diferencia).toFixed(2)}</span>
                        {diferencia > 0 && <span> (Sobrante)</span>}
                        {diferencia < 0 && <span> (Faltante)</span>}
                        {diferencia === 0 && <span> (Cuadrada)</span>}
                    </div>
                )}

                <button
                    type="submit"
                    className="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 disabled:opacity-50"
                    disabled={loading || montoSistema === null}
                >
                    {loading ? 'Cerrando Caja...' : 'Confirmar Cierre de Caja'}
                </button>
            </form>
        </div>
    );
}
