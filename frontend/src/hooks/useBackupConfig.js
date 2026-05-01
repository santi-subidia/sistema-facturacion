import { useState, useCallback } from 'react'
import { API_BASE_URL } from '../config'
import { fetchWithAuth } from '../utils/authHeaders'

export function useBackupConfig() {
    const [backups, setBackups] = useState([])
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState(null)
    const [notification, setNotification] = useState(null)

    const showNotification = (type, message) => {
        setNotification({ type, message })
        setTimeout(() => setNotification(null), 5000)
    }

    const hideNotification = () => setNotification(null)

    const fetchBackups = useCallback(async () => {
        try {
            setLoading(true)
            const response = await fetchWithAuth(`${API_BASE_URL}/backup`)
            if (!response.ok) {
                throw new Error('Error al obtener los backups')
            }
            const data = await response.json()
            setBackups(data)
        } catch (err) {
            setError(err.message)
            showNotification('error', err.message)
        } finally {
            setLoading(false)
        }
    }, [])

    const triggerManualBackup = async () => {
        try {
            setLoading(true)
            const response = await fetchWithAuth(`${API_BASE_URL}/backup/manual`, {
                method: 'POST'
            })

            const data = await response.json()

            if (!response.ok) {
                throw new Error(data.message || 'Error al forzar el backup')
            }

            showNotification('success', data.message || 'Backup creado exitosamente')
            await fetchBackups() // Refresh list
            return data.backup
        } catch (err) {
            setError(err.message)
            showNotification('error', err.message)
            throw err
        } finally {
            setLoading(false)
        }
    }

    const downloadBackup = async (fileName) => {
        try {
            const response = await fetchWithAuth(`${API_BASE_URL}/backup/download/${fileName}`)
            if (!response.ok) {
                const errorData = await response.json()
                throw new Error(errorData.message || 'Error al descargar el backup')
            }

            const blob = await response.blob()
            const url = window.URL.createObjectURL(blob)
            const a = document.createElement('a')
            a.href = url
            a.download = fileName
            document.body.appendChild(a)
            a.click()
            window.URL.revokeObjectURL(url)
            document.body.removeChild(a)

        } catch (err) {
            showNotification('error', err.message)
        }
    }

    return {
        backups,
        loading,
        error,
        notification,
        showNotification,
        hideNotification,
        fetchBackups,
        triggerManualBackup,
        downloadBackup
    }
}
