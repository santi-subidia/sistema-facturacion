import React, { useState } from 'react'
import { useUsuarios, useNotification } from './hooks/useUsuarios'
import Notification from './components/shared/Notification'
import UsuariosTable from './components/usuarios/UsuariosTable'
import UsuarioModal from './components/usuarios/UsuarioModal'
import DeleteConfirmModal from './components/shared/DeleteConfirmModal'
import Pagination from './components/shared/Pagination'

function Usuarios() {
  const {
    usuarios,
    pagination,
    currentPage,
    loading,
    error,
    roles,
    fetchUsuarios,
    createUsuario,
    updateUsuario,
    deleteUsuario,
    handlePageChange,
    pageSize,
    handlePageSizeChange
  } = useUsuarios()

  const { notification, showNotification, hideNotification } = useNotification()

  const [showModal, setShowModal] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const [editingUser, setEditingUser] = useState(null)
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false)
  const [userToDelete, setUserToDelete] = useState(null)

  const handleCloseModal = () => {
    setShowModal(false)
    setEditingUser(null)
  }

  const handleEditUser = (usuario) => {
    setEditingUser(usuario)
    setShowModal(true)
  }

  const handleDeleteClick = (usuario) => {
    setUserToDelete(usuario)
    setShowDeleteConfirm(true)
  }

  const handleModalSubmit = async (formData) => {
    setSubmitting(true)

    try {
      if (editingUser) {
        await updateUsuario(editingUser, formData)
        showNotification('success', '¡Usuario actualizado exitosamente!')
      } else {
        await createUsuario(formData)
        showNotification('success', '¡Usuario creado exitosamente!')
      }

      setShowModal(false)
      setEditingUser(null)
      fetchUsuarios(currentPage)
    } catch (err) {
      console.error('Error en handleModalSubmit:', err)
      showNotification('error', err.message)
    } finally {
      setSubmitting(false)
    }
  }

  const handleDeleteConfirm = async () => {
    try {
      await deleteUsuario(userToDelete.id)

      setShowDeleteConfirm(false)
      setUserToDelete(null)
      fetchUsuarios(currentPage)

      showNotification('success', '¡Usuario eliminado exitosamente!')
    } catch (err) {
      showNotification('error', 'Error al eliminar usuario: ' + err.message)
    }
  }

  return (
    <div className="space-y-6">
      <Notification
        notification={notification}
        onClose={hideNotification}
      />

      <div className="bg-white rounded-2xl shadow-sm border border-slate-200 overflow-hidden">
        <div className="px-6 py-4 border-b border-slate-200 flex justify-between items-center flex-wrap gap-4">
          <h2 className="text-xl font-semibold text-slate-800">Listado de Usuarios</h2>
          <div className="flex items-center gap-4">
            <div className="flex items-center space-x-2">
              <label htmlFor="pageSize" className="text-sm font-medium text-gray-700">Mostrar:</label>
              <select
                id="pageSize"
                value={pageSize}
                onChange={(e) => handlePageSizeChange(parseInt(e.target.value))}
                className="block w-full pl-3 pr-10 py-1.5 text-sm border-gray-300 focus:outline-none focus:ring-blue-500 focus:border-blue-500 rounded-md"
              >
                <option value={10}>10</option>
                <option value={20}>20</option>
                <option value={50}>50</option>
                <option value={100}>100</option>
              </select>
            </div>
            <button
              onClick={() => setShowModal(true)}
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-xl shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
            >
              <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
              </svg>
              Nuevo Usuario
            </button>
          </div>
        </div>

        <UsuariosTable
          usuarios={usuarios}
          loading={loading}
          error={error}
          onEdit={handleEditUser}
          onDelete={handleDeleteClick}
        />

        <Pagination
          pagination={pagination}
          currentPage={currentPage}
          onPageChange={handlePageChange}
          itemName="usuarios"
        />
      </div>

      <UsuarioModal
        show={showModal}
        editingUser={editingUser}
        roles={roles}
        onClose={handleCloseModal}
        onSubmit={handleModalSubmit}
        submitting={submitting}
      />

      <DeleteConfirmModal
        show={showDeleteConfirm}
        item={userToDelete}
        itemType="Usuario"
        itemName={userToDelete?.nombre}
        onConfirm={handleDeleteConfirm}
        onCancel={() => {
          setShowDeleteConfirm(false)
          setUserToDelete(null)
        }}
      />
    </div>
  )
}

export default Usuarios
