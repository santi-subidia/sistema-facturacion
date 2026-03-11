import React, { useState } from 'react'
import { useComprobantesList } from './hooks/useComprobantesList'
import ComprobantesTable from './components/comprobantes/ComprobantesTable'
import ComprobanteDetalle from './components/comprobantes/ComprobanteDetalle'
import ComprobanteFiltros from './components/comprobantes/ComprobanteFiltros'
import Pagination from './components/shared/Pagination'

function ComprobantesLista() {
    const {
        comprobantes,
        pagination,
        currentPage,
        loading,
        error,
        handlePageChange,
        applyFilters,
        pageSize,
        setPageSize
    } = useComprobantesList()

    const [comprobanteSeleccionado, setComprobanteSeleccionado] = useState(null)
    const [showDetalle, setShowDetalle] = useState(false)

    const handleVerDetalle = (comprobante) => {
        setComprobanteSeleccionado(comprobante)
        setShowDetalle(true)
    }

    const handleCloseDetalle = () => {
        setShowDetalle(false)
        setComprobanteSeleccionado(null)
    }

    return (
        <div className="space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-700">
            <div className="glass-panel rounded-3xl overflow-hidden shadow-xl border border-white/40 ring-1 ring-black/5">
                <div className="absolute top-0 left-0 w-full h-1.5 bg-gradient-to-r from-blue-400 via-blue-600 to-sky-400 opacity-70"></div>

                <div className="px-8 py-6 border-b border-slate-100/50">
                    <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                        <div>
                            <h2 className="text-2xl font-bold text-slate-800 tracking-tight font-outfit uppercase">
                                Facturas <span className="text-blue-600">Generadas</span>
                            </h2>
                            <p className="text-[10px] font-bold text-slate-400 mt-1 uppercase tracking-[0.2em]">
                                Historial completo de transacciones
                            </p>
                        </div>
                        <div className="flex items-center gap-4">
                            <div className="flex items-center space-x-2">
                                <label htmlFor="pageSize" className="text-sm font-medium text-slate-700">Mostrar:</label>
                                <select
                                    id="pageSize"
                                    value={pageSize}
                                    onChange={(e) => setPageSize(parseInt(e.target.value))}
                                    className="block w-full pl-3 pr-8 py-1.5 text-sm border-slate-300 focus:outline-none focus:ring-blue-500 focus:border-blue-500 rounded-md"
                                >
                                    <option value={10}>10</option>
                                    <option value={20}>20</option>
                                    <option value={50}>50</option>
                                    <option value={100}>100</option>
                                </select>
                            </div>
                            {pagination && (
                                <div className="px-4 py-2 bg-blue-50 rounded-2xl border border-blue-100/50">
                                    <span className="text-[10px] font-bold text-blue-700 uppercase tracking-widest">
                                        Total: {pagination.totalItems} registros
                                    </span>
                                </div>
                            )}
                        </div>
                    </div>
                </div>

                <ComprobanteFiltros onFilterChange={applyFilters} />

                <div className="px-1">
                    <ComprobantesTable
                        comprobantes={comprobantes}
                        loading={loading}
                        error={error}
                        onVerDetalle={handleVerDetalle}
                    />
                </div>

                {pagination && (
                    <div className="px-8 py-6 border-t border-slate-100/50 bg-white/40">
                        <Pagination
                            pagination={pagination}
                            currentPage={currentPage}
                            onPageChange={handlePageChange}
                            itemName="comprobantes"
                        />
                    </div>
                )}
            </div>

            <ComprobanteDetalle
                show={showDetalle}
                comprobante={comprobanteSeleccionado}
                onClose={handleCloseDetalle}
            />
        </div>
    )
}

export default ComprobantesLista
