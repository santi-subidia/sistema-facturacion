import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useCajaGlobal } from '../../context/CajaContext';

const CajaStatusBadge = () => {
  const { sesionActiva, loading } = useCajaGlobal();
  const navigate = useNavigate();

  if (loading && !sesionActiva) return null;

  const isOpen = !!sesionActiva;

  return (
    <button
      onClick={() => navigate('/caja')}
      className={`inline-flex items-center px-3 py-1.5 rounded-xl text-[10px] font-bold uppercase tracking-wider transition-all duration-200 shadow-sm border ${
        isOpen 
          ? 'bg-emerald-50 text-emerald-600 border-emerald-100 hover:bg-emerald-100' 
          : 'bg-slate-50 text-slate-500 border-slate-200 hover:bg-slate-100'
      }`}
    >
      <span className={`h-1.5 w-1.5 rounded-full mr-2 ${
        isOpen ? 'bg-emerald-500 animate-pulse' : 'bg-slate-400'
      } shadow-[0_0_8px_rgba(16,185,129,0.4)]`}></span>
      Caja {isOpen ? `Abierta: ${sesionActiva.caja?.nombre || 'General'}` : 'Cerrada'}
    </button>
  );
};

export default CajaStatusBadge;
