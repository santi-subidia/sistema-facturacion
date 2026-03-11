import React from 'react';
import { useDashboard } from './hooks/useDashboard';
import { Activity, DollarSign, FileText, Package, AlertTriangle, TrendingUp, RefreshCw } from 'lucide-react';

// Components
import KpiCard from './components/dashboard/KpiCard';
import SalesChart from './components/dashboard/SalesChart';
import PaymentDonut from './components/dashboard/PaymentDonut';
import RecentActivity from './components/dashboard/RecentActivity';
import AlertsPanel from './components/dashboard/AlertsPanel';

const Dashboard = () => {
    const { data, loading, error, refetch } = useDashboard();

    const formatCurrency = (value) => {
        return new Intl.NumberFormat('es-AR', {
            style: 'currency',
            currency: 'ARS',
            maximumFractionDigits: 0
        }).format(value || 0);
    };

    if (loading && !data) {
        return (
            <div className="flex flex-col items-center justify-center h-96">
                <RefreshCw className="w-8 h-8 text-blue-500 animate-spin mb-4" />
                <p className="text-slate-500 font-medium">Cargando métricas del negocio...</p>
            </div>
        );
    }

    if (error) {
        return (
            <div className="bg-red-50 text-red-600 p-6 rounded-2xl border border-red-100 flex flex-col items-center justify-center text-center">
                <AlertTriangle className="w-10 h-10 mb-3" />
                <h3 className="text-lg font-bold mb-1">Error al cargar dashboard</h3>
                <p className="text-sm opacity-80 mb-4">{error}</p>
                <button
                    onClick={refetch}
                    className="px-4 py-2 bg-red-600 text-white rounded-xl text-sm font-bold shadow-sm hover:bg-red-700 transition"
                >
                    Reintentar
                </button>
            </div>
        );
    }

    if (!data) return null;

    // Calcular tendencia de ventas
    const salesDiff = data.ventasMes - data.ventasMesAnterior;
    const salesTrendPercentage = data.ventasMesAnterior > 0
        ? ((salesDiff) / data.ventasMesAnterior * 100).toFixed(1)
        : 100;
    const salesTrend = salesDiff >= 0 ? 'up' : 'down';

    return (
        <div className="space-y-6 animate-fade-in">
            {/* Header con bienvenida */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-2">
                <div>
                    <h1 className="text-2xl font-bold text-slate-800 font-outfit tracking-tight">Vista General</h1>
                    <p className="text-slate-500 text-sm mt-1">Resumen de la operatoria de tu negocio.</p>
                </div>
                <div className="flex items-center space-x-3">
                    {data.cajaSesionActual && (
                        <div className="bg-emerald-50 text-emerald-700 px-4 py-2 rounded-xl border border-emerald-100 flex items-center shadow-sm">
                            <span className="w-2 h-2 rounded-full bg-emerald-500 mr-2 animate-pulse"></span>
                            <div className="text-xs">
                                <span className="font-bold uppercase tracking-wider block leading-none mb-1">Caja Abierta</span>
                                <span>{formatCurrency(data.cajaSesionActual.saldoActual)}</span>
                            </div>
                        </div>
                    )}
                    <button
                        onClick={refetch}
                        className="p-2.5 bg-white border border-slate-200 text-slate-500 rounded-xl hover:bg-slate-50 transition shadow-sm"
                        title="Actualizar datos"
                    >
                        <RefreshCw className="w-4 h-4" />
                    </button>
                </div>
            </div>

            {/* KPI Cards Row */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                <KpiCard
                    title="Ventas del Mes"
                    value={formatCurrency(data.ventasMes)}
                    icon={<DollarSign className="w-6 h-6" />}
                    trend={salesTrend}
                    trendValue={`${Math.abs(salesTrendPercentage)}%`}
                    subtext="vs mes anterior"
                    color="primary"
                />

                <KpiCard
                    title="Comprobantes Emitidos"
                    value={data.comprobantesEmitidos}
                    icon={<FileText className="w-6 h-6" />}
                    color="emerald"
                />

                <KpiCard
                    title="Presupuestos Pendientes"
                    value={data.presupuestosPendientes}
                    icon={<Activity className="w-6 h-6" />}
                    color={data.presupuestosPendientes > 0 ? "amber" : "blue"}
                />

                <KpiCard
                    title="Stock Crítico"
                    value={data.productosStockBajo}
                    icon={<Package className="w-6 h-6" />}
                    color={data.productosStockBajo > 0 ? "red" : "emerald"}
                />
            </div>

            {/* Charts Row */}
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                <div className="lg:col-span-2">
                    <SalesChart data={data.ventasPorMes} />
                </div>
                <div className="lg:col-span-1">
                    <PaymentDonut data={data.distribucionCondicionVenta} />
                </div>
            </div>

            {/* Bottom Row */}
            <div className="grid grid-cols-1 xl:grid-cols-3 gap-6">
                <div className="xl:col-span-2">
                    <RecentActivity data={data.actividadReciente} />
                </div>
                <div className="xl:col-span-1">
                    <AlertsPanel alertas={data.alertas} />
                </div>
            </div>
        </div>
    );
};

export default Dashboard;
