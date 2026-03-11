import { useState, useEffect, useCallback } from 'react';
import { API_BASE_URL } from '../config';

export function useConnectivity() {
    const [isOnline, setIsOnline] = useState(navigator.onLine);
    const [isAfipOnline, setIsAfipOnline] = useState(true);

    // Chequea si AFIP responde y si el backend local responde
    const checkBackendAndAfip = useCallback(async () => {
        // Si el navegador dice que no hay red, ni lo intentamos
        if (!navigator.onLine) {
            setIsOnline(false);
            setIsAfipOnline(false);
            return;
        }

        try {
            // 1. Backend ping
            const resBackend = await fetch(`${API_BASE_URL}/health`, { method: 'GET' });
            const backendOk = resBackend.ok;
            setIsOnline(backendOk);

            if (!backendOk) {
                setIsAfipOnline(false);
                return;
            }

            // 2. AFIP ping
            const resAfip = await fetch(`${API_BASE_URL}/health/afip`, { method: 'GET' });
            if (resAfip.ok) {
                const data = await resAfip.json();
                setIsAfipOnline(data.isAfipOnline);
            } else {
                setIsAfipOnline(false);
            }
        } catch (err) {
            // Si el fetch falla (net::ERR_CONNECTION_REFUSED, etc), significa que el backend no responde
            setIsOnline(false);
            setIsAfipOnline(false);
        }
    }, []);

    useEffect(() => {
        // Escuchar eventos nivel navegador
        const handleOnline = () => {
            setIsOnline(true);
            checkBackendAndAfip();
        };

        const handleOffline = () => {
            setIsOnline(false);
            setIsAfipOnline(false);
        };

        window.addEventListener('online', handleOnline);
        window.addEventListener('offline', handleOffline);

        // Chequeo inicial
        checkBackendAndAfip();

        // Polling cada 3 minutos por si se cae el enlace pero el WiFi sigue conectado
        const interval = setInterval(checkBackendAndAfip, 3 * 60 * 1000);

        return () => {
            window.removeEventListener('online', handleOnline);
            window.removeEventListener('offline', handleOffline);
            clearInterval(interval);
        };
    }, [checkBackendAndAfip]);

    return { isOnline, isAfipOnline, checkBackendAndAfip };
}
