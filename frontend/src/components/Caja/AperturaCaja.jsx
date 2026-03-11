import React, { useState, useEffect } from 'react';
import { useCaja } from '../../hooks/useCaja';

export default function AperturaCaja({ onAperturaExitosa }) {
    const { cajas, fetchCajas, abrirCaja, loading, error } = useCaja();
    const [selectedCaja, setSelectedCaja] = useState('');
    const [montoInicial, setMontoInicial] = useState('');
    const [localError, setLocalError] = useState('');

    useEffect(() => {
        fetchCajas();
    }, [fetchCajas]);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLocalError('');
        if (!selectedCaja) {
            setLocalError('Debe seleccionar una caja.');
            return;
        }
        if (montoInicial === '' || isNaN(montoInicial) || Number(montoInicial) < 0) {
            setLocalError('Debe ingresar un monto inicial válido.');
            return;
        }

        try {
            await abrirCaja(Number(selectedCaja), Number(montoInicial));
            if (onAperturaExitosa) onAperturaExitosa();
        } catch (err) {
            setLocalError(err.message || 'Error al abrir la caja');
        }
    };

    return (
        <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200 mt-4 max-w-lg mx-auto">
            <h2 className="text-xl font-semibold mb-4 text-gray-800">Apertura de Caja</h2>
            {(error || localError) && (
                <div className="mb-4 bg-red-50 p-3 rounded-md border border-red-200 text-red-600 text-sm">
                    {error || localError}
                </div>
            )}
            <form onSubmit={handleSubmit}>
                <div className="mb-4">
                    <label className="block text-sm font-medium text-gray-700 mb-1">Caja a Abrir</label>
                    <select
                        value={selectedCaja}
                        onChange={(e) => setSelectedCaja(e.target.value)}
                        className="w-full text-slate-800 border-gray-300 rounded-md shadow-sm focus:ring-slate-500 focus:border-slate-500 p-2 border"
                        required
                        disabled={loading}
                    >
                        <option value="">Seleccione una caja...</option>
                        {cajas.filter(c => c.activa).map(caja => (
                            <option key={caja.id} value={caja.id}>{caja.nombre}</option>
                        ))}
                    </select>
                </div>
                <div className="mb-6">
                    <label className="block text-sm font-medium text-gray-700 mb-1">Monto Inicial (Efectivo)</label>
                    <div className="relative">
                        <span className="absolute inset-y-0 left-0 pl-3 flex items-center text-gray-500">$</span>
                        <input
                            type="number"
                            step="0.01"
                            min="0"
                            value={montoInicial}
                            onChange={(e) => setMontoInicial(e.target.value)}
                            className="w-full text-slate-800 pl-8 pr-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-slate-500 focus:border-slate-500"
                            placeholder="0.00"
                            required
                            disabled={loading}
                        />
                    </div>
                </div>
                <button
                    type="submit"
                    className="w-full flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-slate-600 hover:bg-slate-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-slate-500 disabled:opacity-50"
                    disabled={loading}
                >
                    {loading ? 'Abriendo...' : 'Abrir Caja'}
                </button>
            </form>
        </div>
    );
}
