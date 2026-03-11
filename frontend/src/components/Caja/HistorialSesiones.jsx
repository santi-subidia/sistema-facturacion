import React, { useEffect, useState, useCallback } from 'react';
import { useCaja } from '../../hooks/useCaja';

const formatCurrency = (value) =>
    new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' }).format(value ?? 0);

const formatDate = (dateStr) => {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleString('es-AR', {
        day: '2-digit', month: '2-digit', year: 'numeric',
        hour: '2-digit', minute: '2-digit',
    });
};

const formatTime = (dateStr) => {
    if (!dateStr) return '';
    return new Date(dateStr).toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' });
};

const calcularDuracion = (apertura, cierre) => {
    if (!apertura || !cierre) return '-';
    const diff = Math.floor((new Date(cierre) - new Date(apertura)) / 60000);
    const h = Math.floor(diff / 60);
    const m = diff % 60;
    return h > 0 ? `${h}h ${m}m` : `${m}m`;
};

// Colores y íconos por tipo de ítem
function ItemIcon({ item }) {
    if (item.tipo === 'Movimiento') {
        const esIngreso = item.tipoMovimiento === 'Ingreso';
        return (
            <div className={`w-8 h-8 rounded-full flex items-center justify-center flex-shrink-0 ${esIngreso ? 'bg-green-100' : 'bg-red-100'}`}>
                <svg className={`w-4 h-4 ${esIngreso ? 'text-green-600' : 'text-red-600'}`} fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    {esIngreso
                        ? <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 10l7-7m0 0l7 7m-7-7v18" />
                        : <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 14l-7 7m0 0l-7-7m7 7V3" />
                    }
                </svg>
            </div>
        );
    }
    // Comprobante
    return (
        <div className="w-8 h-8 rounded-full bg-blue-100 flex items-center justify-center flex-shrink-0">
            <svg className="w-4 h-4 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
        </div>
    );
}

function DetalleSesion({ sesionId, fetchDetalleSesion }) {
    const [items, setItems] = useState(null);
    const [loadingDetalle, setLoadingDetalle] = useState(false);

    useEffect(() => {
        const cargar = async () => {
            setLoadingDetalle(true);
            const data = await fetchDetalleSesion(sesionId);
            setItems(data?.items ?? []);
            setLoadingDetalle(false);
        };
        cargar();
    }, [sesionId, fetchDetalleSesion]);

    if (loadingDetalle) {
        return (
            <div className="flex items-center justify-center py-6">
                <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-slate-500"></div>
            </div>
        );
    }

    if (!items || items.length === 0) {
        return (
            <p className="text-sm text-gray-400 text-center py-4">Sin movimientos registrados en esta sesión.</p>
        );
    }

    return (
        <div className="relative pl-4">
            {/* Línea vertical del timeline */}
            <div className="absolute left-8 top-2 bottom-2 w-px bg-gray-200"></div>

            <div className="space-y-3">
                {items.map((item, idx) => {
                    const esIngreso = item.tipo === 'Movimiento' ? item.tipoMovimiento === 'Ingreso' : true;
                    const montoColor = item.tipo === 'Comprobante'
                        ? 'text-blue-700'
                        : item.monto >= 0 ? 'text-green-700' : 'text-red-700';

                    return (
                        <div key={idx} className="flex items-start gap-3">
                            <ItemIcon item={item} />
                            <div className="flex-1 min-w-0 flex items-start justify-between gap-2 py-1">
                                <div className="min-w-0">
                                    <p className="text-sm text-gray-700 leading-tight truncate" title={item.descripcion}>
                                        {item.descripcion}
                                    </p>
                                    <div className="flex items-center gap-2 mt-0.5">
                                        <span className="text-xs text-gray-400">{formatTime(item.fecha)}</span>
                                        {item.tipo === 'Comprobante' && item.condicionVenta && (
                                            <span className="text-xs bg-gray-100 text-gray-500 px-1.5 py-0.5 rounded">
                                                {item.condicionVenta}
                                            </span>
                                        )}
                                    </div>
                                </div>
                                <span className={`text-sm font-semibold whitespace-nowrap ${montoColor}`}>
                                    {item.tipo === 'Movimiento' && item.monto >= 0 ? '+' : ''}
                                    {formatCurrency(Math.abs(item.monto))}
                                </span>
                            </div>
                        </div>
                    );
                })}
            </div>
        </div>
    );
}

export default function HistorialSesiones() {
    const { fetchHistorialSesiones, fetchDetalleSesion, loading } = useCaja();
    const [sesiones, setSesiones] = useState([]);
    const [page, setPage] = useState(1);
    const [hasMore, setHasMore] = useState(true);
    const [expandedId, setExpandedId] = useState(null);
    const PAGE_SIZE = 8;

    useEffect(() => {
        cargarHistorial(1);
    }, []);

    const cargarHistorial = async (p) => {
        const data = await fetchHistorialSesiones(p, PAGE_SIZE);
        if (p === 1) {
            setSesiones(data);
        } else {
            setSesiones(prev => [...prev, ...data]);
        }
        setHasMore(data.length === PAGE_SIZE);
        setPage(p);
    };

    const cargarMas = () => cargarHistorial(page + 1);
    const toggleExpand = (id) => setExpandedId(prev => (prev === id ? null : id));

    if (loading && sesiones.length === 0) {
        return (
            <div className="flex justify-center items-center h-40">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-slate-600"></div>
            </div>
        );
    }

    if (!loading && sesiones.length === 0) {
        return (
            <div className="text-center py-16 text-gray-500">
                <svg className="mx-auto h-12 w-12 text-gray-300 mb-3" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
                <p className="text-sm font-medium">No hay sesiones anteriores registradas.</p>
            </div>
        );
    }

    return (
        <div className="space-y-3">
            {sesiones.map((sesion) => {
                const diferencia = sesion.montoCierreReal != null && sesion.montoCierreSistema != null
                    ? sesion.montoCierreReal - sesion.montoCierreSistema
                    : null;
                const isExpanded = expandedId === sesion.id;

                return (
                    <div key={sesion.id} className="bg-white border border-gray-200 rounded-lg shadow-sm overflow-hidden">
                        {/* Cabecera clickable */}
                        <button
                            onClick={() => toggleExpand(sesion.id)}
                            className="w-full text-left px-5 py-4 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-2 hover:bg-gray-50 transition-colors"
                        >
                            <div className="flex items-center gap-3">
                                <div className="flex-shrink-0 w-9 h-9 rounded-full bg-slate-100 flex items-center justify-center">
                                    <svg className="w-5 h-5 text-slate-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.8} d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
                                    </svg>
                                </div>
                                <div>
                                    <p className="text-sm font-semibold text-gray-800">{sesion.cajaNombre}</p>
                                    <p className="text-xs text-gray-400">
                                        {formatDate(sesion.fechaApertura)} · Duración: {calcularDuracion(sesion.fechaApertura, sesion.fechaCierre)} · <span className="font-medium text-gray-500">{sesion.usuarioNombre}</span>
                                    </p>
                                </div>
                            </div>
                            <div className="flex items-center gap-4 sm:gap-6">
                                {diferencia !== null && (
                                    <span className={`text-xs font-medium px-2 py-1 rounded-full ${diferencia >= 0 ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>
                                        {diferencia >= 0 ? '+' : ''}{formatCurrency(diferencia)}
                                    </span>
                                )}
                                <div className="text-right">
                                    <p className="text-xs text-gray-400">Cierre real</p>
                                    <p className="text-sm font-bold text-gray-700">{formatCurrency(sesion.montoCierreReal)}</p>
                                </div>
                                <svg
                                    className={`w-4 h-4 text-gray-400 transition-transform ${isExpanded ? 'rotate-180' : ''}`}
                                    fill="none" viewBox="0 0 24 24" stroke="currentColor"
                                >
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                                </svg>
                            </div>
                        </button>

                        {/* Panel expandible */}
                        {isExpanded && (
                            <div className="border-t border-gray-100">
                                {/* Resumen financiero */}
                                <div className="bg-gray-50 px-5 py-4 grid grid-cols-2 sm:grid-cols-4 gap-4 text-sm border-b border-gray-100">
                                    <div>
                                        <p className="text-xs text-gray-400 mb-1">Apertura</p>
                                        <p className="font-medium text-gray-700">{formatDate(sesion.fechaApertura)}</p>
                                    </div>
                                    <div>
                                        <p className="text-xs text-gray-400 mb-1">Cierre</p>
                                        <p className="font-medium text-gray-700">{formatDate(sesion.fechaCierre)}</p>
                                    </div>
                                    <div>
                                        <p className="text-xs text-gray-400 mb-1">Monto apertura</p>
                                        <p className="font-medium text-gray-700">{formatCurrency(sesion.montoApertura)}</p>
                                    </div>
                                    <div>
                                        <p className="text-xs text-gray-400 mb-1">Cierre sistema</p>
                                        <p className="font-medium text-gray-700">{formatCurrency(sesion.montoCierreSistema)}</p>
                                    </div>
                                    <div>
                                        <p className="text-xs text-gray-400 mb-1">Cierre real</p>
                                        <p className="font-medium text-gray-700">{formatCurrency(sesion.montoCierreReal)}</p>
                                    </div>
                                    <div>
                                        <p className="text-xs text-gray-400 mb-1">Diferencia</p>
                                        <p className={`font-semibold ${diferencia == null ? 'text-gray-500' : diferencia >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                                            {diferencia != null ? `${diferencia >= 0 ? '+' : ''}${formatCurrency(diferencia)}` : '-'}
                                        </p>
                                    </div>
                                </div>

                                {/* Timeline de movimientos y comprobantes */}
                                <div className="px-5 py-4">
                                    <p className="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-3">
                                        Actividad de la sesión
                                    </p>
                                    <DetalleSesion sesionId={sesion.id} fetchDetalleSesion={fetchDetalleSesion} />
                                </div>
                            </div>
                        )}
                    </div>
                );
            })}

            {hasMore && (
                <div className="flex justify-center pt-2">
                    <button
                        onClick={cargarMas}
                        disabled={loading}
                        className="px-5 py-2 text-sm font-medium text-slate-700 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 disabled:opacity-50 transition-colors"
                    >
                        {loading ? 'Cargando...' : 'Cargar más sesiones'}
                    </button>
                </div>
            )}
        </div>
    );
}
