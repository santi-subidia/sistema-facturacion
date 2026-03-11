import React from 'react'
import { usePerfil } from './hooks/usePerfil'
import Notification from './components/shared/Notification'
import ProfileInfo from './components/perfil/ProfileInfo'
import PasswordChange from './components/perfil/PasswordChange'
import ProfileSidebar from './components/perfil/ProfileSidebar'

function Perfil() {
  const {
    user,
    isEditing,
    setIsEditing,
    isChangingPassword,
    setIsChangingPassword,
    loading,
    notification,
    hideNotification,
    showCurrentPassword,
    toggleShowCurrentPassword,
    showNewPassword,
    toggleShowNewPassword,
    formData,
    passwordData,
    handleChange,
    handlePasswordChange,
    handleUpdateProfile,
    handleChangePassword,
    handleCancelEdit,
    handleCancelPasswordChange,
    handleUpdateProfilePicture
  } = usePerfil()

  return (
    <div className="space-y-6">
      <Notification notification={notification} onClose={hideNotification} />

      {/* Header */}
      <div>
        <h2 className="text-2xl font-bold text-slate-900">Mi Perfil</h2>
        <p className="mt-1 text-sm text-slate-500">Gestiona tu información personal y configuración de cuenta</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Información del perfil */}
        <div className="lg:col-span-2 space-y-6">
          <ProfileInfo
            user={user}
            isEditing={isEditing}
            setIsEditing={setIsEditing}
            formData={formData}
            handleChange={handleChange}
            handleUpdateProfile={handleUpdateProfile}
            handleCancelEdit={handleCancelEdit}
            loading={loading}
          />

          <PasswordChange
            passwordData={passwordData}
            handlePasswordChange={handlePasswordChange}
            handleChangePassword={handleChangePassword}
            handleCancelPasswordChange={handleCancelPasswordChange}
            isChangingPassword={isChangingPassword}
            setIsChangingPassword={setIsChangingPassword}
            showCurrentPassword={showCurrentPassword}
            toggleShowCurrentPassword={toggleShowCurrentPassword}
            showNewPassword={showNewPassword}
            toggleShowNewPassword={toggleShowNewPassword}
            loading={loading}
          />
        </div>

        {/* Sidebar */}
        <ProfileSidebar
          user={user}
          onUpdateProfilePicture={handleUpdateProfilePicture}
          loading={loading}
        />
      </div>
    </div>
  )
}

export default Perfil
