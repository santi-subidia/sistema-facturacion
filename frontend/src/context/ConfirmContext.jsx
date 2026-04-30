import React, { createContext, useContext, useState } from 'react';
import ConfirmModal from '../components/shared/ConfirmModal';

const ConfirmContext = createContext();

export const ConfirmProvider = ({ children }) => {
  const [modalState, setModalState] = useState({
    show: false,
    title: '',
    message: '',
    confirmText: 'Confirmar',
    cancelText: 'Cancelar',
    type: 'warning',
    onConfirm: () => {},
    onCancel: () => {}
  });

  const confirm = (options) => {
    return new Promise((resolve) => {
      setModalState({
        show: true,
        title: options.title || 'Confirmación',
        message: options.message || '¿Está seguro de realizar esta acción?',
        confirmText: options.confirmText || 'Confirmar',
        cancelText: options.cancelText || 'Cancelar',
        type: options.type || 'warning',
        onConfirm: () => {
          setModalState((prev) => ({ ...prev, show: false }));
          resolve(true);
        },
        onCancel: () => {
          setModalState((prev) => ({ ...prev, show: false }));
          resolve(false);
        }
      });
    });
  };

  const alert = (options) => {
    const message = typeof options === 'string' ? options : options.message;
    const title = typeof options === 'string' ? 'Información' : (options.title || 'Información');
    const type = typeof options === 'string' ? 'info' : (options.type || 'info');

    return new Promise((resolve) => {
      setModalState({
        show: true,
        title,
        message,
        confirmText: 'Aceptar',
        cancelText: null, // Si es null, el componente no debería mostrarlo
        type,
        onConfirm: () => {
          setModalState((prev) => ({ ...prev, show: false }));
          resolve(true);
        },
        onCancel: () => {
          setModalState((prev) => ({ ...prev, show: false }));
          resolve(true);
        }
      });
    });
  };

  return (
    <ConfirmContext.Provider value={{ confirm, alert }}>
      {children}
      <ConfirmModal 
        {...modalState}
        onConfirm={modalState.onConfirm}
        onCancel={modalState.onCancel}
      />
    </ConfirmContext.Provider>
  );
};

export const useConfirm = () => {
  const context = useContext(ConfirmContext);
  if (!context) {
    throw new Error('useConfirm must be used within a ConfirmProvider');
  }
  return context;
};
