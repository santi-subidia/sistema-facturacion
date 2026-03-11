import React, { useState } from 'react';
import { useComprobantes } from '../../hooks/useComprobantes';

function ComprobanteFiltros({ onFilterChange }) {
    const { tiposComprobante, formasPago, condicionesVenta, loading } = useComprobantes();
    const [isExpanded, setIsExpanded] = useState(false);

    const [filtros, setFiltros] = useState({
        clienteDocumentoNombre: '',
        numeroComprobante: '',
        fechaDesde: '',
        fechaHasta: '',
        idTipoComprobante: '',
        idFormaPago: '',
        idCondicionVenta: '',
        totalDesde: '',
        totalHasta: ''
    });

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFiltros(prev => ({
            ...prev,
            [name]: value
        }));
    };

    const handleApply = () => {
        onFilterChange(filtros);
    };

    const handleClear = () => {
        const clearedFiltros = {
            clienteDocumentoNombre: '',
            numeroComprobante: '',
            fechaDesde: '',
            fechaHasta: '',
            idTipoComprobante: '',
            idFormaPago: '',
            idCondicionVenta: '',
            totalDesde: '',
            totalHasta: ''
        };
        setFiltros(clearedFiltros);
        onFilterChange(clearedFiltros);
    };

    const hasActiveFilters = filtros.clienteDocumentoNombre || filtros.numeroComprobante || filtros.fechaDesde || filtros.fechaHasta || filtros.idTipoComprobante || filtros.idFormaPago || filtros.idCondicionVenta || filtros.totalDesde || filtros.totalHasta;

    return (
        <div className="border-b border-slate-200">
            <div className="px-6 py-3">
                <button
                    onClick={() => setIsExpanded(!isExpanded)}
                    className="inline-flex items-center text-sm font-medium text-slate-700 hover:text-slate-900"
                >
                    <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z" />
                    </svg>
                    {isExpanded ? 'Ocultar Filtros' : 'Mostrar Filtros'}
                    {hasActiveFilters && (
                        <span className="ml-2 inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                            Activos
                        </span>
                    )}
                </button>
            </div>

            {isExpanded && (
                <div className="px-6 pb-4 bg-slate-50/50">
                    <div className="grid grid-cols-1 gap-y-4 gap-x-4 sm:grid-cols-6 mb-4">

                        {/* Fechas */}
                        <div className="sm:col-span-2">
                            <label className="block text-sm font-medium text-slate-700">Fecha Desde</label>
                            <input
                                type="date"
                                name="fechaDesde"
                                value={filtros.fechaDesde}
                                onChange={handleChange}
                                className="mt-1 focus:ring-blue-500 focus:border-blue-500 block w-full shadow-sm sm:text-sm border-slate-300 rounded-md"
                            />
                        </div>
                        <div className="sm:col-span-2">
                            <label className="block text-sm font-medium text-slate-700">Fecha Hasta</label>
                            <input
                                type="date"
                                name="fechaHasta"
                                value={filtros.fechaHasta}
                                onChange={handleChange}
                                className="mt-1 focus:ring-blue-500 focus:border-blue-500 block w-full shadow-sm sm:text-sm border-slate-300 rounded-md"
                            />
                        </div>
                        <div className="sm:col-span-2"></div> {/* Espaciador */}

                        {/* Montos */}
                        <div className="sm:col-span-2">
                            <label className="block text-sm font-medium text-slate-700">Monto Mínimo</label>
                            <div className="mt-1 relative rounded-md shadow-sm">
                                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                                    <span className="text-slate-500 sm:text-sm">$</span>
                                </div>
                                <input
                                    type="number"
                                    name="totalDesde"
                                    value={filtros.totalDesde}
                                    onChange={handleChange}
                                    className="focus:ring-blue-500 focus:border-blue-500 block w-full pl-7 sm:text-sm border-slate-300 rounded-md"
                                    placeholder="0.00"
                                />
                            </div>
                        </div>
                        <div className="sm:col-span-2">
                            <label className="block text-sm font-medium text-slate-700">Monto Máximo</label>
                            <div className="mt-1 relative rounded-md shadow-sm">
                                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                                    <span className="text-slate-500 sm:text-sm">$</span>
                                </div>
                                <input
                                    type="number"
                                    name="totalHasta"
                                    value={filtros.totalHasta}
                                    onChange={handleChange}
                                    className="focus:ring-blue-500 focus:border-blue-500 block w-full pl-7 sm:text-sm border-slate-300 rounded-md"
                                    placeholder="0.00"
                                />
                            </div>
                        </div>
                        <div className="sm:col-span-2"></div>

                        {/* Entidades y Búsqueda */}
                        <div className="sm:col-span-3">
                            <label className="block text-sm font-medium text-slate-700">Cliente o Documento</label>
                            <input
                                type="text"
                                name="clienteDocumentoNombre"
                                value={filtros.clienteDocumentoNombre}
                                onChange={handleChange}
                                placeholder="Buscar cliente o número doc..."
                                className="mt-1 block w-full py-2 px-3 border border-slate-300 bg-white rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                                onKeyDown={(e) => e.key === 'Enter' && handleApply()}
                            />
                        </div>

                        <div className="sm:col-span-3">
                            <label className="block text-sm font-medium text-slate-700">Número de Factura</label>
                            <input
                                type="text"
                                name="numeroComprobante"
                                value={filtros.numeroComprobante}
                                onChange={handleChange}
                                placeholder="Ej: 0001-00000012"
                                className="mt-1 block w-full py-2 px-3 border border-slate-300 bg-white rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                                onKeyDown={(e) => e.key === 'Enter' && handleApply()}
                            />
                        </div>

                        <div className="sm:col-span-3">
                            <label className="block text-sm font-medium text-slate-700">Tipo de Factura</label>
                            <select
                                name="idTipoComprobante"
                                value={filtros.idTipoComprobante}
                                onChange={handleChange}
                                className="mt-1 block w-full py-2 px-3 border border-slate-300 bg-white rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                            >
                                <option value="">Todos los Tipos</option>
                                {!loading && tiposComprobante
                                    .filter(tipo => tipo.nombre?.toLowerCase().includes("factura"))
                                    .map(tipo => (
                                        <option key={tipo.id} value={tipo.id}>{tipo.nombre}</option>
                                    ))}
                            </select>
                        </div>

                        <div className="sm:col-span-3">
                            <label className="block text-sm font-medium text-slate-700">Forma de Pago</label>
                            <select
                                name="idFormaPago"
                                value={filtros.idFormaPago}
                                onChange={handleChange}
                                className="mt-1 block w-full py-2 px-3 border border-slate-300 bg-white rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                            >
                                <option value="">Todas las Formas</option>
                                {!loading && formasPago.map(forma => (
                                    <option key={forma.id} value={forma.id}>{forma.nombre}</option>
                                ))}
                            </select>
                        </div>

                        <div className="sm:col-span-3">
                            <label className="block text-sm font-medium text-slate-700">Condición de Venta</label>
                            <select
                                name="idCondicionVenta"
                                value={filtros.idCondicionVenta}
                                onChange={handleChange}
                                className="mt-1 block w-full py-2 px-3 border border-slate-300 bg-white rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 sm:text-sm"
                            >
                                <option value="">Todas las Condiciones</option>
                                {!loading && condicionesVenta.map(cond => (
                                    <option key={cond.id} value={cond.id}>{cond.descripcion}</option>
                                ))}
                            </select>
                        </div>
                    </div>

                    <div className="flex justify-end gap-3 mt-4 pt-4 border-t border-slate-200">
                        {hasActiveFilters && (
                            <button
                                type="button"
                                onClick={handleClear}
                                className="bg-white py-2 px-4 border border-slate-300 rounded-md shadow-sm text-sm font-medium text-slate-700 hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                            >
                                Limpiar Filtros
                            </button>
                        )}
                        <button
                            type="button"
                            onClick={handleApply}
                            className="inline-flex justify-center py-2 px-4 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                        >
                            <svg className="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                            </svg>
                            Aplicar Filtros
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
}

export default ComprobanteFiltros;
