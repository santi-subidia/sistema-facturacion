import { useMemo } from 'react'
import { useAuth } from './useAuth'

export function usePermissions() {
  const { user } = useAuth()

  const permissions = useMemo(() => {
    if (!user || !user.rol) {
      return {
        canManageUsers: false,
        canDelete: false,
        isAdmin: false,
        isVendedor: false
      }
    }

    const isAdmin = user.rol.nombre === 'Administrador'
    const isVendedor = user.rol.nombre === 'Vendedor'

    return {
      canManageUsers: isAdmin,
      canDelete: isAdmin,
      isAdmin,
      isVendedor
    }
  }, [user])

  return permissions
}
