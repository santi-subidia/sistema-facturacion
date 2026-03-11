import React, { useState, useEffect, useRef } from 'react'
import { fetchWithAuth } from '../../utils/authHeaders'

function Autocomplete({
  placeholder,
  onSelect,
  searchEndpoint,
  renderItem,
  getItemLabel,
  minChars = 3,
  value,
  disabled = false
}) {
  const [query, setQuery] = useState('')
  const [suggestions, setSuggestions] = useState([])
  const [isLoading, setIsLoading] = useState(false)
  const [showSuggestions, setShowSuggestions] = useState(false)
  const [highlightedIndex, setHighlightedIndex] = useState(-1)
  const [dropdownPosition, setDropdownPosition] = useState('bottom')
  const wrapperRef = useRef(null)
  const debounceTimer = useRef(null)

  // Cerrar sugerencias cuando se hace clic fuera
  useEffect(() => {
    function handleClickOutside(event) {
      if (wrapperRef.current && !wrapperRef.current.contains(event.target)) {
        setShowSuggestions(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  // Actualizar query si value cambia externamente
  useEffect(() => {
    if (value && typeof value === 'object') {
      setQuery(getItemLabel(value))
    } else if (!value) {
      setQuery('')
    }
  }, [value, getItemLabel])

  // Calcular si hay espacio suficiente abajo para el dropdown
  const calculateDropdownPosition = () => {
    if (!wrapperRef.current) return

    const rect = wrapperRef.current.getBoundingClientRect()
    const spaceBelow = window.innerHeight - rect.bottom
    const spaceAbove = rect.top
    const dropdownHeight = 400 // Altura aproximada del dropdown

    // Si hay más espacio arriba que abajo y no hay suficiente espacio abajo
    if (spaceBelow < dropdownHeight && spaceAbove > spaceBelow) {
      setDropdownPosition('top')
    } else {
      setDropdownPosition('bottom')
    }
  }

  const fetchSuggestions = async (searchQuery) => {
    if (searchQuery.length < minChars) {
      setSuggestions([])
      return
    }

    setIsLoading(true)
    try {
      const url = `${searchEndpoint}?q=${encodeURIComponent(searchQuery)}`
      console.log('Fetching:', url)

      const response = await fetchWithAuth(url)

      if (!response.ok) {
        const errorText = await response.text()
        console.error('Error response:', response.status, errorText)
        throw new Error(`Error ${response.status}: ${response.statusText}`)
      }

      const data = await response.json()
      console.log('Search results:', data)
      setSuggestions(data.data || [])
      setShowSuggestions(true)

      // Calcular posición del dropdown después de mostrar sugerencias
      setTimeout(() => calculateDropdownPosition(), 0)
    } catch (error) {
      console.error('Error fetching suggestions:', error)
      setSuggestions([])
    } finally {
      setIsLoading(false)
    }
  }

  const handleInputChange = (e) => {
    const value = e.target.value
    setQuery(value)
    setHighlightedIndex(-1)

    // Limpiar el timer anterior
    if (debounceTimer.current) {
      clearTimeout(debounceTimer.current)
    }

    // Si el input está vacío, limpiar la selección
    if (value.trim() === '') {
      setSuggestions([])
      setShowSuggestions(false)
      onSelect(null)
      return
    }

    // Crear nuevo timer para hacer la búsqueda después de 300ms
    debounceTimer.current = setTimeout(() => {
      fetchSuggestions(value)
    }, 300)
  }

  const handleSelectItem = (item) => {
    setQuery(getItemLabel(item))
    setSuggestions([])
    setShowSuggestions(false)
    setHighlightedIndex(-1)
    onSelect(item)
  }

  const handleKeyDown = (e) => {
    if (!showSuggestions || suggestions.length === 0) return

    switch (e.key) {
      case 'ArrowDown':
        e.preventDefault()
        setHighlightedIndex(prev =>
          prev < suggestions.length - 1 ? prev + 1 : prev
        )
        break
      case 'ArrowUp':
        e.preventDefault()
        setHighlightedIndex(prev => prev > 0 ? prev - 1 : -1)
        break
      case 'Enter':
        e.preventDefault()
        if (highlightedIndex >= 0 && highlightedIndex < suggestions.length) {
          handleSelectItem(suggestions[highlightedIndex])
        }
        break
      case 'Escape':
        setShowSuggestions(false)
        setHighlightedIndex(-1)
        break
      default:
        break
    }
  }

  return (
    <div ref={wrapperRef} className="relative w-full">
      <input
        type="text"
        value={query}
        onChange={handleInputChange}
        onKeyDown={handleKeyDown}
        onFocus={() => query.length >= minChars && suggestions.length > 0 && setShowSuggestions(true)}
        placeholder={placeholder}
        disabled={disabled}
        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100 disabled:cursor-not-allowed"
        autoComplete="off"
      />

      {isLoading && (
        <div className="absolute right-3 top-1/2 transform -translate-y-1/2">
          <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-blue-600"></div>
        </div>
      )}

      {showSuggestions && suggestions.length > 0 && (
        <div className={`absolute z-50 w-full bg-white border border-gray-300 rounded-md shadow-xl overflow-auto ${dropdownPosition === 'top' ? 'bottom-full mb-1 max-h-96' : 'mt-1 max-h-96'
          }`}>
          {suggestions.map((item, index) => (
            <div
              key={item.id}
              onClick={() => handleSelectItem(item)}
              onMouseEnter={() => setHighlightedIndex(index)}
              className={`px-4 py-3 cursor-pointer transition-colors ${index === highlightedIndex
                  ? 'bg-blue-100'
                  : 'hover:bg-gray-50'
                }`}
            >
              {renderItem(item)}
            </div>
          ))}
        </div>
      )}

      {showSuggestions && !isLoading && query.length >= minChars && suggestions.length === 0 && (
        <div className={`absolute z-50 w-full bg-white border border-gray-300 rounded-md shadow-lg ${dropdownPosition === 'top' ? 'bottom-full mb-1' : 'mt-1'
          }`}>
          <div className="px-4 py-3 text-gray-500 text-sm">
            No se encontraron resultados
          </div>
        </div>
      )}

      {query.length > 0 && query.length < minChars && !showSuggestions && (
        <div className={`absolute z-50 w-full bg-white border border-gray-300 rounded-md shadow-lg ${dropdownPosition === 'top' ? 'bottom-full mb-1' : 'mt-1'
          }`}>
          <div className="px-4 py-3 text-gray-500 text-sm">
            Escriba al menos {minChars} caracteres
          </div>
        </div>
      )}
    </div>
  )
}

export default Autocomplete
