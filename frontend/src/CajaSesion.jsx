import React, { useEffect, useState } from 'react';
import { useCaja } from './hooks/useCaja';
import AperturaCaja from './components/Caja/AperturaCaja';
import CierreCaja from './components/Caja/CierreCaja';
import MovimientosCaja from './components/Caja/MovimientosCaja';

export default function CajaSesion() {
    const { fetchSesionActiva, sesionActiva, loading } = useCaja();
    const [initializing, setInitializing] = useState(true);
    const [activeTab, setActiveTab] = useState('apertura'); // 'apertura', 'movimientos', 'cierre'

    useEffect(() => {
        cargarSesion();
    }, []);

    const cargarSesion = async () => {
        setInitializing(true);
        const sesion = await fetchSesionActiva();
        if (sesion) {
            setActiveTab('movimientos');
        } else {
            setActiveTab('apertura');
        }
        setInitializing(false);
    };

    if (initializing) {
        return (
            <div className="flex justify-center items-center h-64">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-slate-600"></div>
            </div>
        );
    }

    return (
        <div className="container mx-auto">
            <div className="mb-6 flex flex-col sm:flex-row justify-between items-start sm:items-center">
                <div>
                    <h1 className="text-2xl font-bold text-slate-900">Control de Caja</h1>
                    {sesionActiva ? (
                        <p className="mt-1 text-sm text-slate-500">
                            Caja: <span className="font-semibold text-slate-700">{sesionActiva.cajaNombre}</span> | Abierta desde: {new Date(sesionActiva.fechaApertura).toLocaleTimeString()}
                        </p>
                    ) : (
                        <p className="mt-1 text-sm text-slate-500">Gestión de turnos y saldos en efectivo.</p>
                    )}
                </div>

                {/* Pestañas */}
                <div className="mt-4 sm:mt-0 flex bg-slate-100 p-1 rounded-lg">
                    {!sesionActiva && (
                        <button
                            onClick={() => setActiveTab('apertura')}
                            className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${activeTab === 'apertura' ? 'bg-white text-slate-800 shadow-sm' : 'text-slate-500 hover:text-slate-700'}`}
                        >
                            Abrir Caja
                        </button>
                    )}
                    {sesionActiva && (
                        <>
                            <button
                                onClick={() => setActiveTab('movimientos')}
                                className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${activeTab === 'movimientos' ? 'bg-white text-slate-800 shadow-sm' : 'text-slate-500 hover:text-slate-700'}`}
                            >
                                Movimientos
                            </button>
                            <button
                                onClick={() => setActiveTab('cierre')}
                                className={`px-4 py-2 text-sm font-medium rounded-md transition-colors ${activeTab === 'cierre' ? 'bg-white text-slate-800 shadow-sm' : 'text-slate-500 hover:text-slate-700'}`}
                            >
                                Cerrar Caja
                            </button>
                        </>
                    )}
                </div>
            </div>

            {!sesionActiva && activeTab === 'apertura' && (
                <>
                    <div className="bg-amber-50 border-l-4 border-amber-400 p-4 mb-4 mt-8 max-w-lg mx-auto">
                        <div className="flex">
                            <div className="flex-shrink-0">
                                <svg className="h-5 w-5 text-amber-400" viewBox="0 0 20 20" fill="currentColor">
                                    <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                                </svg>
                            </div>
                            <div className="ml-3">
                                <p className="text-sm text-amber-700">
                                    No tiene ninguna sesión de caja abierta. Debe abrir una caja para comenzar a operar.
                                </p>
                            </div>
                        </div>
                    </div>
                    <AperturaCaja onAperturaExitosa={cargarSesion} />
                </>
            )}

            {sesionActiva && activeTab === 'movimientos' && (
                <MovimientosCaja sesionActiva={sesionActiva} />
            )}

            {sesionActiva && activeTab === 'cierre' && (
                <CierreCaja sesionActiva={sesionActiva} onCierreExitoso={cargarSesion} />
            )}
        </div>
    );
}
