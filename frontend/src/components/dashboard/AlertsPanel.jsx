import React from 'react';
import { useNavigate } from 'react-router-dom';

const AlertsPanel = ({ alertas }) => {
    const navigate = useNavigate();

    if (!alertas || alertas.length === 0) {
        return (
            <div className="bg-white rounded-2xl border border-zinc-200 shadow-sm p-6 text-center text-zinc-400">
                <div className="w-12 h-12 bg-zinc-50 rounded-full flex items-center justify-center mx-auto mb-3 text-zinc-300">
                    <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                    </svg>
                </div>
                <p className="text-sm font-medium">Todo bajo control</p>
                <p className="text-xs mt-1">No hay alertas activas en este momento.</p>
            </div>
        );
    }

    const getAlertStyle = (tipo) => {
        switch (tipo) {
            case 'warning':
                return 'bg-amber-50 border-amber-200 text-amber-800';
            case 'error':
                return 'bg-red-50 border-red-200 text-red-800';
            case 'info':
            default:
                return 'bg-blue-50 border-blue-200 text-blue-800';
        }
    };

    const getAlertIcon = (tipo) => {
        switch (tipo) {
            case 'warning':
                return (
                    <svg className="w-5 h-5 text-amber-500 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                    </svg>
                );
            case 'error':
                return (
                    <svg className="w-5 h-5 text-red-500 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                );
            case 'info':
            default:
                return (
                    <svg className="w-5 h-5 text-blue-500 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                );
        }
    };

    return (
        <div className="bg-white rounded-2xl border border-zinc-200 shadow-sm overflow-hidden h-full">
            <div className="px-6 py-5 border-b border-zinc-100 flex items-center space-x-2">
                <h3 className="text-lg font-bold text-zinc-800 font-outfit">Notificaciones</h3>
                <span className="bg-red-500 text-white text-xs font-bold px-2 py-0.5 rounded-full">
                    {alertas.length}
                </span>
            </div>

            <div className="p-6 space-y-4">
                {alertas.map((alerta) => (
                    <div
                        key={alerta.id}
                        className={`p-4 rounded-xl border flex items-start space-x-3 ${getAlertStyle(alerta.tipo)}`}
                    >
                        {getAlertIcon(alerta.tipo)}
                        <div className="flex-1">
                            <p className="text-sm font-medium">{alerta.mensaje}</p>
                            {alerta.accionUrl && (
                                <button
                                    onClick={() => navigate(alerta.accionUrl)}
                                    className="mt-2 text-xs font-bold uppercase tracking-wider hover:opacity-70 transition-opacity"
                                >
                                    Ir a resolver &rarr;
                                </button>
                            )}
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
};

export default AlertsPanel;
