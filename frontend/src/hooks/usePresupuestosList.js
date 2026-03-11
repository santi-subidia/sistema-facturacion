import { useState, useEffect } from "react";
import { API_BASE_URL } from "../config";
import { fetchWithAuth } from "../utils/authHeaders";

export function usePresupuestosList() {
  const [presupuestos, setPresupuestos] = useState([]);
  const [pagination, setPagination] = useState(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [estados, setEstados] = useState([]);
  const [pageSize, setPageSize] = useState(10);

  useEffect(() => {
    fetchPresupuestos(currentPage);
    fetchEstados();
  }, [currentPage]);

  const fetchEstados = async () => {
    try {
      const response = await fetchWithAuth(`${API_BASE_URL}/presupuesto/estados`);
      if (response.ok) {
        const data = await response.json();
        setEstados(data);
      }
    } catch (err) {
      console.error("Error al cargar estados:", err);
    }
  };

  const fetchPresupuestos = async (page = 1, filters = {}) => {
    try {
      setLoading(true);
      setError(null);

      // Limpiar filtros vacíos
      const cleanFilters = Object.fromEntries(
        Object.entries(filters).filter(
          ([_, v]) => v !== "" && v !== null && v !== undefined,
        ),
      );

      const queryParams = new URLSearchParams({
        page,
        pageSize: pageSize,
        ...cleanFilters,
      });

      const response = await fetchWithAuth(
        `${API_BASE_URL}/presupuesto?${queryParams.toString()}`,
      );

      if (!response.ok) {
        throw new Error("Error al cargar presupuestos");
      }

      const data = await response.json();
      setPresupuestos(data.data || []);
      setPagination(data.pagination);
      setCurrentPage(page);
    } catch (err) {
      setError(err.message);
      console.error("Error:", err);
    } finally {
      setLoading(false);
    }
  };

  const handlePageChange = (newPage) => {
    setCurrentPage(newPage);
  };

  const handlePageSizeChange = (newSize) => {
    setPageSize(newSize);
    // Refresh is triggered by next effect or we could do it directly if we changed dependencies
    // But since fetchPresupuestos depends on pagination it's better to fetch it directly
    setCurrentPage(1);
    fetchPresupuestos(1, {}, newSize); // We might not have access to filters here, unless we pass them 
  };


  const convertirAComprobante = async (presupuestoId, dto) => {
    const response = await fetchWithAuth(
      `${API_BASE_URL}/presupuesto/${presupuestoId}/convertir-comprobante`,
      {
        method: "POST",
        body: JSON.stringify(dto),
      },
    );

    if (!response.ok) {
      let errorData;
      try {
        errorData = await response.json();
      } catch {
        throw new Error("Error al conectar con el servidor");
      }
      throw new Error(
        errorData.message || "Error al convertir presupuesto a comprobante",
      );
    }

    return await response.json();
  };

  const cambiarEstado = async (presupuestoId, nuevoEstado) => {
    const response = await fetchWithAuth(
      `${API_BASE_URL}/presupuesto/${presupuestoId}/estado`,
      {
        method: "PUT",
        body: JSON.stringify({ nuevoEstado }),
      },
    );

    if (!response.ok) {
      let errorData;
      try {
        errorData = await response.json();
      } catch {
        throw new Error("Error al conectar con el servidor");
      }
      throw new Error(
        errorData.message || "Error al cambiar el estado del presupuesto",
      );
    }

    return await response.json();
  };

  return {
    presupuestos,
    pagination,
    currentPage,
    loading,
    error,
    estados,
    handlePageChange,
    fetchPresupuestos,
    fetchEstados,
    convertirAComprobante,
    cambiarEstado,
    pageSize,
    setPageSize,
  };
}
