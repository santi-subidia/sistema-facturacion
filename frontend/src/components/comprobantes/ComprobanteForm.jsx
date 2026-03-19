import React, { useState, useEffect } from "react";
import Autocomplete from "../shared/Autocomplete";
import ComprobanteConfirmacionModal from "./ComprobanteConfirmacionModal";
import { API_BASE_URL } from "../../config";

function ComprobanteForm({
  clientes,
  productos,
  tiposComprobante,
  formasPago,
  condicionesVenta,
  onSubmit,
  submitting,
  showNotification,
}) {
  const [tipoCliente, setTipoCliente] = useState("consumidor_final"); // 'consumidor_final', 'consumidor_con_datos', 'cliente_habitual'

  const [formData, setFormData] = useState({
    idCliente: "",
    idTipoComprobante: "",
    idFormaPago: "",
    idCondicionVenta: "",
    porcentajeAjuste: 0,
    clienteDocumento: "",
    clienteNombre: "",
    clienteApellido: "",
    clienteTelefono: "",
    clienteCorreo: "",
    clienteDireccion: "",
    detalles: [],
  });

  const [clienteSeleccionado, setClienteSeleccionado] = useState(null);
  const [productoSeleccionado, setProductoSeleccionado] = useState(null);
  const [selectedProducto, setSelectedProducto] = useState("");
  const [cantidad, setCantidad] = useState(1);
  const [precio, setPrecio] = useState(0);
  const [formaPagoSeleccionada, setFormaPagoSeleccionada] = useState(null);

  // Estados para productos genéricos
  const [tipoProducto, setTipoProducto] = useState("catalogo"); // 'catalogo' o 'generico'
  const [productoGenerico, setProductoGenerico] = useState({
    nombre: "",
    codigo: "",
    cantidad: 1,
    precio: 0,
  });

  const [showConfirmation, setShowConfirmation] = useState(false);

  // Establecer valores por defecto cuando se cargan los datos
  useEffect(() => {
    if (
      tiposComprobante && tiposComprobante.length > 0 &&
      formasPago.length > 0 &&
      condicionesVenta.length > 0
    ) {
      // Buscar IDs de los valores por defecto
      const facturaC = tiposComprobante.find((tf) =>
        tf.nombre?.toLowerCase().includes("factura c") || tf.nombre?.toLowerCase().includes("comprobante")
      );
      const tarjeta = formasPago.find((fp) =>
        fp.nombre?.toLowerCase().includes("tarjeta"),
      );
      const contado = condicionesVenta.find((cv) =>
        cv.nombre?.toLowerCase().includes("contado"),
      );

      // Establecer valores por defecto solo si el formulario está vacío
      if (
        !formData.idTipoComprobante &&
        !formData.idFormaPago &&
        !formData.idCondicionVenta
      ) {
        const nuevosValores = {
          ...formData,
          idTipoComprobante: facturaC?.id || tiposComprobante[0]?.id || "",
          idFormaPago: tarjeta?.id || formasPago[0]?.id || "",
          idCondicionVenta: contado?.id || condicionesVenta[0]?.id || "",
          porcentajeAjuste: tarjeta?.porcentajeAjuste || 0,
        };

        setFormData(nuevosValores);

        if (tarjeta) {
          setFormaPagoSeleccionada(tarjeta);
        }
      }
    }
  }, [tiposComprobante, formasPago, condicionesVenta]);

  // Resetear formulario después de envío exitoso
  useEffect(() => {
    if (!submitting) {
      // Solo resetear si no estamos submitting
    }
  }, [submitting]);

  const handleClienteSelect = (cliente) => {
    setClienteSeleccionado(cliente);
    if (cliente) {
      setFormData({
        ...formData,
        idCliente: cliente.id,
        clienteDocumento: "",
        clienteNombre: "",
        clienteApellido: "",
        clienteTelefono: "",
        clienteCorreo: "",
        clienteDireccion: "",
      });
    } else {
      setFormData({
        ...formData,
        idCliente: "",
        clienteDocumento: "",
        clienteNombre: "",
        clienteApellido: "",
        clienteTelefono: "",
        clienteCorreo: "",
        clienteDireccion: "",
      });
    }
  };

  const handleTipoClienteChange = (tipo) => {
    setTipoCliente(tipo);
    // Limpiar datos según el tipo
    if (tipo === "consumidor_final") {
      setFormData({
        ...formData,
        idCliente: "",
        clienteDocumento: "",
        clienteNombre: "",
        clienteApellido: "",
        clienteTelefono: "",
        clienteCorreo: "",
        clienteDireccion: "",
      });
      setClienteSeleccionado(null);
    } else if (tipo === "cliente_habitual") {
      setFormData({
        ...formData,
        clienteDocumento: "",
        clienteNombre: "",
        clienteApellido: "",
        clienteTelefono: "",
        clienteCorreo: "",
        clienteDireccion: "",
      });
    }
  };

  const handleClienteFieldChange = (field, value) => {
    setFormData({
      ...formData,
      [field]: value,
    });
  };

  const handleFormaPagoChange = (e) => {
    const formaPagoId = parseInt(e.target.value);
    const formaPago = formasPago.find((fp) => fp.id === formaPagoId);

    setFormaPagoSeleccionada(formaPago);
    setFormData({
      ...formData,
      idFormaPago: formaPagoId,
      porcentajeAjuste: formaPago?.porcentajeAjuste || 0,
    });
  };

  const handlePorcentajeAjusteChange = (e) => {
    setFormData({
      ...formData,
      porcentajeAjuste: parseFloat(e.target.value) || 0,
    });
  };

  const handleProductoSelect = (producto) => {
    setProductoSeleccionado(producto);
    if (producto) {
      setSelectedProducto(producto.id);
      setPrecio(producto.precio);
    } else {
      setSelectedProducto("");
      setPrecio(0);
    }
  };

  const handleAgregarProducto = () => {
    if (tipoProducto === "catalogo") {
      // Validaciones para producto del catálogo
      if (!selectedProducto || cantidad <= 0 || precio <= 0) {
        showNotification?.(
          "error",
          "Por favor complete todos los campos del producto",
        );
        return;
      }

      const producto = productoSeleccionado;

      if (!producto) {
        showNotification?.("error", "Producto no encontrado");
        return;
      }

      if (cantidad > producto.stock) {
        showNotification?.(
          "error",
          `Stock insuficiente. Disponible: ${producto.stock}`,
        );
        return;
      }

      // Verificar si el producto ya está en la lista
      const existente = formData.detalles.find(
        (d) => d.idProducto === selectedProducto,
      );
      if (existente) {
        showNotification?.(
          "error",
          "Este producto ya está agregado. Puede editar la cantidad desde la lista.",
        );
        return;
      }

      const nuevoDetalle = {
        idProducto: selectedProducto,
        producto: producto,
        productoNombre: producto.nombre,
        productoCodigo: producto.codigo,
        cantidad: parseInt(cantidad),
        precio: parseFloat(precio),
        subtotal: cantidad * precio,
        esGenerico: false,
      };

      setFormData({
        ...formData,
        detalles: [...formData.detalles, nuevoDetalle],
      });

      // Limpiar selección
      setProductoSeleccionado(null);
      setSelectedProducto("");
      setCantidad(1);
      setPrecio(0);
    } else {
      // Validaciones para producto genérico
      if (
        !productoGenerico.nombre.trim() ||
        productoGenerico.cantidad <= 0 ||
        productoGenerico.precio <= 0
      ) {
        showNotification?.(
          "error",
          "Por favor complete el nombre, cantidad y precio del producto genérico",
        );
        return;
      }

      const nuevoDetalle = {
        idProducto: null,
        producto: null,
        productoNombre: productoGenerico.nombre,
        productoCodigo: productoGenerico.codigo || null,
        cantidad: parseInt(productoGenerico.cantidad),
        precio: parseFloat(productoGenerico.precio),
        subtotal: productoGenerico.cantidad * productoGenerico.precio,
        esGenerico: true,
      };

      setFormData({
        ...formData,
        detalles: [...formData.detalles, nuevoDetalle],
      });

      // Limpiar campos genéricos
      setProductoGenerico({
        nombre: "",
        codigo: "",
        cantidad: 1,
        precio: 0,
      });
    }
  };

  const handleEliminarDetalle = (index) => {
    const nuevosDetalles = formData.detalles.filter((_, i) => i !== index);
    setFormData({ ...formData, detalles: nuevosDetalles });
  };

  const handleCantidadDetalleChange = (index, nuevaCantidad) => {
    const cantidad = parseInt(nuevaCantidad);
    if (cantidad <= 0 || isNaN(cantidad)) return;

    const detalle = formData.detalles[index];

    // Solo validar stock si es un producto del catálogo
    if (!detalle.esGenerico) {
      const producto = detalle.producto;
      if (cantidad > producto.stock) {
        showNotification?.(
          "error",
          `Stock insuficiente. Disponible: ${producto.stock}`,
        );
        return;
      }
    }

    const nuevosDetalles = [...formData.detalles];
    nuevosDetalles[index].cantidad = cantidad;
    nuevosDetalles[index].subtotal = cantidad * nuevosDetalles[index].precio;
    setFormData({ ...formData, detalles: nuevosDetalles });
  };

  const calcularTotal = () => {
    const subtotal = formData.detalles.reduce(
      (total, detalle) => total + detalle.subtotal,
      0,
    );
    const ajuste = subtotal * (formData.porcentajeAjuste / 100);
    return subtotal + ajuste;
  };

  const calcularSubtotal = () => {
    return formData.detalles.reduce(
      (total, detalle) => total + detalle.subtotal,
      0,
    );
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!formData.idTipoComprobante) {
      showNotification?.("error", "Debe seleccionar un tipo de comprobante");
      return;
    }

    if (!formData.idFormaPago) {
      showNotification?.("error", "Debe seleccionar una forma de pago");
      return;
    }

    if (!formData.idCondicionVenta) {
      showNotification?.("error", "Debe seleccionar una condición de venta");
      return;
    }

    if (formData.detalles.length === 0) {
      showNotification?.("error", "Debe agregar al menos un producto");
      return;
    }

    if (tipoCliente === "cliente_habitual" && !formData.idCliente) {
      showNotification?.("error", "Debe seleccionar un cliente habitual");
      return;
    }

    // Validaciones pasaron, mostrar modal de confirmación
    setShowConfirmation(true);
  };

  const handleConfirmar = () => {
    // Preparar datos para enviar
    const comprobanteData = {
      idCliente: tipoCliente === "cliente_habitual" ? formData.idCliente : null,
      idTipoComprobante: formData.idTipoComprobante,
      idFormaPago: formData.idFormaPago,
      idCondicionVenta: formData.idCondicionVenta,
      porcentajeAjuste: formData.porcentajeAjuste,
      clienteDocumento:
        tipoCliente === "consumidor_final"
          ? formData.clienteDocumento || null
          : null,
      clienteNombre:
        tipoCliente === "consumidor_final"
          ? formData.clienteNombre || null
          : null,
      clienteApellido:
        tipoCliente === "consumidor_final"
          ? formData.clienteApellido || null
          : null,
      clienteTelefono:
        tipoCliente === "consumidor_final"
          ? formData.clienteTelefono || null
          : null,
      clienteCorreo:
        tipoCliente === "consumidor_final"
          ? formData.clienteCorreo || null
          : null,
      clienteDireccion:
        tipoCliente === "consumidor_final"
          ? formData.clienteDireccion || null
          : null,
      detalles: formData.detalles.map((d) => ({
        idProducto: d.esGenerico ? null : d.idProducto,
        productoNombre: d.esGenerico ? d.productoNombre : null,
        productoCodigo: d.esGenerico ? d.productoCodigo : null,
        cantidad: d.cantidad,
        precio: d.precio,
      })),
    };

    onSubmit(comprobanteData);
    setShowConfirmation(false);

    // Limpiar formulario después de envío exitoso
    setTipoCliente("consumidor_final");
    setTipoProducto("catalogo");
    setFormData({
      idCliente: "",
      idTipoComprobante: "",
      idFormaPago: "",
      idCondicionVenta: "",
      porcentajeAjuste: 0,
      clienteDocumento: "",
      clienteNombre: "",
      clienteApellido: "",
      clienteTelefono: "",
      clienteCorreo: "",
      clienteDireccion: "",
      detalles: [],
    });
    setClienteSeleccionado(null);
    setProductoSeleccionado(null);
    setFormaPagoSeleccionada(null);
    setProductoGenerico({
      nombre: "",
      codigo: "",
      cantidad: 1,
      precio: 0,
    });
  };

  const handleTipoComprobanteChange = (e) => {
    setFormData({ ...formData, idTipoComprobante: parseInt(e.target.value) });
  };


  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Información del Cliente */}
      <div className="bg-gray-50 p-4 rounded-lg space-y-4">
        <h3 className="text-lg font-medium text-gray-900">
          Información del Cliente
        </h3>

        {/* Selector de Tipo de Cliente */}
        <div className="space-y-2">
          <label className="block text-sm font-medium text-gray-700">
            Tipo de Cliente *
          </label>
          <div className="flex gap-4">
            <label className="flex items-center cursor-pointer">
              <input
                type="radio"
                name="tipoCliente"
                value="consumidor_final"
                checked={tipoCliente === "consumidor_final"}
                onChange={(e) => handleTipoClienteChange(e.target.value)}
                className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
                disabled={submitting}
              />
              <span className="ml-2 text-sm text-gray-700">
                Consumidor Final
              </span>
            </label>

            <label className="flex items-center cursor-pointer">
              <input
                type="radio"
                name="tipoCliente"
                value="cliente_habitual"
                checked={tipoCliente === "cliente_habitual"}
                onChange={(e) => handleTipoClienteChange(e.target.value)}
                className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
                disabled={submitting}
              />
              <span className="ml-2 text-sm text-gray-700">
                Cliente Habitual
              </span>
            </label>
          </div>
        </div>

        {/* Campos según tipo de cliente */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {tipoCliente === "cliente_habitual" && (
            <>
              <div className="md:col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Buscar Cliente *
                </label>
                <Autocomplete
                  placeholder="Buscar cliente por nombre, apellido o documento..."
                  searchEndpoint={`${API_BASE_URL}/cliente/buscar`}
                  onSelect={handleClienteSelect}
                  value={clienteSeleccionado}
                  getItemLabel={(cliente) =>
                    `${cliente.nombre} ${cliente.apellido} - ${cliente.documento}`
                  }
                  renderItem={(cliente) => (
                    <div>
                      <div className="font-medium">
                        {cliente.nombre} {cliente.apellido}
                      </div>
                      <div className="text-sm text-gray-500">
                        {cliente.documento}
                      </div>
                    </div>
                  )}
                  disabled={submitting}
                />
              </div>

              {clienteSeleccionado && (
                <div className="md:col-span-2 p-3 bg-blue-50 rounded border border-blue-200">
                  <p className="text-sm text-gray-700">
                    <strong>Email:</strong> {clienteSeleccionado.correo} |
                    <strong className="ml-3">Teléfono:</strong>{" "}
                    {clienteSeleccionado.telefono} |
                    <strong className="ml-3">Dirección:</strong>{" "}
                    {clienteSeleccionado.direccion}
                  </p>
                </div>
              )}
            </>
          )}

          {tipoCliente === "consumidor_final" && (
            <>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Documento
                </label>
                <input
                  type="text"
                  value={formData.clienteDocumento}
                  onChange={(e) =>
                    handleClienteFieldChange("clienteDocumento", e.target.value)
                  }
                  placeholder="12345678 o 20-12345678-9"
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  disabled={submitting}
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Nombre
                </label>
                <input
                  type="text"
                  value={formData.clienteNombre}
                  onChange={(e) =>
                    handleClienteFieldChange("clienteNombre", e.target.value)
                  }
                  placeholder="Nombre del cliente"
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  disabled={submitting}
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Apellido
                </label>
                <input
                  type="text"
                  value={formData.clienteApellido}
                  onChange={(e) =>
                    handleClienteFieldChange("clienteApellido", e.target.value)
                  }
                  placeholder="Apellido del cliente"
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  disabled={submitting}
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Teléfono
                </label>
                <input
                  type="tel"
                  value={formData.clienteTelefono}
                  onChange={(e) =>
                    handleClienteFieldChange("clienteTelefono", e.target.value)
                  }
                  placeholder="+54 11 1234-5678"
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  disabled={submitting}
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Correo Electrónico
                </label>
                <input
                  type="email"
                  value={formData.clienteCorreo}
                  onChange={(e) =>
                    handleClienteFieldChange("clienteCorreo", e.target.value)
                  }
                  placeholder="cliente@ejemplo.com"
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  disabled={submitting}
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Dirección
                </label>
                <input
                  type="text"
                  value={formData.clienteDireccion}
                  onChange={(e) =>
                    handleClienteFieldChange("clienteDireccion", e.target.value)
                  }
                  placeholder="Calle 123, Ciudad"
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                  disabled={submitting}
                />
              </div>
            </>
          )}

          <div
            className={
              tipoCliente === "consumidor_final" ? "md:col-span-2" : ""
            }
          >
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Tipo de Factura *
            </label>
            <select
              value={formData.idTipoComprobante}
              onChange={handleTipoComprobanteChange}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              required
            >
              <option value="">Seleccione tipo</option>
              {tiposComprobante && tiposComprobante
                .filter(tipo => tipo.nombre?.toLowerCase().includes("factura"))
                .map((tipo) => (
                  <option key={tipo.id} value={tipo.id}>
                    {tipo.nombre}
                  </option>
                ))}
            </select>
          </div>

          <div
            className={
              tipoCliente === "consumidor_final" ? "md:col-span-2" : ""
            }
          >
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Forma de Pago *
            </label>
            <select
              value={formData.idFormaPago}
              onChange={handleFormaPagoChange}
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              required
            >
              <option value="">Seleccione forma de pago</option>
              {formasPago.map((forma) => (
                <option key={forma.id} value={forma.id}>
                  {forma.nombre}{" "}
                  {forma.porcentajeAjuste !== null && !forma.esEditable
                    ? `(+${forma.porcentajeAjuste}%)`
                    : ""}
                </option>
              ))}
            </select>
          </div>

          <div
            className={
              tipoCliente === "consumidor_final" ? "md:col-span-2" : ""
            }
          >
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Condición de Venta *
            </label>
            <select
              value={formData.idCondicionVenta}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  idCondicionVenta: parseInt(e.target.value),
                })
              }
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              required
            >
              <option value="">Seleccione condición</option>
              {condicionesVenta?.map((condicion) => (
                <option key={condicion.id} value={condicion.id}>
                  {condicion.descripcion}{" "}
                  {condicion.diasVencimiento > 0
                    ? `(${condicion.diasVencimiento} días)`
                    : ""}
                </option>
              ))}
            </select>
          </div>

          {formaPagoSeleccionada?.esEditable && (
            <div
              className={
                tipoCliente === "consumidor_final" ? "md:col-span-2" : ""
              }
            >
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Porcentaje de Ajuste (%) *
              </label>
              <input
                type="number"
                step="0.01"
                value={formData.porcentajeAjuste}
                onChange={handlePorcentajeAjusteChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="Ingrese el porcentaje (ej: 15 para 15%)"
                required
              />
              <p className="mt-1 text-xs text-gray-500">
                Ingrese el porcentaje de ajuste a aplicar sobre el subtotal
              </p>
            </div>
          )}
        </div>
      </div>

      {/* Agregar Productos */}
      <div className="bg-gray-50 p-4 rounded-lg space-y-4">
        <h3 className="text-lg font-medium text-gray-900">Agregar Productos</h3>

        {/* Toggle Tipo de Producto */}
        <div className="flex gap-4 items-center">
          <label className="flex items-center cursor-pointer">
            <input
              type="radio"
              name="tipoProducto"
              value="catalogo"
              checked={tipoProducto === "catalogo"}
              onChange={(e) => setTipoProducto(e.target.value)}
              className="mr-2"
              disabled={submitting}
            />
            <span className="text-sm font-medium text-gray-700">
              Producto del Catálogo
            </span>
          </label>
          <label className="flex items-center cursor-pointer">
            <input
              type="radio"
              name="tipoProducto"
              value="generico"
              checked={tipoProducto === "generico"}
              onChange={(e) => setTipoProducto(e.target.value)}
              className="mr-2"
              disabled={submitting}
            />
            <span className="text-sm font-medium text-gray-700">
              Producto Genérico
            </span>
          </label>

        </div>

        {tipoProducto === "catalogo" ? (
          // Campos para Producto del Catálogo
          <div className="grid grid-cols-1 md:grid-cols-12 gap-4 items-end">
            <div className="md:col-span-5">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Producto
              </label>
              <Autocomplete
                placeholder="Buscar producto por nombre, código o proveedor..."
                searchEndpoint={`${API_BASE_URL}/productos/buscar`}
                onSelect={handleProductoSelect}
                value={productoSeleccionado}
                getItemLabel={(producto) =>
                  `${producto.nombre} - ${producto.codigo}`
                }
                renderItem={(producto) => (
                  <div>
                    <div className="font-medium">{producto.nombre}</div>
                    <div className="text-sm text-gray-500">
                      Código: {producto.codigo} | Stock: {producto.stock} | $
                      {producto.precio}
                    </div>
                  </div>
                )}
                disabled={submitting}
              />
            </div>

            <div className="md:col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Cantidad
              </label>
              <input
                type="number"
                min="1"
                value={cantidad}
                onChange={(e) => setCantidad(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div className="md:col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Precio
              </label>
              <input
                type="number"
                step="0.01"
                min="0"
                value={precio}
                onChange={(e) => setPrecio(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>

            <div className="md:col-span-3">
              <button
                type="button"
                onClick={handleAgregarProducto}
                className="w-full px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 transition-colors"
              >
                Agregar
              </button>
            </div>
          </div>
        ) : (
          // Campos para Producto Genérico
          <div className="grid grid-cols-1 md:grid-cols-12 gap-4 items-end">
            <div className="md:col-span-4">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Nombre del Producto *
              </label>
              <input
                type="text"
                value={productoGenerico.nombre}
                onChange={(e) =>
                  setProductoGenerico({
                    ...productoGenerico,
                    nombre: e.target.value,
                  })
                }
                placeholder="Ej: Materiales eléctricos"
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                disabled={submitting}
              />
            </div>

            <div className="md:col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Código (opcional)
              </label>
              <input
                type="text"
                value={productoGenerico.codigo}
                onChange={(e) =>
                  setProductoGenerico({
                    ...productoGenerico,
                    codigo: e.target.value,
                  })
                }
                placeholder="Código"
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                disabled={submitting}
              />
            </div>

            <div className="md:col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Cantidad *
              </label>
              <input
                type="number"
                min="1"
                value={productoGenerico.cantidad}
                onChange={(e) =>
                  setProductoGenerico({
                    ...productoGenerico,
                    cantidad: e.target.value,
                  })
                }
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                disabled={submitting}
              />
            </div>

            <div className="md:col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Precio *
              </label>
              <input
                type="number"
                step="0.01"
                min="0"
                value={productoGenerico.precio}
                onChange={(e) =>
                  setProductoGenerico({
                    ...productoGenerico,
                    precio: e.target.value,
                  })
                }
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                disabled={submitting}
              />
            </div>

            <div className="md:col-span-2">
              <button
                type="button"
                onClick={handleAgregarProducto}
                className="w-full px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500 transition-colors"
              >
                Agregar
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Lista de Productos */}
      {formData.detalles.length > 0 && (
        <div className="bg-white border border-gray-200 rounded-lg overflow-hidden">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Producto
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Código
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Cantidad
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Precio Unit.
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Subtotal
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Acciones
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {formData.detalles.map((detalle, index) => (
                  <tr
                    key={index}
                    className={detalle.esGenerico ? "bg-blue-50" : ""}
                  >
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      <div className="flex items-center gap-2">
                        {detalle.esGenerico
                          ? detalle.productoNombre
                          : detalle.producto.nombre}
                        {detalle.esGenerico && (
                          <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-blue-100 text-blue-800">
                            Genérico
                          </span>
                        )}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {detalle.esGenerico
                        ? detalle.productoCodigo || "-"
                        : detalle.producto.codigo}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      <input
                        type="number"
                        min="1"
                        value={detalle.cantidad}
                        onChange={(e) =>
                          handleCantidadDetalleChange(index, e.target.value)
                        }
                        className="w-20 px-2 py-1 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                      />
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      $
                      {(
                        detalle.precio *
                        (1 + formData.porcentajeAjuste / 100)
                      ).toFixed(2)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      $
                      {(
                        detalle.subtotal *
                        (1 + formData.porcentajeAjuste / 100)
                      ).toFixed(2)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm">
                      <button
                        type="button"
                        onClick={() => handleEliminarDetalle(index)}
                        className="text-red-600 hover:text-red-900"
                      >
                        <svg
                          className="w-5 h-5"
                          fill="none"
                          stroke="currentColor"
                          viewBox="0 0 24 24"
                        >
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={2}
                            d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"
                          />
                        </svg>
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
              <tfoot className="bg-gray-50">
                <tr className="border-t-2 border-gray-300">
                  <td
                    colSpan="4"
                    className="px-6 py-4 text-right text-sm font-bold text-gray-900"
                  >
                    TOTAL:
                  </td>
                  <td
                    colSpan="2"
                    className="px-6 py-4 text-sm font-bold text-gray-900"
                  >
                    ${calcularTotal().toFixed(2)}
                  </td>
                </tr>
              </tfoot>
            </table>
          </div>
        </div>
      )}

      {/* Botones de Acción */}
      <div className="flex justify-end space-x-4">
        <button
          type="button"
          onClick={() => {
            setTipoCliente("consumidor_final");
            setFormData({
              idCliente: "",
              idTipoComprobante: "",
              idFormaPago: "",
              porcentajeAjuste: 0,
              clienteDocumento: "",
              clienteNombre: "",
              clienteApellido: "",
              clienteTelefono: "",
              clienteCorreo: "",
              clienteDireccion: "",
              detalles: [],
            });
            setClienteSeleccionado(null);
            setProductoSeleccionado(null);
            setSelectedProducto("");
            setCantidad(1);
            setPrecio(0);
            setFormaPagoSeleccionada(null);
          }}
          className="px-6 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors"
          disabled={submitting}
        >
          Limpiar
        </button>

        <button
          type="submit"
          disabled={submitting || formData.detalles.length === 0}
          className="px-6 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors"
        >
          {submitting ? "Generando..." : "Generar Comprobante"}
        </button>
      </div>


      <ComprobanteConfirmacionModal
        isOpen={showConfirmation}
        onClose={() => setShowConfirmation(false)}
        onConfirm={handleConfirmar}
        formData={formData}
        clienteSeleccionado={clienteSeleccionado}
        tiposComprobante={tiposComprobante}
        formasPago={formasPago}
        condicionesVenta={condicionesVenta}
      />
    </form>
  );
}

export default ComprobanteForm;
