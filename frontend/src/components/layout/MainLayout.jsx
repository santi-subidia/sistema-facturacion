import React, { useState } from 'react'
import { Outlet, NavLink, useNavigate, useLocation } from 'react-router-dom'
import { useAuth } from '../../hooks/useAuth'
import { usePermissions } from '../../hooks/usePermissions'
import { useAfipConfiguracion } from '../../hooks/useAfipConfiguracion'
import { useConnectivity } from '../../hooks/useConnectivity'
import { API_BASE_URL } from '../../config'

function MainLayout() {
    const { user, logout } = useAuth()
    const { canManageUsers } = usePermissions()
    const { configuracionActiva } = useAfipConfiguracion()
    const { isOnline, isAfipOnline } = useConnectivity()
    const navigate = useNavigate()
    const location = useLocation()
    const [sidebarOpen, setSidebarOpen] = useState(true)
    const [openDropdown, setOpenDropdown] = useState(null)
    const [version, setVersion] = useState('...')

    React.useEffect(() => {
        fetch(`${API_BASE_URL}/version`)
            .then(res => res.json())
            .then(data => setVersion(data.version))
            .catch(() => setVersion('v1.0.0'))
    }, [])

    const companyName = configuracionActiva?.nombreFantasia || configuracionActiva?.razonSocial || 'Sistema de Facturación'

    const handleLogout = () => {
        logout()
        navigate('/login')
    }

    const isComprobantesActive = location.pathname.startsWith('/comprobantes')
    const isPresupuestosActive = location.pathname.startsWith('/presupuestos')
    const isCajaActive = location.pathname.startsWith('/caja')

    const dropdownParentClass = (isActive) =>
        `${isActive
            ? 'bg-blue-500/10 text-blue-400 border-l-2 border-blue-500'
            : 'text-slate-400 hover:bg-white/5 hover:text-white border-l-2 border-transparent'
        } group flex items-center justify-between px-4 py-3 text-sm font-medium transition-all duration-200 cursor-pointer`

    const navLinkClass = ({ isActive }) =>
        `${isActive
            ? 'bg-blue-500/10 text-blue-400 border-l-2 border-blue-500 shadow-[inset_1px_0_0_0_rgba(59,130,246,0.2)]'
            : 'text-slate-400 hover:bg-white/5 hover:text-white border-l-2 border-transparent'
        } group flex items-center px-4 py-2.5 my-0.5 mx-2 rounded-md text-sm font-medium transition-all duration-200`

    const subNavLinkClass = ({ isActive }) =>
        `${isActive
            ? 'text-blue-400 font-medium'
            : 'text-slate-500 hover:text-slate-300'
        } group flex items-center px-4 py-2 text-xs transition-all duration-200 ${sidebarOpen ? 'pl-11' : 'pl-4'}`

    const iconClass = (isActive) =>
        `${isActive ? 'text-blue-500' : 'text-slate-500 group-hover:text-slate-300'
        } ${sidebarOpen ? 'mr-3' : 'mr-0'} h-4 w-4 flex-shrink-0 transition-colors duration-200`

    const chevronClass = (isOpen) =>
        `h-3 w-3 transition-transform duration-200 ${isOpen ? 'rotate-180' : ''}`

    return (
        <div className="min-h-screen bg-slate-50 flex font-sans selection:bg-blue-500/20">
            {/* Sidebar */}
            <div className={`${sidebarOpen ? 'w-64' : 'w-20'} bg-gradient-to-b from-[#0f172a] to-[#1e293b] border-r border-white/5 flex flex-col transition-all duration-300 ease-in-out shadow-2xl z-30`}>
                {/* Logo and Toggle */}
                <div className="h-20 flex items-center justify-between px-5 border-b border-white/5">
                    {sidebarOpen && (
                        <div className="flex items-center space-x-3">
                            <div className="h-9 w-9 rounded-xl bg-gradient-to-tr from-blue-600 to-sky-400 flex items-center justify-center flex-shrink-0 shadow-lg shadow-blue-500/20">
                                <svg className="h-5 w-5 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M13 10V3L4 14h7v7l9-11h-7z" />
                                </svg>
                            </div>
                            <div className="overflow-hidden">
                                <h1 className="text-sm font-bold text-white tracking-tight whitespace-nowrap font-outfit uppercase">Factura<span className="text-blue-400">Pro</span></h1>
                            </div>
                        </div>
                    )}
                    <button
                        onClick={() => setSidebarOpen(!sidebarOpen)}
                        className={`${sidebarOpen ? '' : 'mx-auto'} p-1.5 rounded-lg text-slate-500 hover:text-white hover:bg-white/5 transition-all duration-200`}
                    >
                        <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            {sidebarOpen ? (
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 19l-7-7 7-7m8 14l-7-7 7-7" />
                            ) : (
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 5l7 7-7 7M5 5l7 7-7 7" />
                            )}
                        </svg>
                    </button>
                </div>

                {/* Navigation */}
                <nav className="flex-1 px-0 py-6 space-y-1 overflow-y-auto scrollbar-hide">
                    {/* Dashboard */}
                    <NavLink to="/" end className={navLinkClass} title={!sidebarOpen ? "Dashboard" : ""}>
                        {({ isActive }) => (
                            <>
                                <svg className={iconClass(isActive)} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
                                </svg>
                                {sidebarOpen && <span className="font-medium">Inicio</span>}
                            </>
                        )}
                    </NavLink>

                    {/* Comprobantes Dropdown */}
                    <div
                        onMouseEnter={() => sidebarOpen && setOpenDropdown('comprobantes')}
                        onMouseLeave={() => setOpenDropdown(null)}
                    >
                        <div
                            className={dropdownParentClass(isComprobantesActive)}
                            title={!sidebarOpen ? "Facturas" : ""}
                        >
                            <div className="flex items-center">
                                <svg className={iconClass(isComprobantesActive)} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                                </svg>
                                {sidebarOpen && <span className="font-medium">Facturas</span>}
                            </div>
                            {sidebarOpen && (
                                <svg className={chevronClass(openDropdown === 'comprobantes')} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                                </svg>
                            )}
                        </div>
                        {sidebarOpen && openDropdown === 'comprobantes' && (
                            <div className="space-y-0.5 py-1 bg-white/10">
                                <NavLink to="/comprobantes" className={subNavLinkClass}>
                                    {({ isActive }) => (
                                        <>
                                            <div className={`w-1 h-1 rounded-full mr-3 ${isActive ? 'bg-blue-500 shadow-[0_0_8px_rgba(59,130,246,0.6)]' : 'bg-slate-700'}`}></div>
                                            <span>Nueva Factura</span>
                                        </>
                                    )}
                                </NavLink>
                                <NavLink to="/comprobantes/lista" className={subNavLinkClass}>
                                    {({ isActive }) => (
                                        <>
                                            <div className={`w-1 h-1 rounded-full mr-3 ${isActive ? 'bg-blue-500 shadow-[0_0_8px_rgba(59,130,246,0.6)]' : 'bg-slate-700'}`}></div>
                                            <span>Ver Lista</span>
                                        </>
                                    )}
                                </NavLink>
                            </div>
                        )}
                    </div>

                    {/* Presupuestos Dropdown */}
                    <div
                        onMouseEnter={() => sidebarOpen && setOpenDropdown('presupuestos')}
                        onMouseLeave={() => setOpenDropdown(null)}
                    >
                        <div
                            className={dropdownParentClass(isPresupuestosActive)}
                            title={!sidebarOpen ? "Presupuestos" : ""}
                        >
                            <div className="flex items-center">
                                <svg className={iconClass(isPresupuestosActive)} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" />
                                </svg>
                                {sidebarOpen && <span className="font-medium">Presupuestos</span>}
                            </div>
                            {sidebarOpen && (
                                <svg className={chevronClass(openDropdown === 'presupuestos')} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                                </svg>
                            )}
                        </div>
                        {sidebarOpen && openDropdown === 'presupuestos' && (
                            <div className="space-y-0.5 py-1 bg-white/10">
                                <NavLink to="/presupuestos" className={subNavLinkClass}>
                                    {({ isActive }) => (
                                        <>
                                            <div className={`w-1 h-1 rounded-full mr-3 ${isActive ? 'bg-blue-500 shadow-[0_0_8px_rgba(59,130,246,0.6)]' : 'bg-slate-700'}`}></div>
                                            <span>Nuevo Presupuesto</span>
                                        </>
                                    )}
                                </NavLink>
                                <NavLink to="/presupuestos/lista" className={subNavLinkClass}>
                                    {({ isActive }) => (
                                        <>
                                            <div className={`w-1 h-1 rounded-full mr-3 ${isActive ? 'bg-blue-500 shadow-[0_0_8px_rgba(59,130,246,0.6)]' : 'bg-slate-700'}`}></div>
                                            <span>Ver Lista</span>
                                        </>
                                    )}
                                </NavLink>
                            </div>
                        )}
                    </div>

                    {/* Caja Dropdown */}
                    <div
                        onMouseEnter={() => sidebarOpen && setOpenDropdown('caja')}
                        onMouseLeave={() => setOpenDropdown(null)}
                    >
                        <div
                            className={dropdownParentClass(isCajaActive)}
                            title={!sidebarOpen ? "Caja" : ""}
                        >
                            <div className="flex items-center">
                                <svg className={iconClass(isCajaActive)} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                                {sidebarOpen && <span className="font-medium">Caja</span>}
                            </div>
                            {sidebarOpen && (
                                <svg className={chevronClass(openDropdown === 'caja')} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                                </svg>
                            )}
                        </div>
                        {sidebarOpen && openDropdown === 'caja' && (
                            <div className="space-y-0.5 py-1 bg-white/10">
                                <NavLink to="/caja" end className={subNavLinkClass}>
                                    {({ isActive }) => (
                                        <>
                                            <div className={`w-1 h-1 rounded-full mr-3 ${isActive ? 'bg-blue-500 shadow-[0_0_8px_rgba(59,130,246,0.6)]' : 'bg-slate-700'}`}></div>
                                            <span>Sesión de Caja</span>
                                        </>
                                    )}
                                </NavLink>
                                <NavLink to="/caja/historial" className={subNavLinkClass}>
                                    {({ isActive }) => (
                                        <>
                                            <div className={`w-1 h-1 rounded-full mr-3 ${isActive ? 'bg-blue-500 shadow-[0_0_8px_rgba(59,130,246,0.6)]' : 'bg-slate-700'}`}></div>
                                            <span>Historial</span>
                                        </>
                                    )}
                                </NavLink>
                                {canManageUsers && (
                                    <NavLink to="/caja/administrar" className={subNavLinkClass}>
                                        {({ isActive }) => (
                                            <>
                                                <div className={`w-1 h-1 rounded-full mr-3 ${isActive ? 'bg-blue-500 shadow-[0_0_8px_rgba(59,130,246,0.6)]' : 'bg-slate-700'}`}></div>
                                                <span>Administrar Cajas</span>
                                            </>
                                        )}
                                    </NavLink>
                                )}
                            </div>
                        )}
                    </div>

                    <div className="px-6 py-4">
                        <div className="h-px bg-white/10 w-full"></div>
                    </div>

                    {/* Other Navigation Items */}
                    <NavLink to="/clientes" className={navLinkClass} title={!sidebarOpen ? "Clientes" : ""}>
                        {({ isActive }) => (
                            <>
                                <svg className={iconClass(isActive)} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                                </svg>
                                {sidebarOpen && <span className="font-medium">Clientes</span>}
                            </>
                        )}
                    </NavLink>
                    <NavLink to="/productos" className={navLinkClass} title={!sidebarOpen ? "Productos" : ""}>
                        {({ isActive }) => (
                            <>
                                <svg className={iconClass(isActive)} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                                </svg>
                                {sidebarOpen && <span className="font-medium">Productos</span>}
                            </>
                        )}
                    </NavLink>
                    {canManageUsers && (
                        <NavLink to="/usuarios" className={navLinkClass} title={!sidebarOpen ? "Usuarios" : ""}>
                            {({ isActive }) => (
                                <>
                                    <svg className={iconClass(isActive)} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
                                    </svg>
                                    {sidebarOpen && <span className="font-medium">Usuarios</span>}
                                </>
                            )}
                        </NavLink>
                    )}
                    {canManageUsers && (
                        <NavLink to="/afip-config" className={navLinkClass} title={!sidebarOpen ? "Configuración" : ""}>
                            <svg className={iconClass(location.pathname === '/afip-config')} fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                            </svg>
                            {sidebarOpen && <span className="font-medium">Configuración</span>}
                        </NavLink>
                    )}

                    {sidebarOpen && (
                        <div className="px-6 py-4 mt-auto">
                            <p className="text-[10px] items-center space-x-2 font-bold uppercase tracking-[0.2em] text-slate-600/50">
                                <span>v{version}</span>
                            </p>
                        </div>
                    )}
                </nav>

                {/* User Profile Section */}
                <div className="border-t border-white/5 p-4 bg-white/[0.02]">
                    {sidebarOpen ? (
                        <div className="space-y-3">
                            <button
                                onClick={() => navigate('/perfil')}
                                className="w-full flex items-center space-x-3 bg-white/5 rounded-xl px-3 py-2.5 border border-white/5 hover:bg-white/10 transition-all duration-200 cursor-pointer group"
                            >
                                <div className="flex-shrink-0">
                                    <div className="h-9 w-9 rounded-lg bg-blue-600/20 flex items-center justify-center overflow-hidden border border-blue-500/30">
                                        {user?.urlImagen ? (
                                            <img
                                                src={user.urlImagen.startsWith('http') ? user.urlImagen : `${API_BASE_URL.replace('/api', '')}${user.urlImagen}`}
                                                alt={user.nombre}
                                                className="h-full w-full object-cover"
                                            />
                                        ) : (
                                            <span className="text-blue-400 font-bold text-xs">
                                                {user?.nombre?.charAt(0).toUpperCase()}
                                            </span>
                                        )}
                                    </div>
                                </div>
                                <div className="text-left flex-1 min-w-0">
                                    <p className="text-xs font-bold text-white truncate font-outfit uppercase tracking-wider">{user?.nombre}</p>
                                    <p className="text-[10px] text-slate-400 truncate mt-0.5">@{user?.nombreUsuario}</p>
                                </div>
                                <svg className="w-4 h-4 text-slate-500 group-hover:text-slate-300 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                                </svg>
                            </button>
                            <button
                                onClick={handleLogout}
                                className="w-full inline-flex items-center justify-center px-4 py-2 text-xs font-bold uppercase tracking-widest rounded-lg text-slate-400 bg-white/5 hover:bg-red-500/10 hover:text-red-400 border border-white/5 transition-all duration-200"
                            >
                                <svg className="w-3.5 h-3.5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                                </svg>
                                Salir
                            </button>
                        </div>
                    ) : (
                        <div className="flex flex-col items-center space-y-4">
                            <button
                                onClick={() => navigate('/perfil')}
                                className="p-2 rounded-xl bg-white/5 border border-white/5 hover:bg-white/10 transition-all duration-200"
                                title="Perfil"
                            >
                                <div className="h-8 w-8 rounded-lg bg-blue-600/20 flex items-center justify-center overflow-hidden border border-blue-500/30">
                                    {user?.urlImagen ? (
                                        <img
                                            src={user.urlImagen.startsWith('http') ? user.urlImagen : `${API_BASE_URL.replace('/api', '')}${user.urlImagen}`}
                                            alt={user.nombre}
                                            className="h-full w-full object-cover"
                                        />
                                    ) : (
                                        <span className="text-blue-400 font-bold text-xs">
                                            {user?.nombre?.charAt(0).toUpperCase()}
                                        </span>
                                    )}
                                </div>
                            </button>
                            <button
                                onClick={handleLogout}
                                className="p-2 rounded-xl text-slate-500 hover:text-red-400 hover:bg-red-500/10 transition-all duration-200"
                                title="Salir"
                            >
                                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                                </svg>
                            </button>
                        </div>
                    )}
                </div>
            </div>

            {/* Main Content */}
            <div className="flex-1 flex flex-col overflow-hidden relative">
                <div className="absolute inset-0 pattern-dots opacity-[0.4] pointer-events-none"></div>

                {/* Header with Connectivity Status */}
                <header className="bg-white/80 backdrop-blur-md border-b border-slate-200 h-20 flex items-center justify-between px-6 sm:px-10 flex-shrink-0 z-20 sticky top-0">
                    <div className="flex items-center">
                        <h2 className="text-xl font-bold font-outfit text-slate-800 tracking-tight">
                            {location.pathname.split('/').pop().charAt(0).toUpperCase() + location.pathname.split('/').pop().slice(1) || 'Dashboard'}
                        </h2>
                    </div>

                    <div className="flex items-center space-x-4">
                        {!isOnline ? (
                            <span className="inline-flex items-center px-3 py-1.5 rounded-full text-[10px] font-bold uppercase tracking-wider bg-red-50 text-red-600 border border-red-100 shadow-sm" title="No hay conexión al backend o a la red. Guardado local habilitado.">
                                <span className="h-1.5 w-1.5 bg-red-500 rounded-full mr-2 animate-pulse shadow-[0_0_8px_rgba(239,68,68,0.5)]"></span>
                                Sin Conexión
                            </span>
                        ) : !isAfipOnline ? (
                            <span className="inline-flex items-center px-3 py-1.5 rounded-full text-[10px] font-bold uppercase tracking-wider bg-amber-50 text-amber-600 border border-amber-100 shadow-sm" title="Conexión con AFIP no disponible. Las facturas quedan pendientes.">
                                <span className="h-1.5 w-1.5 bg-amber-500 rounded-full mr-2 animate-pulse shadow-[0_0_8px_rgba(245,158,11,0.5)]"></span>
                                AFIP Offline
                            </span>
                        ) : (
                            <span className="inline-flex items-center px-3 py-1.5 rounded-full text-[10px] font-bold uppercase tracking-wider bg-emerald-50 text-emerald-600 border border-emerald-100 shadow-sm" title="En línea y conectado a AFIP">
                                <span className="h-1.5 w-1.5 bg-emerald-500 rounded-full mr-2 shadow-[0_0_8px_rgba(16,185,129,0.5)]"></span>
                                Online
                            </span>
                        )}

                        <div className="h-8 w-px bg-slate-200 mx-2"></div>

                        <div className="text-right hidden sm:block">
                            <p className="text-[10px] font-bold uppercase tracking-widest text-blue-600 font-outfit leading-none">Empresa</p>
                            <p className="text-sm font-bold text-slate-800 tracking-tight mt-1">{companyName}</p>
                        </div>
                    </div>
                </header>

                <main className="flex-1 overflow-auto relative scrollbar-thin">
                    <div className="max-w-7xl mx-auto px-6 sm:px-10 py-10">
                        <Outlet />
                    </div>
                </main>
            </div>
        </div>
    )
}

export default MainLayout
