import React from 'react';
import { PieChart, Pie, Cell, Tooltip, ResponsiveContainer, Legend } from 'recharts';

const PaymentDonut = ({ data }) => {

    // Paleta de colores para la dona
    const COLORS = ['#6366f1', '#10b981', '#f59e0b', '#3b82f6', '#8b5cf6', '#ec4899'];

    const formatCurrency = (value) => {
        return new Intl.NumberFormat('es-AR', {
            style: 'currency',
            currency: 'ARS',
            maximumFractionDigits: 0
        }).format(value);
    };

    const CustomTooltip = ({ active, payload }) => {
        if (active && payload && payload.length) {
            return (
                <div className="bg-white p-3 border border-zinc-200 shadow-xl rounded-xl">
                    <p className="text-sm font-bold text-zinc-500 mb-1">{payload[0].name}</p>
                    <p className="text-lg font-outfit font-bold text-zinc-800">
                        {formatCurrency(payload[0].value)}
                    </p>
                    <p className="text-xs text-zinc-400 mt-1">
                        {payload[0].payload.cantidad} operaciones
                    </p>
                </div>
            );
        }
        return null;
    };

    if (!data || data.length === 0) {
        return (
            <div className="h-72 flex items-center justify-center text-zinc-400 bg-zinc-50 rounded-xl border border-zinc-100 border-dashed">
                <p>No hay datos de ventas disponibles</p>
            </div>
        );
    }

    return (
        <div className="bg-white p-6 rounded-2xl border border-zinc-200 shadow-sm h-full flex flex-col">
            <h3 className="text-lg font-bold text-zinc-800 font-outfit mb-2">Condiciones de Venta</h3>
            <p className="text-sm text-zinc-500 mb-6">Distribución del mes actual</p>

            <div className="flex-1 w-full min-h-[250px]">
                <ResponsiveContainer width="100%" height="100%">
                    <PieChart>
                        <Pie
                            data={data}
                            cx="50%"
                            cy="50%"
                            innerRadius={70}
                            outerRadius={100}
                            paddingAngle={5}
                            dataKey="total"
                            nameKey="condicionVenta"
                            stroke="none"
                        >
                            {data.map((entry, index) => (
                                <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                            ))}
                        </Pie>
                        <Tooltip content={<CustomTooltip />} />
                        <Legend
                            verticalAlign="bottom"
                            height={36}
                            iconType="circle"
                            wrapperStyle={{ fontSize: '12px', paddingTop: '20px' }}
                        />
                    </PieChart>
                </ResponsiveContainer>
            </div>
        </div>
    );
};

export default PaymentDonut;
