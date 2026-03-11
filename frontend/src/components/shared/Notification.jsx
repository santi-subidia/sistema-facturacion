import React from 'react'

function Notification({ notification, onClose }) {
  if (!notification || !notification.show) return null

  return (
    <div className={`fixed top-4 right-4 z-[60] max-w-md w-full shadow-lg rounded-2xl pointer-events-auto ring-1 ring-black ring-opacity-5 overflow-hidden transform transition-all duration-300 ${notification.type === 'success' ? 'bg-emerald-50' : 'bg-red-50'
      }`}>
      <div className="p-4">
        <div className="flex items-start">
          <div className="flex-shrink-0">
            {notification.type === 'success' ? (
              <svg className="h-6 w-6 text-emerald-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            ) : (
              <svg className="h-6 w-6 text-red-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            )}
          </div>
          <div className="ml-3 w-0 flex-1 pt-0.5">
            <p className={`text-sm font-medium ${notification.type === 'success' ? 'text-emerald-900' : 'text-red-900'
              }`}>
              {notification.message}
            </p>
          </div>
          <div className="ml-4 flex-shrink-0 flex">
            <button
              onClick={onClose}
              className={`inline-flex rounded-xl p-1 ${notification.type === 'success' ? 'text-emerald-500 hover:text-emerald-600 hover:bg-emerald-100' : 'text-red-500 hover:text-red-600 hover:bg-red-100'
                } focus:outline-none transition-colors`}
            >
              <span className="sr-only">Cerrar</span>
              <svg className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clipRule="evenodd" />
              </svg>
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}

export default Notification
