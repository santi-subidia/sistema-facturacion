import { useState, useEffect } from 'react'
import { API_BASE_URL } from '../config'
import { fetchWithAuth } from '../utils/authHeaders'

export function useUsuarios() {
  const [usuarios, setUsuarios] = useState([])
  const [pagination, setPagination] = useState(null)
  const [currentPage, setCurrentPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [roles, setRoles] = useState([])
  const [pageSize, setPageSize] = useState(10)

  const fetchUsuarios = async (page = 1, currentSize = pageSize) => {
    try {
      setLoading(true)
      setError(null)
      const response = await fetchWithAuth(`${API_BASE_URL}/usuario?page=${page}&pageSize=${currentSize}`)

      if (!response.ok) {
        throw new Error('Error al cargar usuarios')
      }

      const data = await response.json()
      setUsuarios(data.data || [])
      setPagination(data.pagination)
      setCurrentPage(page)
    } catch (err) {
      setError(err.message)
      console.error('Error:', err)
    } finally {
      setLoading(false)
    }
  }

  const fetchRoles = async () => {
    try {
      const response = await fetchWithAuth(`${API_BASE_URL}/rol`)
      if (response.ok) {
        const data = await response.json()
        setRoles(data)
      }
    } catch (err) {
      console.error('Error al cargar roles:', err)
    }
  }

  const createUsuario = async (formData) => {
    const dataToSend = {
      username: formData.username,
      nombre: formData.nombre,
      url_imagen: formData.url_imagen || '',
      idRol: parseInt(formData.idRol),
      passwordHash: formData.passwordHash
    }

    const response = await fetchWithAuth(`${API_BASE_URL}/usuario`, {
      method: 'POST',
      body: JSON.stringify(dataToSend)
    })

    if (!response.ok) {
      const errorData = await response.json()
      let errorMessage = 'Error de validación: '
      if (errorData.errors) {
        if (Array.isArray(errorData.errors)) {
          errorMessage += errorData.errors.join('; ')
        } else {
          const errorMessages = Object.entries(errorData.errors).map(([field, messages]) => {
            if (!isNaN(field)) {
              return Array.isArray(messages) ? messages.join(', ') : messages
            }
            return `${field}: ${Array.isArray(messages) ? messages.join(', ') : messages}`
          })
          errorMessage += errorMessages.join('; ')
        }
      } else {
        errorMessage = errorData.title || errorData.message || 'Error al crear usuario'
      }
      throw new Error(errorMessage)
    }

    return response.json()
  }

  const updateUsuario = async (usuario, formData) => {
    const dataToSend = {
      id: usuario.id,
      username: formData.username,
      nombre: formData.nombre,
      url_imagen: formData.url_imagen || '',
      idRol: parseInt(formData.idRol),
      eliminado_at: usuario.eliminado_at || null
    }

    if (formData.passwordHash) {
      dataToSend.passwordHash = formData.passwordHash
    } else {
      dataToSend.passwordHash = usuario.passwordHash
    }

    const response = await fetchWithAuth(`${API_BASE_URL}/usuario/${usuario.id}`, {
      method: 'PUT',
      body: JSON.stringify(dataToSend)
    })

    if (!response.ok) {
      const errorData = await response.json()
      let errorMessage = 'Error de validación: '
      if (errorData.errors) {
        if (Array.isArray(errorData.errors)) {
          errorMessage += errorData.errors.join('; ')
        } else {
          const errorMessages = Object.entries(errorData.errors).map(([field, messages]) => {
            if (!isNaN(field)) {
              return Array.isArray(messages) ? messages.join(', ') : messages
            }
            return `${field}: ${Array.isArray(messages) ? messages.join(', ') : messages}`
          })
          errorMessage += errorMessages.join('; ')
        }
      } else {
        errorMessage = errorData.title || errorData.message || 'Error al actualizar usuario'
      }
      throw new Error(errorMessage)
    }

    if (response.status === 204) {
      return null
    }

    return response.json()
  }

  const deleteUsuario = async (usuarioId) => {
    const response = await fetchWithAuth(`${API_BASE_URL}/usuario/${usuarioId}`, {
      method: 'DELETE'
    })

    if (!response.ok) {
      throw new Error('Error al eliminar usuario')
    }

    if (response.status === 204) {
      return null
    }

    return response.json()
  }

  useEffect(() => {
    fetchUsuarios(1)
    fetchRoles()
  }, [])

  const handlePageChange = (newPage) => {
    if (newPage >= 1 && newPage <= (pagination?.totalPages || 1)) {
      fetchUsuarios(newPage, pageSize)
    }
  }

  const handlePageSizeChange = (newSize) => {
    setPageSize(newSize)
    fetchUsuarios(1, newSize)
  }

  return {
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
