import { useState, useEffect } from "react";
import { API_BASE_URL } from "../config";
import { fetchWithAuth } from "../utils/authHeaders";

export function useComprobantesList() {
  const [comprobantes, setComprobantes] = useState([]);
  const [pagination, setPagination] = useState(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [pageSize, setPageSize] = useState(10);

  const [filtrosActivos, setFiltrosActivos] = useState({});

  useEffect(() => {
    fetchComprobantes(currentPage, filtrosActivos);
  }, [currentPage, filtrosActivos]);

  const fetchComprobantes = async (page = 1, currentFilters = {}, currentSize = pageSize) => {
    try {
      setLoading(true);
      setError(null);

      let queryParams = `?page=${page}&pageSize=${currentSize}`;

      // Construir el string de filtros (ignorando nulos o vacíos)
      Object.keys(currentFilters).forEach((key) => {
        if (
          currentFilters[key] !== null &&
          currentFilters[key] !== undefined &&
          currentFilters[key] !== ""
        ) {
          queryParams += `&${key}=${encodeURIComponent(currentFilters[key])}`;
        }
      });

      const response = await fetchWithAuth(
        `${API_BASE_URL}/comprobantes${queryParams}`,
      );

      if (!response.ok) {
        throw new Error("Error al cargar comprobantes");
      }

      const data = await response.json();
      setComprobantes(data.data || []);
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

  const applyFilters = (newFilters) => {
    const filtersWithoutPageSize = { ...newFilters };
    let targetSize = pageSize;
    if (filtersWithoutPageSize.pageSize !== undefined) {
      targetSize = filtersWithoutPageSize.pageSize;
      setPageSize(targetSize);
      delete filtersWithoutPageSize.pageSize;
    }
    setFiltrosActivos(filtersWithoutPageSize);
    setCurrentPage(1);
    fetchComprobantes(1, filtersWithoutPageSize, targetSize);
  };

  return {
    comprobantes,
    pagination,
    currentPage,
    loading,
    error,
    filtrosActivos,
    handlePageChange,
    applyFilters,
    pageSize,
    setPageSize: (size) => {
      setPageSize(size);
      fetchComprobantes(1, filtrosActivos, size);
    },
    refreshComprobantes: () => fetchComprobantes(currentPage, filtrosActivos, pageSize),
  };
}
