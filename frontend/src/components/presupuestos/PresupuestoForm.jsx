import React, { useState, useEffect } from "react";
import Autocomplete from "../shared/Autocomplete";
import { API_BASE_URL } from "../../config";
import PresupuestoConfirmacionModal from "./PresupuestoConfirmacionModal";

function PresupuestoForm({
  clientes,
  productos,
  formasPago,
  condicionesVenta,
  onSubmit,
  submitting,
  showNotification,
}) {
  const [tipoCliente, setTipoCliente] = useState("consumidor_final");
  const [mostrarModal, setMostrarModal] = useState(false);
  const [esVentaEnNegro, setEsVentaEnNegro] = useState(false);

  const [formData, setFormData] = useState({
    idCliente: "",
    idFormaPago: "",
    idCondicionVenta: "",
    porcentajeAjuste: 0,
    fecha: new Date().toISOString().split('T')[0],
    fechaVencimiento: "",
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
  const [tipoProducto, setTipoProducto] = useState("catalogo");
  const [productoGenerico, setProductoGenerico] = useState({
    nombre: "",
    codigo: "",
    cantidad: 1,
    precio: 0,
  });

  // Establecer valores por defecto cuando se cargan los datos
  useEffect(() => {
    if (
      formasPago.length > 0 &&
      condicionesVenta.length > 0
    ) {
      const tarjeta = formasPago.find((fp) =>
        fp.nombre?.toLowerCase().includes("tarjeta"),
      );
      const contado = condicionesVenta.find((cv) =>
        cv.nombre?.toLowerCase().includes("contado"),
      );

      if (
        !formData.idFormaPago &&
        !formData.idCondicionVenta
      ) {
        const nuevosValores = {
          ...formData,
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
  }, [formasPago, condicionesVenta]);

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
    } else if (tipo === "consumidor_con_datos") {
      setFormData({
        ...formData,
        idCliente: "",
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

      setProductoSeleccionado(null);
      setSelectedProducto("");
      setCantidad(1);
      setPrecio(0);
    } else {
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

    // Mostrar modal de confirmación
    setMostrarModal(true);
  };

  const handleConfirmarPresupuesto = () => {
    // Preparar datos para enviar
    const presupuestoData = {
      idCliente: tipoCliente === "cliente_habitual" ? formData.idCliente : null,
      idFormaPago: formData.idFormaPago,
      idCondicionVenta: formData.idCondicionVenta,
      porcentajeAjuste: formData.porcentajeAjuste,
      fecha: formData.fecha ? new Date(formData.fecha).toISOString() : new Date().toISOString(),
      fechaVencimiento: formData.fechaVencimiento ? new Date(formData.fechaVencimiento).toISOString() : null,
      clienteDocumento:
        tipoCliente === "consumidor_con_datos"
          ? formData.clienteDocumento || null
          : null,
      clienteNombre:
        tipoCliente === "consumidor_con_datos"
          ? formData.clienteNombre || null
          : null,
      clienteApellido:
        tipoCliente === "consumidor_con_datos"
          ? formData.clienteApellido || null
          : null,
      clienteTelefono:
        tipoCliente === "consumidor_con_datos"
          ? formData.clienteTelefono || null
          : null,
      clienteCorreo:
        tipoCliente === "consumidor_con_datos"
          ? formData.clienteCorreo || null
          : null,
      clienteDireccion:
        tipoCliente === "consumidor_con_datos"
          ? formData.clienteDireccion || null
          : null,
      detalles: formData.detalles.map((d) => ({
        idProducto: d.esGenerico ? null : d.idProducto,
        productoNombre: d.esGenerico ? d.productoNombre : null,
        productoCodigo: d.esGenerico ? d.productoCodigo : null,
        cantidad: d.cantidad,
        precio: d.precio,
      })),
      esVentaEnNegro: esVentaEnNegro
    };

    // Cerrar modal
    setMostrarModal(false);

    // Enviar datos
    onSubmit(presupuestoData);

    // Limpiar formulario después de envío
    setTipoCliente("consumidor_final");
    setTipoProducto("catalogo");
    setFormData({
      idCliente: "",
      idFormaPago: "",
      idCondicionVenta: "",
      porcentajeAjuste: 0,
      fecha: new Date().toISOString().split('T')[0],
      fechaVencimiento: "",
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
    setEsVentaEnNegro(false);
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
                value="consumidor_con_datos"
                checked={tipoCliente === "consumidor_con_datos"}
                onChange={(e) => handleTipoClienteChange(e.target.value)}
                className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300"
                disabled={submitting}
              />
              <span className="ml-2 text-sm text-gray-700">
                Consumidor Final con Datos
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

          {tipoCliente === "consumidor_con_datos" && (
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

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Fecha *
            </label>
            <input
              type="date"
              value={formData.fecha}
              onChange={(e) =>
                setFormData({ ...formData, fecha: e.target.value })
              }
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              required
              disabled={submitting}
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Fecha de Vencimiento
            </label>
            <input
              type="date"
              value={formData.fechaVencimiento}
              onChange={(e) =>
                setFormData({ ...formData, fechaVencimiento: e.target.value })
              }
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
              disabled={submitting}
            />
          </div>

          <div>
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

          <div>
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
            <div className="md:col-span-2">
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
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div className="md:col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Producto
              </label>
              <Autocomplete
                placeholder="Buscar producto por nombre o código..."
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
                      Código: {producto.codigo} | Stock: {producto.stock} |
                      Precio: ${producto.precio}
                    </div>
                  </div>
                )}
                disabled={submitting}
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Cantidad
              </label>
              <input
                type="number"
                min="1"
                value={cantidad}
                onChange={(e) => setCantidad(parseInt(e.target.value) || 1)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                disabled={submitting}
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Precio
              </label>
              <input
                type="number"
                step="0.01"
                min="0"
                value={precio}
                onChange={(e) => setPrecio(parseFloat(e.target.value) || 0)}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                disabled={submitting}
              />
            </div>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Nombre *
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
                placeholder="Nombre del producto"
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                disabled={submitting}
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Código
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
                placeholder="Código opcional"
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                disabled={submitting}
              />
            </div>

            <div>
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
                    cantidad: parseInt(e.target.value) || 1,
                  })
                }
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                disabled={submitting}
              />
            </div>

            <div>
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
                    precio: parseFloat(e.target.value) || 0,
                  })
                }
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                disabled={submitting}
              />
            </div>
          </div>
        )}

        <button
          type="button"
          onClick={handleAgregarProducto}
          className="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-green-500"
          disabled={submitting}
        >
          Agregar Producto
        </button>
      </div>

      {/* Lista de Productos */}
      {formData.detalles.length > 0 && (
        <div className="bg-white border border-gray-200 rounded-lg overflow-hidden">
          <div className="px-4 py-3 bg-gray-50 border-b border-gray-200">
            <h3 className="text-lg font-medium text-gray-900">
              Productos Agregados
            </h3>
          </div>
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Producto
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Código
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Cantidad
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Precio Unit.
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Subtotal
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Acciones
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {formData.detalles.map((detalle, index) => (
                  <tr key={index}>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {detalle.productoNombre}
                      {detalle.esGenerico && (
                        <span className="ml-2 text-xs text-gray-500 italic">
                          (Genérico)
                        </span>
                      )}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {detalle.productoCodigo || "-"}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      <input
                        type="number"
                        min="1"
                        value={detalle.cantidad}
                        onChange={(e) =>
                          handleCantidadDetalleChange(index, e.target.value)
                        }
                        className="w-20 px-2 py-1 border border-gray-300 rounded-md"
                        disabled={submitting}
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
                        disabled={submitting}
                      >
                        Eliminar
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

      {/* Botón de Envío */}
      <div className="flex justify-end">
        <button
          type="submit"
          disabled={submitting || formData.detalles.length === 0}
          className="px-6 py-3 bg-blue-600 text-white font-medium rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-400 disabled:cursor-not-allowed"
        >
          {submitting ? "Creando..." : "Crear Presupuesto"}
        </button>
      </div>

      {/* Modal de Confirmación */}
      <PresupuestoConfirmacionModal
        isOpen={mostrarModal}
        onClose={() => setMostrarModal(false)}
        onConfirm={handleConfirmarPresupuesto}
        formData={formData}
        clienteSeleccionado={clienteSeleccionado}
        tipoCliente={tipoCliente}
        formasPago={formasPago}
        condicionesVenta={condicionesVenta}
        esVentaEnNegro={esVentaEnNegro}
        setEsVentaEnNegro={setEsVentaEnNegro}
      />
    </form>
  );
}

export default PresupuestoForm;
