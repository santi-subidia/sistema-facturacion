// Este archivo fue refactorizado. Las rutas de Caja ahora son:
// /caja          -> CajaSesion.jsx  (apertura, movimientos, cierre)
// /caja/historial -> CajaHistorial.jsx
// /caja/administrar -> CajaAdministrar.jsx (solo admin)
import { Navigate } from 'react-router-dom';
export default function Caja() {
    return <Navigate to="/caja" replace />;
}
