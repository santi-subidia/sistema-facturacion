import React, { useState } from 'react'

function UsuarioModal({
  show,
  editingUser,
  roles,
  onClose,
  onSubmit,
  submitting
}) {
  const [formData, setFormData] = useState({
    username: editingUser?.username || '',
    passwordHash: '',
    nombre: editingUser?.nombre || '',
    url_imagen: editingUser?.url_imagen || '',
    idRol: editingUser?.idRol?.toString() || ''
  })
  const [formErrors, setFormErrors] = useState({})

  React.useEffect(() => {
    if (editingUser) {
      setFormData({
        username: editingUser.username,
        passwordHash: '',
        nombre: editingUser.nombre,
        url_imagen: editingUser.url_imagen || '',
        idRol: editingUser.rol?.id?.toString() || editingUser.idRol?.toString() || ''
      })
    } else {
      setFormData({
        username: '',
        passwordHash: '',
        nombre: '',
        url_imagen: '',
        idRol: ''
      })
    }
    setFormErrors({})
  }, [editingUser, show])

  const handleInputChange = (e) => {
    const { name, value } = e.target
    setFormData(prev => ({
      ...prev,
      [name]: value
    }))
    if (formErrors[name]) {
      setFormErrors(prev => ({
        ...prev,
        [name]: ''
      }))
    }
  }

  const validateForm = () => {
    const errors = {}

    if (!formData.username.trim()) {
      errors.username = 'El nombre de usuario es obligatorio.'
    }

    if (!editingUser) {
      if (!formData.passwordHash.trim()) {
        errors.passwordHash = 'La contraseña es obligatoria.'
      } else if (formData.passwordHash.length < 6) {
        errors.passwordHash = 'La contraseña debe tener al menos 6 caracteres.'
      }
    } else if (formData.passwordHash && formData.passwordHash.length < 6) {
      errors.passwordHash = 'La contraseña debe tener al menos 6 caracteres.'
    }

    if (!formData.nombre.trim()) {
      errors.nombre = 'El nombre es obligatorio.'
    } else if (formData.nombre.trim().length < 3) {
      errors.nombre = 'El nombre debe tener al menos 3 caracteres.'
    } else if (!/^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$/.test(formData.nombre)) {
      errors.nombre = 'El nombre solo puede contener letras y espacios.'
    }

    if (!formData.idRol) {
      errors.idRol = 'El rol es obligatorio.'
    }

    setFormErrors(errors)
    return Object.keys(errors).length === 0
  }

  const handleSubmit = (e) => {
    e.preventDefault()

    if (!validateForm()) {
      return
    }

    onSubmit(formData)
  }

  if (!show) return null

  return (
    <div className="fixed z-[60] inset-0 overflow-y-auto">
      <div className="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
        <div
          className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity"
          onClick={onClose}
        ></div>

        <span className="hidden sm:inline-block sm:align-middle sm:h-screen" aria-hidden="true">&#8203;</span>

        <div className="relative z-10 inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
          <form onSubmit={handleSubmit}>
            <div className="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
              <div className="sm:flex sm:items-start">
                <div className="mx-auto flex-shrink-0 flex items-center justify-center h-12 w-12 rounded-full bg-indigo-100 sm:mx-0 sm:h-10 sm:w-10">
                  <svg className="h-6 w-6 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                  </svg>
                </div>
                <div className="mt-3 text-center sm:mt-0 sm:ml-4 sm:text-left w-full">
                  <h3 className="text-lg leading-6 font-medium text-gray-900 mb-4">
                    {editingUser ? 'Editar Usuario' : 'Crear Nuevo Usuario'}
                  </h3>

                  <div className="space-y-4">
                    {/* Username */}
                    <div>
                      <label htmlFor="username" className="block text-sm font-medium text-gray-700">
                        Nombre de Usuario *
                      </label>
                      <input
                        type="text"
                        name="username"
                        id="username"
                        value={formData.username}
                        onChange={handleInputChange}
                        className={`mt-1 block w-full border ${formErrors.username ? 'border-red-300' : 'border-gray-300'} rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm`}
                        placeholder="usuario123"
                      />
                      {formErrors.username && (
                        <p className="mt-1 text-sm text-red-600">{formErrors.username}</p>
                      )}
                    </div>

                    {/* Password */}
                    <div>
                      <label htmlFor="passwordHash" className="block text-sm font-medium text-gray-700">
                        Contraseña {editingUser ? '(dejar en blanco para no cambiar)' : '*'}
                      </label>
                      <input
                        type="password"
                        name="passwordHash"
                        id="passwordHash"
                        value={formData.passwordHash}
                        onChange={handleInputChange}
                        className={`mt-1 block w-full border ${formErrors.passwordHash ? 'border-red-300' : 'border-gray-300'} rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm`}
                        placeholder="••••••••"
                      />
                      {formErrors.passwordHash && (
                        <p className="mt-1 text-sm text-red-600">{formErrors.passwordHash}</p>
                      )}
                    </div>

                    {/* Nombre completo */}
                    <div>
                      <label htmlFor="nombre" className="block text-sm font-medium text-gray-700">
                        Nombre Completo *
                      </label>
                      <input
                        type="text"
                        name="nombre"
                        id="nombre"
                        value={formData.nombre}
                        onChange={handleInputChange}
                        className={`mt-1 block w-full border ${formErrors.nombre ? 'border-red-300' : 'border-gray-300'} rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm`}
                        placeholder="Juan Pérez"
                      />
                      {formErrors.nombre && (
                        <p className="mt-1 text-sm text-red-600">{formErrors.nombre}</p>
                      )}
                    </div>

                    {/* URL Imagen */}
                    <div>
                      <label htmlFor="url_imagen" className="block text-sm font-medium text-gray-700">
                        URL de Imagen (opcional)
                      </label>
                      <input
                        type="text"
                        name="url_imagen"
                        id="url_imagen"
                        value={formData.url_imagen}
                        onChange={handleInputChange}
                        className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                        placeholder="https://..."
                      />
                    </div>

                    {/* Rol */}
                    <div>
                      <label htmlFor="idRol" className="block text-sm font-medium text-gray-700">
                        Rol *
                      </label>
                      <select
                        name="idRol"
                        id="idRol"
                        value={formData.idRol}
                        onChange={handleInputChange}
                        className={`mt-1 block w-full border ${formErrors.idRol ? 'border-red-300' : 'border-gray-300'} rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm`}
                      >
                        <option value="">Seleccione un rol</option>
                        {roles.map(rol => (
                          <option key={rol.id} value={rol.id}>
                            {rol.nombre}
                          </option>
                        ))}
                      </select>
                      {formErrors.idRol && (
                        <p className="mt-1 text-sm text-red-600">{formErrors.idRol}</p>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div className="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
              <button
                type="submit"
                disabled={submitting}
                className={`w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 text-base font-medium text-white sm:ml-3 sm:w-auto sm:text-sm ${submitting
                  ? 'bg-indigo-400 cursor-not-allowed'
                  : 'bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500'
                  }`}
              >
                {submitting ? (
                  <>
                    <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
                      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                    </svg>
                    {editingUser ? 'Actualizando...' : 'Creando...'}
                  </>
                ) : (
                  editingUser ? 'Actualizar Usuario' : 'Crear Usuario'
                )}
              </button>
              <button
                type="button"
                onClick={onClose}
                disabled={submitting}
                className="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm"
              >
                Cancelar
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  )
}

export default UsuarioModal
