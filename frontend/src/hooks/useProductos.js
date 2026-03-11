import { useState, useEffect } from "react";
import { API_BASE_URL } from "../config";
import { fetchWithAuth, fetchWithAuthMultipart } from "../utils/authHeaders";

export function useProductos() {
  const [productos, setProductos] = useState([]);
  const [pagination, setPagination] = useState(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [filters, setFilters] = useState({
    search: "",
    proveedor: "",
    sinStock: false,
    pageSize: 10,
  });

  const fetchProductos = async (page = 1, currentFilters = filters) => {
    try {
      setLoading(true);
      setError(null);

      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: (currentFilters.pageSize || 10).toString(),
      });

      if (currentFilters.search) {
        params.append("search", currentFilters.search);
      }
      if (currentFilters.proveedor) {
        params.append("proveedor", currentFilters.proveedor);
      }
      if (currentFilters.sinStock) {
        params.append("sinStock", "true");
      }

      const response = await fetchWithAuth(`${API_BASE_URL}/productos?${params}`);

      if (!response.ok) {
        throw new Error("Error al cargar productos");
      }

      const data = await response.json();
      setProductos(data.data || []);
      setPagination(data.pagination);
      setCurrentPage(page);
    } catch (err) {
      setError(err.message);
      console.error("Error:", err);
    } finally {
      setLoading(false);
    }
  };

  const applyFilters = (newFilters) => {
    setFilters(newFilters);
    fetchProductos(1, newFilters);
  };

  const clearFilters = () => {
    const emptyFilters = {
      search: "",
      proveedor: "",
      sinStock: false,
      pageSize: filters.pageSize,
    };
    setFilters(emptyFilters);
    fetchProductos(1, emptyFilters);
  };

  const createProducto = async (formData) => {
    const user = JSON.parse(localStorage.getItem("user") || "{}");
    const dataToSend = {
      nombre: formData.nombre,
      codigo: formData.codigo,
      precio: parseFloat(formData.precio),
      stock: parseInt(formData.stock),
      stockNegro: formData.stockNegro ? parseInt(formData.stockNegro) : null,
      proveedor: formData.proveedor || "",
      idCreado_por: user.id || 1,
    };

    const response = await fetchWithAuth(`${API_BASE_URL}/productos`, {
      method: "POST",
      body: JSON.stringify(dataToSend),
    });

    if (!response.ok) {
      const errorData = await response.json();
      let errorMessage = "Error de validación: ";
      if (errorData.errors) {
        if (Array.isArray(errorData.errors)) {
          errorMessage += errorData.errors.join("; ");
        } else {
          const errorMessages = Object.entries(errorData.errors).map(
            ([field, messages]) => {
              // Si el field es un índice numérico (0, 1, etc), solo mostramos el mensaje
              if (!isNaN(field)) {
                return Array.isArray(messages) ? messages.join(", ") : messages;
              }
              return `${field}: ${Array.isArray(messages) ? messages.join(", ") : messages}`;
            },
          );
          errorMessage += errorMessages.join("; ");
        }
      } else {
        errorMessage =
          errorData.title || errorData.message || "Error al crear producto";
      }
      throw new Error(errorMessage);
    }

    return response.json();
  };

  const updateProducto = async (producto, formData) => {
    const dataToSend = {
      id: producto.id,
      nombre: formData.nombre,
      codigo: formData.codigo,
      precio: parseFloat(formData.precio),
      stock: parseInt(formData.stock),
      stockNegro:
        formData.stockNegro !== undefined
          ? formData.stockNegro === ""
            ? null
            : parseInt(formData.stockNegro)
          : producto.stockNegro,
      proveedor: formData.proveedor || "",
      idCreado_por: producto.idCreado_por,
      creado_at: producto.creado_at,
      eliminado_at: producto.eliminado_at || null,
      idEliminado_por: producto.idEliminado_por || null,
    };

    const response = await fetchWithAuth(`${API_BASE_URL}/productos/${producto.id}`, {
      method: "PUT",
      body: JSON.stringify(dataToSend),
    });

    if (!response.ok) {
      const errorData = await response.json();
      let errorMessage = "Error de validación: ";
      if (errorData.errors) {
        if (Array.isArray(errorData.errors)) {
          errorMessage += errorData.errors.join("; ");
        } else {
          const errorMessages = Object.entries(errorData.errors).map(
            ([field, messages]) => {
              // Si el field es un índice numérico (0, 1, etc), solo mostramos el mensaje
              if (!isNaN(field)) {
                return Array.isArray(messages) ? messages.join(", ") : messages;
              }
              return `${field}: ${Array.isArray(messages) ? messages.join(", ") : messages}`;
            },
          );
          errorMessage += errorMessages.join("; ");
        }
      } else {
        errorMessage =
          errorData.title ||
          errorData.message ||
          "Error al actualizar producto";
      }
      throw new Error(errorMessage);
    }

    if (response.status === 204) {
      return null;
    }

    return response.json();
  };

  const deleteProducto = async (productoId) => {
    const response = await fetchWithAuth(`${API_BASE_URL}/productos/${productoId}`, {
      method: "DELETE",
    });

    if (!response.ok) {
      throw new Error("Error al eliminar producto");
    }

    if (response.status === 204) {
      return null;
    }

    return response.json();
  };

  const ajusteMasivo = async (productosIds, porcentaje, redondeo = null) => {
    const dataToSend = {
      productosIds,
      porcentaje,
      redondeo,
    };

    const response = await fetchWithAuth(`${API_BASE_URL}/productos/ajuste-masivo`, {
      method: "POST",
      body: JSON.stringify(dataToSend),
    });

    if (!response.ok) {
      const errorData = await response.json();
      let errorMessage = "Error al aplicar aumento: ";
      if (errorData.errors) {
        errorMessage += errorData.errors.join("; ");
      } else {
        errorMessage = errorData.message || "Error al aplicar aumento masivo";
      }
      throw new Error(errorMessage);
    }

    return response.json();
  };

  const ajusteStock = async (ajustes) => {
    const dataToSend = {
      ajustes: ajustes.map((ajuste) => ({
        id: ajuste.id,
        tipoAjuste: ajuste.tipoAjuste,
        cantidad: ajuste.cantidad,
        stockNuevo: ajuste.stockNuevo,
        esStockNegro: ajuste.esStockNegro || false,
      })),
    };

    const response = await fetchWithAuth(`${API_BASE_URL}/productos/ajuste-stock`, {
      method: "POST",
      body: JSON.stringify(dataToSend),
    });

    if (!response.ok) {
      const errorData = await response.json();
      let errorMessage = "Error al ajustar stock: ";
      if (errorData.errors) {
        errorMessage += Array.isArray(errorData.errors)
          ? errorData.errors.join("; ")
          : JSON.stringify(errorData.errors);
      } else {
        errorMessage = errorData.message || "Error al ajustar stock";
      }
      throw new Error(errorMessage);
    }

    return response.json();
  };

  const importarProductos = async (archivo, accionExistentes) => {
    const formData = new FormData();
    formData.append("archivo", archivo);
    formData.append("accionExistentes", accionExistentes);

    const response = await fetchWithAuthMultipart(`${API_BASE_URL}/productos/importar`, {
      method: "POST",
      body: formData,
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      let errorMessage = "Error al importar productos: ";
      if (errorData.errors && Array.isArray(errorData.errors)) {
        errorMessage += errorData.errors.join("; ");
      } else {
        errorMessage +=
          errorData.message || "Ocurrió un error inesperado al importar.";
      }
      throw new Error(errorMessage);
    }

    return response.json();
  };

  useEffect(() => {
    fetchProductos(1);
  }, []);

  const handlePageChange = (newPage) => {
    if (newPage >= 1 && newPage <= (pagination?.totalPages || 1)) {
      fetchProductos(newPage);
    }
  };

  return {
    productos,
    pagination,
    currentPage,
    loading,
    error,
    filters,
    fetchProductos,
    createProducto,
    updateProducto,
    deleteProducto,
    ajusteMasivo,
    ajusteStock,
    importarProductos,
    handlePageChange,
    applyFilters,
    clearFilters,
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
