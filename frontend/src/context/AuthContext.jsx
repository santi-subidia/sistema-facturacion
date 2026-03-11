import React, { createContext, useState, useEffect } from "react";
import { API_BASE_URL } from "../config";

export const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [hasAfipConfig, setHasAfipConfig] = useState(null);

    useEffect(() => {
        // Verificar si hay un usuario guardado en localStorage
        const storedUser = localStorage.getItem("user");
        const storedToken = localStorage.getItem("token");

        if (storedUser && storedToken) {
            try {
                setUser(JSON.parse(storedUser));
            } catch (error) {
                console.error("Error al parsear usuario:", error);
                localStorage.removeItem("user");
                localStorage.removeItem("token");
                localStorage.removeItem("refreshToken");
            }
        }
        setIsLoading(false);

        // Cargar estado de AFIP si está autenticado
        if (storedUser && storedToken) {
            checkAfipConfig();
        }
    }, []);

    const checkAfipConfig = async () => {
        try {
            const { fetchWithAuth } = await import("../utils/authHeaders");
            const response = await fetchWithAuth(`${API_BASE_URL}/afipConfiguracion/activa`);
            setHasAfipConfig(response.ok);
        } catch (error) {
            console.error("Error verificando config AFIP:", error);
            setHasAfipConfig(false);
        }
    };

    const login = (userData, token, refreshToken) => {
        if (token) localStorage.setItem("token", token);
        if (refreshToken) localStorage.setItem("refreshToken", refreshToken);
        if (userData) localStorage.setItem("user", JSON.stringify(userData));
        setUser(userData);
    };

    const logout = async () => {
        // Revocar el refresh token en el backend
        const refreshToken = localStorage.getItem("refreshToken");
        if (refreshToken) {
            try {
                await fetch(`${API_BASE_URL}/auth/logout`, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ refreshToken }),
                });
            } catch (error) {
                console.error("Error al revocar token:", error);
            }
        }

        localStorage.removeItem("user");
        localStorage.removeItem("token");
        localStorage.removeItem("refreshToken");
        setUser(null);
    };

    const getToken = () => {
        return localStorage.getItem("token");
    };

    return (
        <AuthContext.Provider
            value={{
                user,
                isLoading,
                isAuthenticated: !!user,
                login,
                logout,
                getToken,
                hasAfipConfig,
                checkAfipConfig
            }}
        >
            {children}
        </AuthContext.Provider>
    );
};
