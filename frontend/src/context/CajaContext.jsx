import React, { createContext, useContext, useState, useCallback, useEffect } from 'react';
import { API_BASE_URL } from '../config';
import { fetchWithAuth } from '../utils/authHeaders';

const CajaContext = createContext();

export const CajaProvider = ({ children }) => {
  const [sesionActiva, setSesionActiva] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const fetchSesionActiva = useCallback(async () => {
    try {
      setLoading(true);
      const response = await fetchWithAuth(`${API_BASE_URL}/Caja/activa`);
      if (response.status === 404 || response.status === 204) {
        setSesionActiva(null);
        return null;
      }

      const text = await response.text();
      if (!text) {
        setSesionActiva(null);
        return null;
      }

      const data = JSON.parse(text);
      if (response.ok) {
        setSesionActiva(data);
        return data;
      } else {
        throw new Error(data.message || 'Error al obtener sesión activa');
      }
    } catch (err) {
      console.error('CajaContext: fetchSesionActiva error', err);
      setSesionActiva(null);
      return null;
    } finally {
      setLoading(false);
    }
  }, []);

  // Abrir caja y actualizar estado global
  const abrirCajaGlobal = async (cajaId, montoInicial) => {
    try {
      setLoading(true);
      const response = await fetchWithAuth(`${API_BASE_URL}/Caja/abrir`, {
        method: 'POST',
        body: JSON.stringify({ cajaId, montoInicial }),
      });
      const data = await response.json();
      if (!response.ok) throw new Error(data.message || 'Error al abrir caja');
      setSesionActiva(data);
      return data;
    } catch (err) {
      setError(err.message);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Cerrar caja y limpiar estado global
  const cerrarCajaGlobal = async (sesionCajaId, montoCierreReal) => {
    try {
      setLoading(true);
      const response = await fetchWithAuth(`${API_BASE_URL}/Caja/cerrar`, {
        method: 'POST',
        body: JSON.stringify({ sesionCajaId, montoCierreReal }),
      });
      const data = await response.json();
      if (!response.ok) throw new Error(data.message || 'Error al cerrar caja');
      setSesionActiva(null);
      return data;
    } catch (err) {
      setError(err.message);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  // Cargar sesión activa al montar (si hay token)
  useEffect(() => {
    const token = localStorage.getItem('token');
    if (token) {
      fetchSesionActiva();
    }
  }, [fetchSesionActiva]);

  return (
    <CajaContext.Provider value={{ 
      sesionActiva, 
      loading, 
      error, 
      fetchSesionActiva, 
      abrirCajaGlobal, 
      cerrarCajaGlobal 
    }}>
      {children}
    </CajaContext.Provider>
  );
};

export const useCajaGlobal = () => {
  const context = useContext(CajaContext);
  if (!context) {
    throw new Error('useCajaGlobal must be used within a CajaProvider');
  }
  return context;
};
