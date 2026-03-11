import React from 'react'

function Pagination({ pagination, currentPage, onPageChange, itemName = 'items' }) {
  if (!pagination || pagination.totalPages <= 1) return null

  const getPageNumbers = () => {
    const pages = [];
    const maxVisiblePages = 5;
    const { totalPages } = pagination;

    if (totalPages <= maxVisiblePages) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      let startPage = Math.max(1, currentPage - 2);
      let endPage = Math.min(totalPages, currentPage + 2);

      if (currentPage <= 3) {
        endPage = 5;
      } else if (currentPage + 2 >= totalPages) {
        startPage = totalPages - 4;
      }

      for (let i = startPage; i <= endPage; i++) {
        pages.push(i);
      }

      if (startPage > 1) {
        if (startPage > 2) pages.unshift('...');
        pages.unshift(1);
      }

      if (endPage < totalPages) {
        if (endPage < totalPages - 1) pages.push('...');
        pages.push(totalPages);
      }
    }
    return pages;
  };

  return (
    <div className="bg-white px-4 py-3 border-t border-slate-200 sm:px-6">
      <div className="flex items-center justify-between">
        <div className="flex-1 flex justify-between sm:hidden">
          <button
            onClick={() => onPageChange(currentPage - 1)}
            disabled={!pagination.hasPrevious}
            className={`relative inline-flex items-center px-4 py-2 border border-slate-300 text-sm font-medium rounded-xl ${pagination.hasPrevious
              ? 'text-slate-700 bg-white hover:bg-slate-50'
              : 'text-slate-400 bg-slate-50 cursor-not-allowed'
              }`}
          >
            Anterior
          </button>
          <button
            onClick={() => onPageChange(currentPage + 1)}
            disabled={!pagination.hasNext}
            className={`ml-3 relative inline-flex items-center px-4 py-2 border border-slate-300 text-sm font-medium rounded-xl ${pagination.hasNext
              ? 'text-slate-700 bg-white hover:bg-slate-50'
              : 'text-slate-400 bg-slate-50 cursor-not-allowed'
              }`}
          >
            Siguiente
          </button>
        </div>
        <div className="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
          <div>
            <p className="text-sm text-slate-700">
              Mostrando página <span className="font-medium">{pagination.currentPage}</span> de{' '}
              <span className="font-medium">{pagination.totalPages}</span> ({pagination.totalItems} {itemName} en total)
            </p>
          </div>
          <div>
            <nav className="relative z-0 inline-flex rounded-xl shadow-sm -space-x-px">
              <button
                onClick={() => onPageChange(currentPage - 1)}
                disabled={!pagination.hasPrevious}
                className={`relative inline-flex items-center px-2 py-2 rounded-l-xl border border-slate-300 text-sm font-medium ${pagination.hasPrevious
                  ? 'text-slate-500 bg-white hover:bg-slate-50'
                  : 'text-slate-300 bg-slate-50 cursor-not-allowed'
                  }`}
              >
                <span className="sr-only">Anterior</span>
                <svg className="h-5 w-5" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M12.707 5.293a1 1 0 010 1.414L9.414 10l3.293 3.293a1 1 0 01-1.414 1.414l-4-4a1 1 0 010-1.414l4-4a1 1 0 011.414 0z" clipRule="evenodd" />
                </svg>
              </button>

              {getPageNumbers().map((page, index) => (
                page === '...' ? (
                  <span
                    key={`ellipsis-${index}`}
                    className="relative inline-flex items-center px-4 py-2 border border-slate-300 bg-white text-sm font-medium text-slate-500"
                  >
                    ...
                  </span>
                ) : (
                  <button
                    key={page}
                    onClick={() => onPageChange(page)}
                    className={`relative inline-flex items-center px-4 py-2 border text-sm font-medium transition-colors ${page === currentPage
                      ? 'z-10 bg-blue-50 border-blue-500 text-blue-600 font-bold'
                      : 'bg-white border-slate-300 text-slate-500 hover:bg-slate-50 cursor-pointer'
                      }`}
                  >
                    {page}
                  </button>
                )
              ))}

              <button
                onClick={() => onPageChange(currentPage + 1)}
                disabled={!pagination.hasNext}
                className={`relative inline-flex items-center px-2 py-2 rounded-r-xl border border-slate-300 text-sm font-medium ${pagination.hasNext
                  ? 'text-slate-500 bg-white hover:bg-slate-50'
                  : 'text-slate-300 bg-slate-50 cursor-not-allowed'
                  }`}
              >
                <span className="sr-only">Siguiente</span>
                <svg className="h-5 w-5" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clipRule="evenodd" />
                </svg>
              </button>
            </nav>
          </div>
        </div>
      </div>
    </div>
  )
}

export default Pagination
