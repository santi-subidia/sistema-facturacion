import React from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../../hooks/useAuth'
import { usePermissions } from '../../hooks/usePermissions'
import LoadingFallback from './LoadingFallback'

function ProtectedRoute({ children, requireAdmin = false, requireAfipConfig = true }) {
    const { isAuthenticated, isLoading, hasAfipConfig } = useAuth()
    const { canManageUsers } = usePermissions()
    const location = useLocation()

    // Show loading while checking auth
    if (isLoading) {
        return <LoadingFallback />
    }

    // Redirect to login if not authenticated
    if (!isAuthenticated) {
        return <Navigate to="/login" state={{ from: location }} replace />
    }

    // Check AFIP config
    if (requireAfipConfig && hasAfipConfig === null) {
        return <LoadingFallback />
    }

    // Redirect to onboarding if no AFIP config
    if (requireAfipConfig && !hasAfipConfig) {
        return <Navigate to="/onboarding" replace />
    }

    // Check admin permission
    if (requireAdmin && !canManageUsers) {
        return <Navigate to="/comprobantes" replace />
    }

    return children
}

export default ProtectedRoute
