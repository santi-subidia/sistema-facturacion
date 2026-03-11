import { useState, useEffect } from 'react'
import { API_BASE_URL } from '../config'
import { fetchWithAuth, fetchWithAuthMultipart } from '../utils/authHeaders'

export function useAfipConfiguracion() {
  const [configuraciones, setConfiguraciones] = useState([])
  const [condicionesIva, setCondicionesIva] = useState([])
  const [configuracionActiva, setConfiguracionActiva] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  const fetchConfiguraciones = async () => {
    try {
      setLoading(true)
      setError(null)
      const response = await fetchWithAuth(`${API_BASE_URL}/afipConfiguracion`)

      if (!response.ok) {
        throw new Error('Error al cargar configuraciones')
      }

      const data = await response.json()
      setConfiguraciones(data.data || [])
      setConfiguracionActiva(data.data?.find(c => c.activa))
    } catch (err) {
      setError(err.message)
      console.error('Error:', err)
    } finally {
      setLoading(false)
    }
  }

  const fetchCondicionesIva = async () => {
    try {
      const response = await fetchWithAuth(`${API_BASE_URL}/afipConfiguracion/condiciones-iva`)
      if (response.ok) {
        const data = await response.json()
        setCondicionesIva(data.data || [])
      }
    } catch (err) {
      console.error('Error al cargar condiciones IVA:', err)
    }
  }

  const createConfiguracion = async (data) => {
    const formData = new FormData()
    Object.keys(data).forEach(key => {
      if (data[key] === null || data[key] === undefined) return

      if (key === 'certificado' || key === 'logo') {
        if (data[key]) formData.append(key, data[key])
      } else {
        formData.append(key, data[key])
      }
    })

    const response = await fetchWithAuthMultipart(`${API_BASE_URL}/afipConfiguracion`, {
      method: 'POST',
      body: formData
    })

    if (!response.ok) {
      const errorData = await response.json()
      throw new Error(errorData.message || 'Error al crear configuración')
    }

    return response.json()
  }

  const updateConfiguracion = async (configId, data) => {
    const formData = new FormData()
    Object.keys(data).forEach(key => {
      if (data[key] === null || data[key] === undefined) return

      if (key === 'certificado' || key === 'logo') {
        if (data[key]) formData.append(key, data[key])
      } else {
        formData.append(key, data[key])
      }
    })

    const response = await fetchWithAuthMultipart(`${API_BASE_URL}/afipConfiguracion/${configId}`, {
      method: 'PUT',
      body: formData
    })

    if (!response.ok) {
      const errorData = await response.json()
      throw new Error(errorData.message || 'Error al actualizar configuración')
    }

    return response.json()
  }

  useEffect(() => {
    fetchConfiguraciones()
    fetchCondicionesIva()
  }, [])

  return {
    configuraciones,
    condicionesIva,
    configuracionActiva,
    loading,
    error,
    fetchConfiguraciones,
    createConfiguracion,
    updateConfiguracion
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

