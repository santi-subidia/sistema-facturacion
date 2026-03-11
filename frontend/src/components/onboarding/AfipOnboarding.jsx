import React, { useState } from 'react'
import { useAfipConfiguracion } from '../../hooks/useAfipConfiguracion'
import AfipConfiguracionModal from '../afip/AfipConfiguracionModal'

function AfipOnboarding() {
    const {
        createConfiguracion,
        condicionesIva,
        loading
    } = useAfipConfiguracion()

    const [submitting, setSubmitting] = useState(false)
    const [error, setError] = useState(null)

    const handleSubmit = async (formData) => {
        setSubmitting(true)
        setError(null)

        try {
            await createConfiguracion(formData)
            // Redirect to home after successful configuration
            window.location.href = '/'
        } catch (err) {
            console.error('Error en onboarding:', err)
            setError(err.message || 'Ocurrió un error al guardar la configuración.')
        } finally {
            setSubmitting(false)
        }
    }

    if (loading) {
        return (
            <div className="min-h-screen flex items-center justify-center bg-gray-50">
                <div className="text-center">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto"></div>
                    <p className="mt-4 text-gray-600">Cargando configuración...</p>
                </div>
            </div>
        )
    }

    return (
        <div className="min-h-screen bg-gray-50 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
            <div className="sm:mx-auto sm:w-full sm:max-w-md">
                <div className="mx-auto h-12 w-12 bg-indigo-100 rounded-full flex items-center justify-center">
                    <svg className="h-8 w-8 text-indigo-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                    </svg>
                </div>
                <h2 className="mt-6 text-center text-3xl font-extrabold text-gray-900">
                    Bienvenido al Sistema
                </h2>
                <p className="mt-2 text-center text-sm text-gray-600">
                    Para comenzar, necesitamos configurar los datos de facturación en AFIP.
                </p>
            </div>

            <div className="mt-8 sm:mx-auto sm:w-full sm:max-w-3xl">
                {error && (
                    <div className="mb-4 bg-red-50 border-l-4 border-red-400 p-4">
                        <div className="flex">
                            <div className="flex-shrink-0">
                                <svg className="h-5 w-5 text-red-400" fill="currentColor" viewBox="0 0 20 20">
                                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
                                </svg>
                            </div>
                            <div className="ml-3">
                                <p className="text-sm text-red-700">{error}</p>
                            </div>
                        </div>
                    </div>
                )}

                {/* Usamos el modal pero renderizado inline o como modal fijo */}
                <AfipConfiguracionModal
                    show={true}
                    editingConfig={null}
                    condicionesIva={condicionesIva}
                    onClose={() => { }} // No hacer nada al cerrar
                    onSubmit={handleSubmit}
                    submitting={submitting}
                    isOnboarding={true}
                />
            </div>
        </div>
    )
}

export default AfipOnboarding
