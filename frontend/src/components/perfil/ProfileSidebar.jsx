import React, { useRef } from 'react'
import { API_BASE_URL } from '../../config'

function ProfileSidebar({ user, onUpdateProfilePicture, loading }) {
  const fileInputRef = useRef(null)

  const handleImageClick = () => {
    if (!loading) {
      fileInputRef.current.click()
    }
  }

  const handleFileChange = (e) => {
    const file = e.target.files[0]
    if (file) {
      onUpdateProfilePicture(file)
    }
  }

  const getImageUrl = (url) => {
    if (!url) return null
    if (url.startsWith('http')) return url
    const baseUrl = API_BASE_URL.replace('/api', '')
    return `${baseUrl}${url}`
  }

  return (
    <div className="lg:col-span-1">
      <div className="bg-white rounded-2xl shadow-sm border border-slate-200 p-6">
        <div className="flex flex-col items-center">
          <div className="relative group cursor-pointer" onClick={handleImageClick}>
            <div className="h-24 w-24 rounded-full bg-slate-600 flex items-center justify-center overflow-hidden border-2 border-slate-200">
              {user?.urlImagen ? (
                <img
                  src={getImageUrl(user.urlImagen)}
                  alt={user.nombre}
                  className="h-full w-full object-cover"
                />
              ) : (
                <span className="text-white font-bold text-3xl">
                  {user?.nombre?.charAt(0).toUpperCase()}
                </span>
              )}
            </div>

            {/* Overlay for upload */}
            <div className={`absolute inset-0 flex items-center justify-center bg-black bg-opacity-0 group-hover:bg-opacity-50 rounded-full transition-all duration-200 ${loading ? 'opacity-0' : ''}`}>
              <svg className="w-8 h-8 text-white opacity-0 group-hover:opacity-100" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 9a2 2 0 012-2h.93a2 2 0 001.664-.89l.812-1.22A2 2 0 0110.07 4h3.86a2 2 0 011.664.89l.812 1.22A2 2 0 0018.07 7H19a2 2 0 012 2v9a2 2 0 01-2 2H5a2 2 0 01-2-2V9z" />
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 13a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
            </div>

            {loading && (
              <div className="absolute inset-0 flex items-center justify-center bg-white bg-opacity-70 rounded-full">
                <svg className="animate-spin h-6 w-6 text-slate-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
              </div>
            )}
          </div>

          <input
            type="file"
            ref={fileInputRef}
            onChange={handleFileChange}
            accept="image/png, image/jpeg, image/jpg"
            className="hidden"
          />

          <h3 className="mt-4 text-lg font-semibold text-slate-900">{user?.nombre}</h3>
          <p className="text-sm text-slate-500">@{user?.nombreUsuario}</p>
          <span className="mt-2 inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-blue-50 text-blue-700">
            {user?.rol?.nombre}
          </span>
        </div>

        <div className="mt-6 space-y-3">
          <div className="flex items-center text-sm text-slate-600">
            <svg className="w-5 h-5 mr-2 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
            Usuario del sistema
          </div>
          <div className="flex items-center text-sm text-slate-600">
            <svg className="w-5 h-5 mr-2 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
            </svg>
            Cuenta verificada
          </div>
        </div>
      </div>
    </div>
  )
}

export default ProfileSidebar
