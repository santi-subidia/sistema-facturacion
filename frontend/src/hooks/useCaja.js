import { useState, useCallback } from 'react';
import { API_BASE_URL } from '../config';
import { fetchWithAuth } from '../utils/authHeaders';
import { useCajaGlobal } from '../context/CajaContext';

export function useCaja() {
  const { 
    sesionActiva, 
    fetchSesionActiva, 
    abrirCajaGlobal, 
    cerrarCajaGlobal,
    loading: loadingGlobal,
    error: errorGlobal
  } = useCajaGlobal();

  const [movimientos, setMovimientos] = useState([]);
  const [cajas, setCajas] = useState([]);
  const [puntosVentaAfip, setPuntosVentaAfip] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const fetchCajas = useCallback(async () => {
    try {
      setLoading(true);
      const response = await fetchWithAuth(`${API_BASE_URL}/Caja`);
      if (!response.ok) throw new Error('Error al obtener cajas');
      const data = await response.json();
      setCajas(data);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, []);

  const fetchPuntosVentaAfip = useCallback(async () => {
    try {
      setLoading(true);
      const response = await fetchWithAuth(`${API_BASE_URL}/afip/puntos-venta`);
      if (!response.ok) throw new Error('Error al obtener Puntos de Venta AFIP');
      const data = await response.json();
      setPuntosVentaAfip(data.data || []);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, []);

  const abrirCaja = async (cajaId, montoInicial) => {
    return await abrirCajaGlobal(cajaId, montoInicial);
  };

  const cerrarCaja = async (sesionCajaId, montoCierreReal) => {
    return await cerrarCajaGlobal(sesionCajaId, montoCierreReal);
  };

  const obtenerMovimientos = useCallback(async (sesionCajaId) => {
    try {
      setLoading(true);
      const response = await fetchWithAuth(`${API_BASE_URL}/Caja/sesiones/${sesionCajaId}/movimientos`);
      if (!response.ok) throw new Error('Error al obtener movimientos');
      const data = await response.json();
      setMovimientos(data);
      return data;
    } catch (err) {
      setError(err.message);
      return [];
    } finally {
      setLoading(false);
    }
  }, []);

  const agregarMovimiento = async (sesionCajaId, tipo, monto, concepto) => {
    try {
      setLoading(true);
      const response = await fetchWithAuth(`${API_BASE_URL}/Caja/movimientos`, {
        method: 'POST',
        body: JSON.stringify({ sesionCajaId, tipo, monto, concepto }),
      });
      const data = await response.json();
      if (!response.ok) throw new Error(data.message || 'Error al agregar movimiento');
      return data;
    } catch (err) {
      setError(err.message);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const fetchDetalleSesion = useCallback(async (sesionId) => {
    try {
      const response = await fetchWithAuth(`${API_BASE_URL}/Caja/sesiones/${sesionId}/detalle`);
      if (!response.ok) throw new Error('Error al obtener detalle de la sesión');
      return await response.json();
    } catch (err) {
      setError(err.message);
      return null;
    }
  }, []);

  const fetchHistorialSesiones = useCallback(async (page = 1, pageSize = 10) => {
    try {
      setLoading(true);
      const response = await fetchWithAuth(`${API_BASE_URL}/Caja/historial?page=${page}&pageSize=${pageSize}`);
      if (!response.ok) throw new Error('Error al obtener historial de sesiones');
      const data = await response.json();
      return data;
    } catch (err) {
      setError(err.message);
      return [];
    } finally {
      setLoading(false);
    }
  }, []);

  const calcularArqueo = async (sesionCajaId) => {
    try {
      setLoading(true);
      const response = await fetchWithAuth(`${API_BASE_URL}/Caja/sesiones/${sesionCajaId}/arqueo`);
      const data = await response.json();
      if (!response.ok) throw new Error(data.message || 'Error al calcular arqueo');
      return data.montoCierreSistema;
    } catch (err) {
      setError(err.message);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const crearCaja = async (cajaData) => {
    try {
      setLoading(true);
      const response = await fetchWithAuth(`${API_BASE_URL}/Caja`, {
        method: 'POST',
        body: JSON.stringify(cajaData),
      });
      const data = await response.json();
      if (!response.ok) throw new Error(data.message || 'Error al crear caja');
      await fetchCajas(); // Refresh list
      return data;
    } catch (err) {
      setError(err.message);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const actualizarCaja = async (id, cajaData) => {
    try {
      setLoading(true);
      const response = await fetchWithAuth(`${API_BASE_URL}/Caja/${id}`, {
        method: 'PUT',
        body: JSON.stringify(cajaData),
      });
      const data = await response.json();
      if (!response.ok) throw new Error(data.message || 'Error al actualizar caja');
      await fetchCajas(); // Refresh list
      return data;
    } catch (err) {
      setError(err.message);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  return {
    cajas,
    sesionActiva,
    movimientos,
    puntosVentaAfip,
    loading: loading || loadingGlobal,
    error: error || errorGlobal,
    fetchCajas,
    fetchPuntosVentaAfip,
    fetchSesionActiva,
    fetchHistorialSesiones,
    fetchDetalleSesion,
    abrirCaja,
    cerrarCaja,
    obtenerMovimientos,
    agregarMovimiento,
    calcularArqueo,
    crearCaja,
    actualizarCaja
  };
}
