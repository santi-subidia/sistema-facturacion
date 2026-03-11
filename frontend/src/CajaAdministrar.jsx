import React from 'react';
import AdministrarCajas from './components/Caja/AdministrarCajas';

export default function CajaAdministrar() {
    return (
        <div className="container mx-auto">
            <div className="mb-6">
                <h1 className="text-2xl font-bold text-gray-900">Administrar Cajas</h1>
                <p className="mt-1 text-sm text-gray-500">Gestión de cajas registradoras y puntos de venta.</p>
            </div>
            <AdministrarCajas />
        </div>
    );
}
