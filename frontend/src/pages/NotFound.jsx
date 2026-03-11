import React from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { Home, AlertTriangle } from 'lucide-react';

const NotFound = () => {
    const navigate = useNavigate();
    const location = useLocation();

    return (
        <div className="min-h-screen bg-gray-50 flex flex-col justify-center items-center px-4 sm:px-6 lg:px-8">
            <div className="max-w-md w-full space-y-8 text-center">
                <div className="flex justify-center">
                    <div className="h-24 w-24 bg-red-100 rounded-full flex items-center justify-center">
                        <AlertTriangle className="h-12 w-12 text-red-600" />
                    </div>
                </div>

                <h2 className="mt-6 text-3xl font-extrabold text-gray-900">
                    Error 404: Página no encontrada
                </h2>

                <div className="bg-white px-6 py-8 rounded-xl shadow-lg border border-gray-100 mt-8">
                    <p className="text-md text-gray-600 mb-6">
                        Lo sentimos, pero la ruta <span className="font-mono text-sm bg-gray-100 px-2 py-1 rounded text-red-600 break-all">{location.pathname}</span> no existe o no tienes permisos para acceder a ella.
                    </p>

                    <button
                        onClick={() => navigate('/')}
                        className="w-full flex justify-center items-center py-3 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
                    >
                        <Home className="mr-2 h-5 w-5" />
                        Volver al Inicio
                    </button>
                </div>
            </div>
        </div>
    );
};

export default NotFound;
