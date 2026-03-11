import React, { lazy } from 'react'
import { Navigate } from 'react-router-dom'
import ProtectedRoute from './components/routing/ProtectedRoute'
import MainLayout from './components/layout/MainLayout'

// Lazy load all page components for code splitting
const Login = lazy(() => import('./components/auth/Login'))
const AfipOnboarding = lazy(() => import('./components/onboarding/AfipOnboarding'))
const Dashboard = lazy(() => import('./Dashboard'))
const Comprobantes = lazy(() => import('./Comprobantes'))
const ComprobantesLista = lazy(() => import('./ComprobantesLista'))
const NotaCredito = lazy(() => import('./NotaCredito'))
const Clientes = lazy(() => import('./Clientes'))
const Productos = lazy(() => import('./Productos'))
const Usuarios = lazy(() => import('./Usuarios'))
const AfipConfiguracion = lazy(() => import('./AfipConfiguracion'))
const Perfil = lazy(() => import('./Perfil'))
const Presupuestos = lazy(() => import('./Presupuestos'))
const PresupuestosLista = lazy(() => import('./PresupuestosLista'))
const CajaSesion = lazy(() => import('./CajaSesion'))
const CajaAdministrar = lazy(() => import('./CajaAdministrar'))
const CajaHistorial = lazy(() => import('./CajaHistorial'))
const NotFound = lazy(() => import('./pages/NotFound'))

const routes = [
    {
        path: '/login',
        element: <Login />
    },
    {
        path: '/onboarding',
        element: (
            <ProtectedRoute requireAfipConfig={false}>
                <AfipOnboarding />
            </ProtectedRoute>
        )
    },
    {
        path: '/',
        element: (
            <ProtectedRoute>
                <MainLayout />
            </ProtectedRoute>
        ),
        children: [
            {
                index: true,
                element: <Dashboard />
            },
            {
                path: 'comprobantes',
                element: <Comprobantes />
            },
            {
                path: 'comprobantes/lista',
                element: <ComprobantesLista />
            },
            {
                path: 'comprobantes/nota-credito',
                element: <NotaCredito />
            },
            {
                path: 'presupuestos',
                element: <Presupuestos />
            },
            {
                path: 'presupuestos/lista',
                element: <PresupuestosLista />
            },
            {
                path: 'caja',
                element: <CajaSesion />
            },
            {
                path: 'caja/administrar',
                element: (
                    <ProtectedRoute requireAdmin={true}>
                        <CajaAdministrar />
                    </ProtectedRoute>
                )
            },
            {
                path: 'caja/historial',
                element: <CajaHistorial />
            },
            {
                path: 'clientes',
                element: <Clientes />
            },
            {
                path: 'productos',
                element: <Productos />
            },
            {
                path: 'usuarios',
                element: (
                    <ProtectedRoute requireAdmin={true}>
                        <Usuarios />
                    </ProtectedRoute>
                )
            },
            {
                path: 'afip-config',
                element: (
                    <ProtectedRoute requireAdmin={true}>
                        <AfipConfiguracion />
                    </ProtectedRoute>
                )
            },
            {
                path: 'perfil',
                element: <Perfil />
            }
        ]
    },
    {
        path: '*',
        element: <NotFound />
    }
]

export default routes
