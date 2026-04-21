import { useState, useEffect, useRef } from 'react'
import { API_BASE_URL } from '../config'
import { fetchWithAuth, getAuthHeaders } from '../utils/authHeaders'

export function useComprobantes() {
  const [clientes, setClientes] = useState([])
  const [productos, setProductos] = useState([])
  const [tiposComprobante, setTiposComprobante] = useState([])
  const [formasPago, setFormasPago] = useState([])
  const [condicionesVenta, setCondicionesVenta] = useState([])
  const [afipConfig, setAfipConfig] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  // Clave de idempotencia: se genera una por "sesión de formulario".
  // Se regenera automáticamente tras cada creación exitosa.
  const idempotencyKeyRef = useRef(crypto.randomUUID())

  // Cargar datos iniciales
  useEffect(() => {
    fetchInitialData()
  }, [])

  const fetchInitialData = async () => {
    try {
      setLoading(true)
      setError(null)

      // Cargar clientes activos
      const clientesResponse = await fetchWithAuth(`${API_BASE_URL}/cliente?page=1&pageSize=1000`)
      if (!clientesResponse.ok) throw new Error('Error al cargar clientes')
      const clientesData = await clientesResponse.json()
      setClientes(clientesData.data || [])

      // Cargar productos activos con stock
      const productosResponse = await fetchWithAuth(`${API_BASE_URL}/productos?page=1&pageSize=1000`)
      if (!productosResponse.ok) throw new Error('Error al cargar productos')
      const productosData = await productosResponse.json()
      setProductos(productosData.data || [])

      // Cargar tipos de comprobante (habilitados por configuración)
      const tiposResponse = await fetchWithAuth(`${API_BASE_URL}/tipocomprobante/habilitados`)
      if (!tiposResponse.ok) throw new Error('Error al cargar tipos de comprobante')
      const tiposData = await tiposResponse.json()
      setTiposComprobante(tiposData.data || tiposData || [])

      // Cargar formas de pago (activas)
      const formasPagoResponse = await fetchWithAuth(`${API_BASE_URL}/formapago/activas`)
      if (!formasPagoResponse.ok) throw new Error('Error al cargar formas de pago')
      const formasPagoData = await formasPagoResponse.json()
      setFormasPago(formasPagoData.data || formasPagoData || [])

      // Cargar condiciones de venta (activas)
      const condicionesVentaResponse = await fetchWithAuth(`${API_BASE_URL}/condicionventa`)
      if (!condicionesVentaResponse.ok) throw new Error('Error al cargar condiciones de venta')
      const condicionesVentaData = await condicionesVentaResponse.json()
      setCondicionesVenta(condicionesVentaData.data || condicionesVentaData || [])

      // Cargar configuración AFIP activa
      const afipConfigResponse = await fetchWithAuth(`${API_BASE_URL}/afipconfiguracion/activa`)
      if (afipConfigResponse.ok) {
        const afipConfigData = await afipConfigResponse.json()
        setAfipConfig(afipConfigData)
      }

    } catch (err) {
      setError(err.message)
      console.error('Error:', err)
    } finally {
      setLoading(false)
    }
  }

  // Regenerar manualmente la clave (útil al resetear el formulario sin desmontar)
  const resetIdempotencyKey = () => {
    idempotencyKeyRef.current = crypto.randomUUID()
  }

  const createComprobante = async (comprobanteData) => {
    try {
      const response = await fetchWithAuth(`${API_BASE_URL}/comprobantes/crear-con-detalles`, {
        method: 'POST',
        headers: {
          ...getAuthHeaders(),
          'X-Idempotency-Key': idempotencyKeyRef.current
        },
        body: JSON.stringify(comprobanteData)
      })

      if (!response.ok) {
        let errorData;
        try {
          errorData = await response.json()
        } catch {
          throw new Error('Error al conectar con el servidor')
        }

        // Si el backend devolvió 409 (conflicto de idempotencia en proceso), informar al usuario
        if (response.status === 409) {
          throw new Error(errorData.message || 'La operación ya está siendo procesada. Por favor, espere.')
        }

        let errorMessage = errorData.message || 'Error al crear el comprobante';
        if (errorData.errors) {
          if (Array.isArray(errorData.errors) && errorData.errors.length > 0) {
            errorMessage = errorData.errors[0];
          } else if (typeof errorData.errors === 'object') {
            const firstKey = Object.keys(errorData.errors)[0];
            if (firstKey && errorData.errors[firstKey].length > 0) {
              errorMessage = errorData.errors[firstKey][0];
            }
          }
        }
        throw new Error(errorMessage)
      }

      const data = await response.json()

      // Creación exitosa: regenerar la clave para la próxima operación
      resetIdempotencyKey()

      return data
    } catch (err) {
      console.error('Error al crear comprobante:', err)
      throw err
    }
  }

  return {
    clientes,
    productos,
    tiposComprobante,
    formasPago,
    condicionesVenta,
    afipConfig,
    loading,
    error,
    createComprobante,
    resetIdempotencyKey,
    refreshData: fetchInitialData
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
