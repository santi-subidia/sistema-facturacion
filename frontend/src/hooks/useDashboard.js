import { useState, useEffect, useCallback } from 'react';
import { useAuth } from './useAuth';
import { API_BASE_URL } from '../config';

export const useDashboard = () => {
    const { getToken, logout } = useAuth();
    const token = getToken();
    const [data, setData] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const fetchDashboardData = useCallback(async () => {
        const currentToken = getToken();
        if (!currentToken) return;

        try {
            setLoading(true);
            setError(null);

            const response = await fetch(`${API_BASE_URL}/Dashboard`, {
                headers: {
                    'Authorization': `Bearer ${currentToken}`
                }
            });

            if (!response.ok) {
                if (response.status === 401 || response.status === 403) {
                    logout();
                    throw new Error('Sesión expirada o permisos insuficientes');
                }
                throw new Error('Error al cargar datos del dashboard');
            }

            const jsonData = await response.json();
            setData(jsonData);
        } catch (err) {
            console.error('Error fetching dashboard:', err);
            setError(err.message || 'Ocurrió un error inesperado');
        } finally {
            setLoading(false);
        }
    }, [getToken, logout]);

    useEffect(() => {
        fetchDashboardData();

        // Polling cada 5 minutos
        const interval = setInterval(() => {
            fetchDashboardData();
        }, 5 * 60 * 1000);

        return () => clearInterval(interval);
    }, [fetchDashboardData]);

    return {
        data,
        loading,
        error,
        refetch: fetchDashboardData
    };
};
