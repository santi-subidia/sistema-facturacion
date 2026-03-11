import { useState, useEffect } from 'react'
import { API_BASE_URL } from '../config'
import { fetchWithAuth } from '../utils/authHeaders'

export function useClientes() {
  const [clientes, setClientes] = useState([])
  const [pagination, setPagination] = useState(null)
  const [currentPage, setCurrentPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [pageSize, setPageSize] = useState(10)

  const fetchClientes = async (page = 1, currentSize = pageSize) => {
    try {
      setLoading(true)
      setError(null)
      const response = await fetchWithAuth(`${API_BASE_URL}/cliente?page=${page}&pageSize=${currentSize}`)

      if (!response.ok) {
        throw new Error('Error al cargar clientes')
      }

      const data = await response.json()
      setClientes(data.data || [])
      setPagination(data.pagination)
      setCurrentPage(page)
    } catch (err) {
      setError(err.message)
      console.error('Error:', err)
    } finally {
      setLoading(false)
    }
  }

  const createCliente = async (formData) => {
    const user = JSON.parse(localStorage.getItem('user') || '{}')
    const dataToSend = {
      documento: formData.documento,
      nombre: formData.nombre,
      apellido: formData.apellido,
      telefono: formData.telefono,
      correo: formData.correo,
      direccion: formData.direccion,
      idCreado_por: user.id || 1
    }

    const response = await fetchWithAuth(`${API_BASE_URL}/cliente`, {
      method: 'POST',
      body: JSON.stringify(dataToSend)
    })

    if (!response.ok) {
      const errorData = await response.json()
      let errorMessage = 'Error de validación: '
      if (errorData.errors) {
        const errorMessages = Object.entries(errorData.errors).map(([field, messages]) => {
          return `${field}: ${Array.isArray(messages) ? messages.join(', ') : messages}`
        })
        errorMessage += errorMessages.join('; ')
      } else {
        errorMessage = errorData.title || errorData.message || 'Error al crear cliente'
      }
      throw new Error(errorMessage)
    }

    return response.json()
  }

  const updateCliente = async (cliente, formData) => {
    const dataToSend = {
      id: cliente.id,
      documento: formData.documento,
      nombre: formData.nombre,
      apellido: formData.apellido,
      telefono: formData.telefono,
      correo: formData.correo,
      direccion: formData.direccion,
      creado_at: cliente.creado_at,
      idCreado_por: cliente.idCreado_por,
      eliminado_at: cliente.eliminado_at || null,
      idEliminado_por: cliente.idEliminado_por || null
    }

    const response = await fetchWithAuth(`${API_BASE_URL}/cliente/${cliente.id}`, {
      method: 'PUT',
      body: JSON.stringify(dataToSend)
    })

    if (!response.ok) {
      const errorData = await response.json()
      let errorMessage = 'Error de validación: '
      if (errorData.errors) {
        const errorMessages = Object.entries(errorData.errors).map(([field, messages]) => {
          return `${field}: ${Array.isArray(messages) ? messages.join(', ') : messages}`
        })
        errorMessage += errorMessages.join('; ')
      } else {
        errorMessage = errorData.title || errorData.message || 'Error al actualizar cliente'
      }
      throw new Error(errorMessage)
    }

    if (response.status === 204) {
      return null
    }

    return response.json()
  }

  const deleteCliente = async (id) => {
    const user = JSON.parse(localStorage.getItem('user') || '{}')
    const response = await fetchWithAuth(`${API_BASE_URL}/cliente/${id}`, {
      method: 'DELETE',
      body: JSON.stringify(user.id || 1)
    })

    if (!response.ok) {
      const errorData = await response.json()
      throw new Error(errorData.message || 'Error al eliminar cliente')
    }

    if (response.status === 204) {
      return null
    }

    return response.json()
  }

  const handlePageChange = (page) => {
    fetchClientes(page, pageSize)
  }

  const handlePageSizeChange = (newSize) => {
    setPageSize(newSize)
    fetchClientes(1, newSize)
  }

  useEffect(() => {
    fetchClientes()
  }, [])

  return {
    clientes,
    pagination,
    currentPage,
    loading,
    error,
    fetchClientes,
    createCliente,
    updateCliente,
    deleteCliente,
    handlePageChange,
    pageSize,
    handlePageSizeChange
  }
}

export function useNotification() {
  const [notification, setNotification] = useState(null)

  const showNotification = (type, message) => {
    setNotification({ show: true, type, message })
    setTimeout(() => setNotification(null), 5000)
  }

  const hideNotification = () => {
    setNotification(null)
  }

  return {
    notification,
    showNotification,
    hideNotification
  }
}
