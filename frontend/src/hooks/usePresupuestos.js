import { useState, useEffect } from "react";
import { API_BASE_URL } from "../config";
import { fetchWithAuth } from "../utils/authHeaders";

export function usePresupuestos() {
  const [clientes, setClientes] = useState([]);
  const [productos, setProductos] = useState([]);
  const [formasPago, setFormasPago] = useState([]);
  const [condicionesVenta, setCondicionesVenta] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Cargar datos iniciales
  useEffect(() => {
    fetchInitialData();
  }, []);

  const fetchInitialData = async () => {
    try {
      setLoading(true);
      setError(null);

      // Cargar clientes activos
      const clientesResponse = await fetchWithAuth(
        `${API_BASE_URL}/cliente?page=1&pageSize=1000`,
      );
      if (!clientesResponse.ok) throw new Error("Error al cargar clientes");
      const clientesData = await clientesResponse.json();
      setClientes(clientesData.data || []);

      // Cargar productos activos con stock
      const productosResponse = await fetchWithAuth(
        `${API_BASE_URL}/productos?page=1&pageSize=1000`,
      );
      if (!productosResponse.ok) throw new Error("Error al cargar productos");
      const productosData = await productosResponse.json();
      setProductos(productosData.data || []);

      // Cargar formas de pago (activas)
      const formasPagoResponse = await fetchWithAuth(
        `${API_BASE_URL}/formapago/activas`,
      );
      if (!formasPagoResponse.ok)
        throw new Error("Error al cargar formas de pago");
      const formasPagoData = await formasPagoResponse.json();
      setFormasPago(formasPagoData.data || formasPagoData || []);

      // Cargar condiciones de venta (activas)
      const condicionesVentaResponse = await fetchWithAuth(
        `${API_BASE_URL}/condicionventa`,
      );
      if (!condicionesVentaResponse.ok)
        throw new Error("Error al cargar condiciones de venta");
      const condicionesVentaData = await condicionesVentaResponse.json();
      setCondicionesVenta(
        condicionesVentaData.data || condicionesVentaData || [],
      );
    } catch (err) {
      setError(err.message);
      console.error("Error:", err);
    } finally {
      setLoading(false);
    }
  };

  const createPresupuesto = async (presupuestoData) => {
    try {
      const response = await fetchWithAuth(`${API_BASE_URL}/presupuesto`, {
        method: "POST",
        body: JSON.stringify(presupuestoData),
      });

      if (!response.ok) {
        const errorData = await response.json();
        const errorMessage =
          errorData.message ||
          errorData.errors?.[0] ||
          "Error al crear el presupuesto";
        throw new Error(errorMessage);
      }

      const data = await response.json();
      return data;
    } catch (err) {
      console.error("Error al crear presupuesto:", err);
      throw err;
    }
  };

  return {
    clientes,
    productos,
    formasPago,
    condicionesVenta,
    loading,
    error,
    createPresupuesto,
    refreshData: fetchInitialData,
  };
}

export function useNotification() {
  const [notification, setNotification] = useState(null);

  const showNotification = (type, message) => {
    setNotification({ show: true, type, message });
    setTimeout(() => setNotification(null), 5000);
  };

  const hideNotification = () => {
    setNotification(null);
  };

  return {
    notification,
    showNotification,
    hideNotification,
  };
}
