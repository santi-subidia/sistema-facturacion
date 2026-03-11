import React from 'react';
import HistorialSesiones from './components/Caja/HistorialSesiones';

export default function CajaHistorial() {
    return (
        <div className="container mx-auto">
            <div className="mb-6">
                <h1 className="text-2xl font-bold text-gray-900">Historial de Sesiones</h1>
                <p className="mt-1 text-sm text-gray-500">Registro de todas las sesiones de caja cerradas.</p>
            </div>
            <HistorialSesiones />
        </div>
    );
}
