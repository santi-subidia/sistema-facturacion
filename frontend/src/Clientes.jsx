import React, { useState } from "react";
import { useAfipConfiguracion } from "./hooks/useAfipConfiguracion";
import { useClientes, useNotification } from "./hooks/useClientes";
import { usePermissions } from "./hooks/usePermissions";
import Notification from "./components/shared/Notification";
import ClientesTable from "./components/clientes/ClientesTable";
import ClienteModal from "./components/clientes/ClienteModal";
import ClientesFilters from "./components/clientes/ClientesFilters";
import DeleteConfirmModal from "./components/shared/DeleteConfirmModal";
import Pagination from "./components/shared/Pagination";

function Clientes() {
  const {
    clientes,
    pagination,
    currentPage,
    loading,
    error,
    filters,
    fetchClientes,
    createCliente,
    updateCliente,
    deleteCliente,
    handlePageChange,
    applyFilters,
    clearFilters
  } = useClientes();

  const { condicionesIva } = useAfipConfiguracion();
  const { notification, showNotification, hideNotification } =
    useNotification();
  const { canDelete } = usePermissions();

  const [showModal, setShowModal] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [editingCliente, setEditingCliente] = useState(null);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [clienteToDelete, setClienteToDelete] = useState(null);

  const handleCloseModal = () => {
    setShowModal(false);
    setEditingCliente(null);
  };

  const handleEditCliente = (cliente) => {
    setEditingCliente(cliente);
    setShowModal(true);
  };

  const handleDeleteClick = (cliente) => {
    setClienteToDelete(cliente);
    setShowDeleteConfirm(true);
  };

  const handleModalSubmit = async (formData) => {
    setSubmitting(true);

    try {
      if (editingCliente) {
        await updateCliente(editingCliente, formData);
        showNotification("success", "¡Cliente actualizado exitosamente!");
      } else {
        await createCliente(formData);
        showNotification("success", "¡Cliente creado exitosamente!");
      }

      setShowModal(false);
      setEditingCliente(null);
      fetchClientes(currentPage);
    } catch (err) {
      console.error("Error en handleModalSubmit:", err);
      showNotification("error", err.message);
    } finally {
      setSubmitting(false);
    }
  };

  const handleDeleteConfirm = async () => {
    try {
      await deleteCliente(clienteToDelete.id);

      setShowDeleteConfirm(false);
      setClienteToDelete(null);
      fetchClientes(currentPage);

      showNotification("success", "¡Cliente eliminado exitosamente!");
    } catch (err) {
      showNotification("error", "Error al eliminar cliente: " + err.message);
    }
  };

  return (
    <div className="space-y-6">
      <Notification notification={notification} onClose={hideNotification} />

      <div className="bg-white rounded-2xl shadow-sm border border-slate-200 overflow-hidden">
        <div className="px-6 py-4 border-b border-slate-200 flex justify-between items-center flex-wrap gap-4">
          <h2 className="text-xl font-semibold text-slate-800">
            Listado de Clientes
          </h2>
          <div className="flex items-center gap-4">
            <button
              onClick={() => setShowModal(true)}
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-xl shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
            >
              <svg
                className="w-5 h-5 mr-2"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M12 6v6m0 0v6m0-6h6m-6 0H6"
                />
              </svg>
              Nuevo Cliente
            </button>
          </div>
        </div>

        <ClientesFilters
          filters={filters}
          onApplyFilters={applyFilters}
          onClearFilters={clearFilters}
        />

        <ClientesTable
          clientes={clientes}
          loading={loading}
          error={error}
          onEdit={handleEditCliente}
          onDelete={handleDeleteClick}
          canDelete={canDelete}
        />

        <Pagination
          pagination={pagination}
          currentPage={currentPage}
          onPageChange={handlePageChange}
          itemName="clientes"
        />
      </div>

      <ClienteModal
        show={showModal}
        editingCliente={editingCliente}
        condicionesIva={condicionesIva}
        onClose={handleCloseModal}
        onSubmit={handleModalSubmit}
        submitting={submitting}
      />

      <DeleteConfirmModal
        show={showDeleteConfirm}
        item={clienteToDelete}
        itemType="Cliente"
        itemName={
          clienteToDelete
            ? `${clienteToDelete.nombre} ${clienteToDelete.apellido}`
            : ""
        }
        onConfirm={handleDeleteConfirm}
        onCancel={() => {
          setShowDeleteConfirm(false);
          setClienteToDelete(null);
        }}
      />
    </div>
  );
}

export default Clientes;
