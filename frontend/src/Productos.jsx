import React, { useState } from 'react'
import { useProductos, useNotification } from './hooks/useProductos'
import { usePermissions } from './hooks/usePermissions'
import Notification from './components/shared/Notification'
import ProductosTable from './components/productos/ProductosTable'
import ProductosFilters from './components/productos/ProductosFilters'
import ProductoModal from './components/productos/ProductoModal'
import DeleteConfirmModal from './components/shared/DeleteConfirmModal'
import AjusteMasivoModal from './components/productos/AjusteMasivoModal'
import AjusteStockModal from './components/productos/AjusteStockModal'
import ImportarProductosModal from './components/ImportarProductosModal'
import SelectionBar from './components/productos/SelectionBar'
import Pagination from './components/shared/Pagination'

function Productos() {
  const {
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
    clearFilters
  } = useProductos()

  const { notification, showNotification, hideNotification } = useNotification()
  const { canDelete, isAdmin } = usePermissions()

  const [showModal, setShowModal] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const [editingProduct, setEditingProduct] = useState(null)
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false)
  const [productToDelete, setProductToDelete] = useState(null)

  // Estados para ajuste masivo
  const [selectionMode, setSelectionMode] = useState(false)
  const [modoSeleccion, setModoSeleccion] = useState(null) // 'precio' o 'stock'
  const [selectedIds, setSelectedIds] = useState([])
  const [showAjusteModal, setShowAjusteModal] = useState(false)
  const [showAjusteStockModal, setShowAjusteStockModal] = useState(false)
  const [showImportarModal, setShowImportarModal] = useState(false)
  const [submittingAjuste, setSubmittingAjuste] = useState(false)

  const handleCloseModal = () => {
    setShowModal(false)
    setEditingProduct(null)
  }

  const handleEditProduct = (producto) => {
    setEditingProduct(producto)
    setShowModal(true)
  }

  const handleDeleteClick = (producto) => {
    setProductToDelete(producto)
    setShowDeleteConfirm(true)
  }

  const handleModalSubmit = async (formData) => {
    setSubmitting(true)

    try {
      if (editingProduct) {
        await updateProducto(editingProduct, formData)
        showNotification('success', '¡Producto actualizado exitosamente!')
      } else {
        await createProducto(formData)
        showNotification('success', '¡Producto creado exitosamente!')
      }

      setShowModal(false)
      setEditingProduct(null)
      fetchProductos(currentPage)
    } catch (err) {
      console.error('Error en handleModalSubmit:', err)
      showNotification('error', err.message)
    } finally {
      setSubmitting(false)
    }
  }

  const handleDeleteConfirm = async () => {
    try {
      await deleteProducto(productToDelete.id)

      setShowDeleteConfirm(false)
      setProductToDelete(null)
      fetchProductos(currentPage)

      showNotification('success', '¡Producto eliminado exitosamente!')
    } catch (err) {
      showNotification('error', 'Error al eliminar producto: ' + err.message)
    }
  }

  // Funciones para ajuste masivo
  const handleActivarSeleccionPrecio = () => {
    setSelectionMode(true)
    setModoSeleccion('precio')
    setSelectedIds([])
  }

  const handleActivarSeleccionStock = () => {
    setSelectionMode(true)
    setModoSeleccion('stock')
    setSelectedIds([])
  }

  const handleCancelarSeleccion = () => {
    setSelectionMode(false)
    setModoSeleccion(null)
    setSelectedIds([])
  }

  const handleToggleSelect = (id) => {
    setSelectedIds(prev =>
      prev.includes(id)
        ? prev.filter(selectedId => selectedId !== id)
        : [...prev, id]
    )
  }

  const handleToggleSelectAll = (ids) => {
    if (ids.every(id => selectedIds.includes(id))) {
      // Si todos están seleccionados, deseleccionar todos
      setSelectedIds(prev => prev.filter(id => !ids.includes(id)))
    } else {
      // Seleccionar todos los de la página actual
      setSelectedIds(prev => [...new Set([...prev, ...ids])])
    }
  }

  const handleAbrirAjusteModal = () => {
    if (selectedIds.length === 0) {
      showNotification('error', 'Debe seleccionar al menos un producto')
      return
    }

    if (modoSeleccion === 'precio') {
      setShowAjusteModal(true)
    } else if (modoSeleccion === 'stock') {
      setShowAjusteStockModal(true)
    }
  }

  const handleConfirmarAjustePrecio = async (porcentaje, redondeo) => {
    setSubmittingAjuste(true)

    try {
      const result = await ajusteMasivo(selectedIds, porcentaje, redondeo)

      setShowAjusteModal(false)
      setSelectionMode(false)
      setModoSeleccion(null)
      setSelectedIds([])

      fetchProductos(currentPage)

      showNotification('success', `¡${result.productosActualizados} producto(s) actualizado(s) exitosamente!`)
    } catch (err) {
      console.error('Error en ajuste masivo:', err)
      showNotification('error', err.message)
    } finally {
      setSubmittingAjuste(false)
    }
  }

  const handleConfirmarAjusteStock = async (ajustes) => {
    setSubmittingAjuste(true)

    try {
      const result = await ajusteStock(ajustes)

      setShowAjusteStockModal(false)
      setSelectionMode(false)
      setModoSeleccion(null)
      setSelectedIds([])

      fetchProductos(currentPage)

      showNotification('success', `¡${result.productosActualizados} producto(s) actualizado(s) exitosamente!`)
    } catch (err) {
      console.error('Error en ajuste de stock:', err)
      showNotification('error', err.message)
    } finally {
      setSubmittingAjuste(false)
    }
  }

  const productosSeleccionados = productos.filter(p => selectedIds.includes(p.id))

  return (
    <div className="space-y-6">
      <Notification
        notification={notification}
        onClose={hideNotification}
      />

      <div className="bg-white rounded-2xl shadow-sm border border-slate-200 overflow-hidden">
        <div className="px-6 py-4 border-b border-slate-200 flex justify-between items-center">
          <h2 className="text-xl font-semibold text-slate-800">Listado de Productos</h2>
          <div className="flex gap-2">
            {!selectionMode ? (
              <>
                <button
                  onClick={handleActivarSeleccionStock}
                  className="inline-flex items-center px-4 py-2 border border-blue-600 text-sm font-medium rounded-xl text-blue-600 bg-white hover:bg-blue-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
                >
                  <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" />
                  </svg>
                  Ajustar Stock
                </button>
                <button
                  onClick={handleActivarSeleccionPrecio}
                  className="inline-flex items-center px-4 py-2 border border-sky-600 text-sm font-medium rounded-xl text-sky-600 bg-white hover:bg-sky-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-sky-500 transition-colors"
                >
                  <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  Ajustar Precios
                </button>
                <button
                  onClick={() => setShowImportarModal(true)}
                  className="inline-flex items-center px-4 py-2 border border-slate-300 text-sm font-medium rounded-xl text-slate-700 bg-white hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
                >
                  <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12" />
                  </svg>
                  Importar Excel/CSV
                </button>
                <button
                  onClick={() => setShowModal(true)}
                  className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-xl shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
                >
                  <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                  </svg>
                  Nuevo Producto
                </button>
              </>
            ) : (
              <div className={`${modoSeleccion === 'stock' ? 'bg-sky-50 border-sky-200' : 'bg-blue-50 border-blue-200'} border rounded-xl px-4 py-2`}>
                <p className={`text-sm ${modoSeleccion === 'stock' ? 'text-sky-800' : 'text-blue-800'} font-medium`}>
                  {modoSeleccion === 'stock'
                    ? 'Modo inventario - Seleccione productos para ajustar stock'
                    : 'Modo selección - Seleccione productos para ajustar precios'}
                </p>
              </div>
            )}
          </div>
        </div>

        <ProductosFilters
          filters={filters}
          onApplyFilters={applyFilters}
          onClearFilters={clearFilters}
        />

        <ProductosTable
          productos={productos}
          loading={loading}
          error={error}
          onEdit={handleEditProduct}
          onDelete={handleDeleteClick}
          canDelete={canDelete}
          isAdmin={isAdmin}
          selectionMode={selectionMode}
          selectedIds={selectedIds}
          onToggleSelect={handleToggleSelect}
          onToggleSelectAll={handleToggleSelectAll}
        />

        <Pagination
          pagination={pagination}
          currentPage={currentPage}
          onPageChange={handlePageChange}
          itemName="productos"
        />
      </div>

      <ProductoModal
        show={showModal}
        editingProduct={editingProduct}
        onClose={handleCloseModal}
        onSubmit={handleModalSubmit}
        submitting={submitting}
        isAdmin={isAdmin}
      />

      <DeleteConfirmModal
        show={showDeleteConfirm}
        item={productToDelete}
        itemType="Producto"
        itemName={productToDelete?.nombre}
        onConfirm={handleDeleteConfirm}
        onCancel={() => {
          setShowDeleteConfirm(false)
          setProductToDelete(null)
        }}
      />

      <AjusteMasivoModal
        show={showAjusteModal}
        productosSeleccionados={productosSeleccionados}
        onClose={() => setShowAjusteModal(false)}
        onConfirm={handleConfirmarAjustePrecio}
        submitting={submittingAjuste}
      />

      <AjusteStockModal
        show={showAjusteStockModal}
        productosSeleccionados={productosSeleccionados}
        onClose={() => setShowAjusteStockModal(false)}
        onConfirm={handleConfirmarAjusteStock}
        submitting={submittingAjuste}
        isAdmin={isAdmin}
      />

      <ImportarProductosModal
        show={showImportarModal}
        onHide={() => setShowImportarModal(false)}
        importarProductos={importarProductos}
        onImportSuccess={(res) => {
          setShowImportarModal(false)
          fetchProductos(currentPage)
          if (res.errores && res.errores.length > 0 && res.totalProcesados > 0) {
            showNotification('warning', `Importación finalizada con advertencias. Creados: ${res.creados}, Actualizados: ${res.actualizados}, Ignorados: ${res.ignorados}. Errores: ${res.errores.length}`)
          } else {
            showNotification('success', `Importación exitosa. Creados: ${res.creados}, Actualizados: ${res.actualizados}, Ignorados: ${res.ignorados}`)
          }
        }}
      />

      <SelectionBar
        selectedCount={selectedIds.length}
        onAplicarAumento={handleAbrirAjusteModal}
        onCancelar={handleCancelarSeleccion}
        modoSeleccion={modoSeleccion}
      />
    </div>
  )
}

export default Productos
