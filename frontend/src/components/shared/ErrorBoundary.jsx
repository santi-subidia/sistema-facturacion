import React from 'react';

class ErrorBoundary extends React.Component {
    constructor(props) {
        super(props);
        this.state = { hasError: false, error: null, errorInfo: null };
    }

    static getDerivedStateFromError(error) {
        // Actualiza el estado para que el siguiente renderizado muestre la interfaz de repuesto
        return { hasError: true };
    }

    componentDidCatch(error, errorInfo) {
        // También puedes registrar el error en un servicio de reporte de errores
        console.error("Uncaught error:", error, errorInfo);
        this.setState({ error, errorInfo });
    }

    render() {
        if (this.state.hasError) {
            // Puedes renderizar cualquier interfaz de repuesto
            return (
                <div className="min-h-screen bg-gray-50 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
                    <div className="mt-8 sm:mx-auto sm:w-full sm:max-w-md">
                        <div className="bg-white py-8 px-4 shadow sm:rounded-lg sm:px-10 text-center">
                            <svg className="mx-auto h-12 w-12 text-red-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" aria-hidden="true">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z" />
                            </svg>
                            <h2 className="mt-4 text-center text-xl font-extrabold text-gray-900">
                                Ocurrió un error inesperado
                            </h2>
                            <p className="mt-2 text-center text-sm text-gray-600">
                                La aplicación encontró un problema que no pudo manejar.
                            </p>
                            <div className="mt-6 flex justify-center space-x-4">
                                <button
                                    onClick={() => window.location.reload()}
                                    className="inline-flex justify-center py-2 px-4 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                                >
                                    Recargar página
                                </button>
                                <button
                                    onClick={() => window.location.href = '/'}
                                    className="inline-flex justify-center py-2 px-4 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                                >
                                    Volver al inicio
                                </button>
                            </div>
                            {import.meta.env.DEV && this.state.error && (
                                <div className="mt-4 p-4 bg-red-100 text-red-900 overflow-auto text-sm font-mono rounded-md max-h-[300px]">
                                    <p className="font-bold mb-2">Error Details (Development Only):</p>
                                    <p className="text-sm font-semibold text-red-600 truncate">{this.state.error.toString()}</p>
                                    <pre className="mt-2 text-xs text-gray-500 overflow-auto bg-gray-100 p-2 rounded max-h-40">
                                        {this.state.errorInfo?.componentStack}
                                    </pre>
                                </div>
                            )}
                        </div>
                    </div>
                </div>
            );
        }

        return this.props.children;
    }
}

export default ErrorBoundary;
