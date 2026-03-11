import { useState, useEffect } from "react";
import { useAuth } from "./useAuth";
import { API_BASE_URL } from "../config";

export function usePerfil() {
  const { user, login } = useAuth();
  const [isEditing, setIsEditing] = useState(false);
  const [isChangingPassword, setIsChangingPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [notification, setNotification] = useState({
    show: false,
    type: "",
    message: "",
  });
  const [showCurrentPassword, setShowCurrentPassword] = useState(false);
  const [showNewPassword, setShowNewPassword] = useState(false);

  const [formData, setFormData] = useState({
    nombre: user?.nombre || "",
    username: user?.nombreUsuario || "",
    urlImagen: user?.urlImagen || "",
  });

  const [passwordData, setPasswordData] = useState({
    currentPassword: "",
    newPassword: "",
    confirmPassword: "",
  });

  // Actualizar formData cuando el usuario cambie
  useEffect(() => {
    if (user) {
      setFormData({
        nombre: user.nombre || "",
        username: user.nombreUsuario || "",
        urlImagen: user.urlImagen || "",
      });
    }
  }, [user]);

  const hideNotification = () => {
    setNotification({ show: false, type: "", message: "" });
  };

  const showNotification = (type, message) => {
    setNotification({ show: true, type, message });
    setTimeout(() => {
      hideNotification();
    }, 5000);
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handlePasswordChange = (e) => {
    const { name, value } = e.target;
    setPasswordData((prev) => ({ ...prev, [name]: value }));
  };

  const handleUpdateProfile = async (e) => {
    e.preventDefault();
    setLoading(true);

    try {
      const token = localStorage.getItem("token");
      const response = await fetch(`${API_BASE_URL}/usuario/perfil`, {
        method: "PUT",
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          nombre: formData.nombre,
          username: formData.username,
          urlImagen: formData.urlImagen,
        }),
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.message || "Error al actualizar el perfil");
      }

      // Actualizar el usuario en localStorage y en el estado
      const updatedUser = {
        ...user,
        nombre: data.nombre,
        nombreUsuario: data.username,
        urlImagen: data.urlImagen,
      };
      localStorage.setItem("user", JSON.stringify(updatedUser));
      login(updatedUser);

      showNotification("success", "Perfil actualizado correctamente");
      setIsEditing(false);
    } catch (error) {
      showNotification("error", error.message);
    } finally {
      setLoading(false);
    }
  };

  const handleChangePassword = async (e) => {
    e.preventDefault();

    if (passwordData.newPassword !== passwordData.confirmPassword) {
      showNotification("error", "Las contraseñas no coinciden");
      return;
    }

    if (passwordData.newPassword.length < 6) {
      showNotification(
        "error",
        "La contraseña debe tener al menos 6 caracteres",
      );
      return;
    }

    setLoading(true);

    try {
      const token = localStorage.getItem("token");
      const response = await fetch(`${API_BASE_URL}/usuario/cambiar-password`, {
        method: "PUT",
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          currentPassword: passwordData.currentPassword,
          newPassword: passwordData.newPassword,
        }),
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.message || "Error al cambiar la contraseña");
      }

      showNotification("success", "Contraseña actualizada correctamente");
      setIsChangingPassword(false);
      setPasswordData({
        currentPassword: "",
        newPassword: "",
        confirmPassword: "",
      });
    } catch (error) {
      showNotification("error", error.message);
    } finally {
      setLoading(false);
    }
  };

  const handleCancelEdit = () => {
    setFormData({
      nombre: user?.nombre || "",
      username: user?.nombreUsuario || "",
      urlImagen: user?.urlImagen || "",
    });
    setIsEditing(false);
  };

  const handleCancelPasswordChange = () => {
    setPasswordData({
      currentPassword: "",
      newPassword: "",
      confirmPassword: "",
    });
    setIsChangingPassword(false);
  };

  const handleUpdateProfilePicture = async (file) => {
    if (!file) return;

    setLoading(true);
    const formData = new FormData();
    formData.append("image", file);

    try {
      const token = localStorage.getItem("token");
      const response = await fetch(`${API_BASE_URL}/usuario/perfil/imagen`, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
        },
        body: formData,
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(
          data.error || data.message || "Error al actualizar la foto de perfil",
        );
      }

      const baseUrl = API_BASE_URL.replace("/api", "");
      const updatedUser = {
        ...user,
        urlImagen: `${baseUrl}${data.url}?t=${Date.now()}`,
      };

      localStorage.setItem("user", JSON.stringify(updatedUser));
      login(updatedUser);

      showNotification("success", "Foto de perfil actualizada correctamente");
    } catch (error) {
      showNotification("error", error.message);
    } finally {
      setLoading(false);
    }
  };

  const toggleShowCurrentPassword = () =>
    setShowCurrentPassword(!showCurrentPassword);
  const toggleShowNewPassword = () => setShowNewPassword(!showNewPassword);

  return {
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
    handleUpdateProfilePicture,
  };
}
