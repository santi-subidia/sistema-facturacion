import { API_BASE_URL } from '../config'

// Helper para obtener los headers con el token de autenticación
export const getAuthHeaders = () => {
  const token = localStorage.getItem('token')
  return {
    'Content-Type': 'application/json',
    'Authorization': token ? `Bearer ${token}` : ''
  }
}

export const getAuthHeadersMultipart = () => {
  const token = localStorage.getItem('token')
  return {
    'Authorization': token ? `Bearer ${token}` : ''
  }
}

let refreshPromise = null;

// Intentar renovar el token usando el refresh token con un lock (para evitar llamadas concurrentes)
async function performRefresh() {
  const refreshToken = localStorage.getItem('refreshToken')
  if (!refreshToken) return false

  try {
    const response = await fetch(`${API_BASE_URL}/auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken })
    })

    if (!response.ok) {
      // Refresh token inválido o expirado — forzar login
      localStorage.removeItem('token')
      localStorage.removeItem('refreshToken')
      localStorage.removeItem('user')
      if (window.location.pathname !== '/login') {
        window.location.href = '/login'
      }
      return false
    }

    const data = await response.json()
    localStorage.setItem('token', data.token)
    localStorage.setItem('refreshToken', data.refreshToken)
    return true
  } catch (error) {
    console.error('Error renovando token:', error)
    return false
  }
}

async function refreshAccessToken() {
  if (refreshPromise) return refreshPromise;

  refreshPromise = performRefresh().finally(() => {
    refreshPromise = null;
  });

  return refreshPromise;
}

// Fetch con auto-refresh: si recibe 401, intenta renovar y reintentar
export async function fetchWithAuth(url, options = {}) {
  // Agregar headers de autenticación
  const headers = options.headers || getAuthHeaders()
  const response = await fetch(url, { ...options, headers })

  // Si no es 401, devolver la respuesta directamente
  if (response.status !== 401) {
    return response
  }

  // Intentar renovar el token
  const refreshed = await refreshAccessToken()
  if (!refreshed) {
    return response // Devolver el 401 original
  }

  // Reintentar con el nuevo token
  const newHeaders = {
    ...headers,
    'Authorization': `Bearer ${localStorage.getItem('token')}`
  }
  return fetch(url, { ...options, headers: newHeaders })
}

// Fetch con auto-refresh para multipart (file uploads)
export async function fetchWithAuthMultipart(url, options = {}) {
  const headers = options.headers || getAuthHeadersMultipart()
  const response = await fetch(url, { ...options, headers })

  if (response.status !== 401) {
    return response
  }

  const refreshed = await refreshAccessToken()
  if (!refreshed) {
    return response
  }

  const newHeaders = {
    ...headers,
    'Authorization': `Bearer ${localStorage.getItem('token')}`
  }
  return fetch(url, { ...options, headers: newHeaders })
}
