import React from 'react';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';

const RecentActivity = ({ data }) => {

    if (!data || data.length === 0) {
        return (
            <div className="py-8 text-center text-zinc-400 bg-zinc-50 rounded-xl border border-zinc-100 border-dashed">
                <p>No hay actividad reciente</p>
            </div>
        );
    }

    const formatCurrency = (value) => {
        return new Intl.NumberFormat('es-AR', {
            style: 'currency',
            currency: 'ARS'
        }).format(value);
    };

    const getStatusStyle = (estado) => {
        switch (estado?.toLowerCase()) {
            case 'aprobado':
            case 'facturado':
            case 'aceptado':
                return 'bg-emerald-50 text-emerald-600 border-emerald-100';
            case 'pendiente':
            case 'borrador':
                return 'bg-amber-50 text-amber-600 border-amber-100';
            case 'rechazado':
            case 'cancelado':
                return 'bg-red-50 text-red-600 border-red-100';
            case 'venta en negro':
                return 'bg-zinc-100 text-zinc-600 border-zinc-200';
            default:
                return 'bg-blue-50 text-blue-600 border-blue-100';
        }
    };

    const getTypeIcon = (tipo) => {
        if (tipo === 'Comprobante') {
            return (
                <div className="p-2 rounded-lg bg-indigo-50 text-indigo-500">
                    <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                    </svg>
                </div>
            );
        }
        return (
            <div className="p-2 rounded-lg bg-orange-50 text-orange-500">
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" />
                </svg>
            </div>
        );
    };

    return (
        <div className="bg-white rounded-2xl border border-zinc-200 shadow-sm overflow-hidden">
            <div className="px-6 py-5 border-b border-zinc-100 flex justify-between items-center">
                <h3 className="text-lg font-bold text-zinc-800 font-outfit">Actividad Reciente</h3>
            </div>

            <div className="overflow-x-auto">
                <table className="w-full text-sm text-left">
                    <thead className="text-xs text-zinc-500 bg-zinc-50/50 uppercase border-b border-zinc-100">
                        <tr>
                            <th className="px-6 py-3 font-medium">Documento</th>
                            <th className="px-6 py-3 font-medium">Monto</th>
                            <th className="px-6 py-3 font-medium">Fecha</th>
                            <th className="px-6 py-3 font-medium text-right">Estado</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-zinc-100">
                        {data.map((item, index) => (
                            <tr key={`${item.tipo}-${item.id}-${index}`} className="hover:bg-zinc-50/50 transition-colors">
                                <td className="px-6 py-4">
                                    <div className="flex items-center space-x-3">
                                        {getTypeIcon(item.tipo)}
                                        <div>
                                            <p className="font-medium text-zinc-800">{item.descripcion}</p>
                                            <p className="text-xs text-zinc-400">{item.tipo}</p>
                                        </div>
                                    </div>
                                </td>
                                <td className="px-6 py-4 font-outfit font-bold text-zinc-700">
                                    {formatCurrency(item.monto)}
                                </td>
                                <td className="px-6 py-4 text-zinc-500">
                                    {format(new Date(item.fecha), "d MMM, HH:mm", { locale: es })}
                                </td>
                                <td className="px-6 py-4 text-right">
                                    <span className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium border ${getStatusStyle(item.estado)}`}>
                                        {item.estado}
                                    </span>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
};

export default RecentActivity;
