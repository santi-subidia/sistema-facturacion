using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Facturacion;
using Backend.Models;
using Backend.Services.External.Afip.Interfaces;
using Backend.Services.External.Afip.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

public class ComprobantesService : IComprobantesService
{
    private readonly AppDbContext _db;
    private readonly IAfipWsfeService _afipService;
    private readonly IAfipComprobantePdfService _pdfService;
    private readonly IDetalleComprobanteService _detalleService;

    public ComprobantesService(
        AppDbContext db,
        IAfipWsfeService afipService,
        IAfipComprobantePdfService pdfService,
        IDetalleComprobanteService detalleService)
    {
        _db = db;
        _afipService = afipService;
        _pdfService = pdfService;
        _detalleService = detalleService;
    }

    private record ComprobanteError(string Message, IEnumerable<string>? Errors = null);

    private record EntidadesValidadas(
        Cliente? Cliente,
        TipoComprobante TipoComprobante,
        FormaPago FormaPago,
        int PuntoVenta,
        int? SesionCajaId);

    public async Task<(bool success, string message, Comprobante? comprobante, IEnumerable<string>? errors)> CrearComprobanteAsync(CreateComprobanteDto dto, int userId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            // 1. Validar entidades relacionadas
            var (entidades, errorValidacion) = await ValidarEntidadesAsync(dto, userId);
            if (errorValidacion != null)
                return (false, errorValidacion.Message, null, errorValidacion.Errors);

            // 2. Construir comprobante y asignar datos del cliente
            var comprobante = ConstruirComprobante(dto, entidades!.Cliente, entidades.FormaPago,
                entidades.PuntoVenta, userId, entidades.SesionCajaId);

            // 3. Procesar detalles y stock
            bool esNotaDeCredito = entidades.TipoComprobante.EsNotaDeCredito;
            decimal porcentajeAjuste = dto.PorcentajeAjuste ?? entidades.FormaPago.PorcentajeAjuste ?? 0m;
            var errorDetalles = await ProcesarDetallesAsync(comprobante, dto, esNotaDeCredito, porcentajeAjuste, userId);
            if (errorDetalles != null)
                return (false, errorDetalles.Message, null, errorDetalles.Errors);

            // 4. Resolver datos de documento del cliente para AFIP
            var (tipoDoc, numDoc, condIva) = ResolverDatosDocumentoCliente(entidades.Cliente);

            // 5. Validar comprobantes asociados (notas de crÃ©dito)
            var (cbtesAsoc, errorAsociados) = await ValidarComprobantesAsociadosAsync(comprobante, dto);
            if (errorAsociados != null)
                return (false, errorAsociados.Message, null, errorAsociados.Errors);

            // 6. Solicitar autorizaciÃ³n AFIP (Ãºltimo autorizado + CAE)
            var errorAfip = await SolicitarAutorizacionAfipAsync(
                comprobante, dto, entidades.TipoComprobante, entidades.PuntoVenta,
                tipoDoc, numDoc, condIva, cbtesAsoc);
            if (errorAfip != null)
                return (false, errorAfip.Message, null, errorAfip.Errors);

            // 7. Persistir
            _db.Comprobantes.Add(comprobante);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            // 8. Cargar comprobante completo con relaciones
            var comprobanteCompleto = await _db.Comprobantes
                .Include(f => f.Cliente)
                .Include(f => f.TipoComprobante)
                .Include(f => f.FormaPago)
                .Include(f => f.Creado_por)
                .FirstOrDefaultAsync(f => f.Id == comprobante.Id);

            // 9. Post-procesamiento: actualizar estado de factura asociada si es nota de crÃ©dito
            if (esNotaDeCredito && comprobante.IdFacturaAsociada.HasValue)
                await ActualizarEstadoFacturaAsociadaAsync(comprobante);

            return (true, "Comprobante creado y autorizado por AFIP exitosamente", comprobanteCompleto, null);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Error al crear el comprobante: {ex.Message}", null, null);
        }
    }

    private async Task<(EntidadesValidadas? entidades, ComprobanteError? error)> ValidarEntidadesAsync(CreateComprobanteDto dto, int userId)
    {
        Cliente? cliente = null;

        if (dto.IdCliente.HasValue)
        {
            cliente = await _db.Clientes.FindAsync(dto.IdCliente.Value);
            if (cliente == null || cliente.Eliminado_at != null)
                return (null, new ComprobanteError("El cliente especificado no existe o estÃ¡ eliminado."));
        }

        var tipoComprobante = await _db.TiposComprobantes.FindAsync(dto.IdTipoComprobante);
        if (tipoComprobante == null)
            return (null, new ComprobanteError("Tipo de comprobante invÃ¡lido"));

        var formaPago = await _db.FormasPago.FindAsync(dto.IdFormaPago);
        if (formaPago == null)
            return (null, new ComprobanteError("La forma de pago especificada no existe."));

        var condicionVenta = await _db.CondicionesVenta.FindAsync(dto.IdCondicionVenta);
        if (condicionVenta == null)
            return (null, new ComprobanteError("La condiciÃ³n de venta especificada no existe."));

        var configAfip = await _db.AfipConfiguraciones
            .Where(c => c.Activa)
            .FirstOrDefaultAsync();
        if (configAfip == null)
            return (null, new ComprobanteError("No hay configuraciÃ³n AFIP activa. Debe configurar AFIP antes de generar comprobantes."));

        var sesionActiva = await _db.SesionesCaja
            .Include(s => s.Caja)
            .FirstOrDefaultAsync(s => s.UsuarioId == userId && s.Estado == EstadoSesionCaja.Abierta);
        if (sesionActiva?.Caja == null)
            return (null, new ComprobanteError("Debe tener una sesiÃ³n de caja abierta para generar comprobantes."));

        return (new EntidadesValidadas(cliente, tipoComprobante, formaPago, sesionActiva.Caja.PuntoVenta, sesionActiva.Id), null);
    }

    /// <summary>
    /// Construye la entidad Comprobante y asigna datos del cliente (registrado o manual).
    /// </summary>
    private Comprobante ConstruirComprobante(CreateComprobanteDto dto, Cliente? cliente,
        FormaPago formaPago, int puntoVenta, int userId, int? sesionCajaId)
    {
        var comprobante = new Comprobante(
            dto.IdTipoComprobante,
            dto.IdFormaPago,
            dto.IdCondicionVenta,
            puntoVenta,
            userId
        )
        {
            IdCliente            = dto.IdCliente,
            Fecha                = dto.Fecha,
            PorcentajeAjuste     = dto.PorcentajeAjuste ?? formaPago.PorcentajeAjuste ?? 0m,
            CodigoConcepto       = dto.CodigoConcepto,
            FechaServicioDesde   = dto.FechaServicioDesde,
            FechaServicioHasta   = dto.FechaServicioHasta,
            FechaVencimientoPago = dto.FechaVencimientoPago,
            CodigoMoneda         = dto.CodigoMoneda,
            CotizacionMoneda     = dto.CotizacionMoneda,
            SesionCajaId         = sesionCajaId
        };

        if (cliente != null)
        {
            comprobante.ClienteDocumento = cliente.Documento;
            comprobante.ClienteNombre    = cliente.Nombre;
            comprobante.ClienteApellido  = cliente.Apellido;
            comprobante.ClienteTelefono  = cliente.Telefono;
            comprobante.ClienteCorreo    = cliente.Correo;
            comprobante.ClienteDireccion = cliente.Direccion;
        }
        else
        {
            comprobante.ClienteDocumento = dto.ClienteDocumento;
            comprobante.ClienteNombre    = dto.ClienteNombre;
            comprobante.ClienteApellido  = dto.ClienteApellido;
            comprobante.ClienteTelefono  = dto.ClienteTelefono;
            comprobante.ClienteCorreo    = dto.ClienteCorreo;
            comprobante.ClienteDireccion = dto.ClienteDireccion;
        }

        return comprobante;
    }

    /// <summary>
    /// Procesa cada lÃ­nea de detalle: valida productos, controla stock y agrega al comprobante.
    /// </summary>
    private async Task<ComprobanteError?> ProcesarDetallesAsync(Comprobante comprobante,
        CreateComprobanteDto dto, bool esNotaDeCredito, decimal porcentajeAjuste, int userId)
    {
        foreach (var detalle in dto.Detalles)
        {
            if (detalle.IdProducto.HasValue)
            {
                var producto = await _db.Productos.FindAsync(detalle.IdProducto.Value);
                if (producto == null || producto.Eliminado_at != null)
                    return new ComprobanteError($"El producto con ID {detalle.IdProducto} no existe o estÃ¡ eliminado.");

                if (!esNotaDeCredito)
                {
                    if (producto.Stock < detalle.Cantidad)
                        return new ComprobanteError($"Stock insuficiente para el producto '{producto.Nombre}'. Disponible: {producto.Stock}, Solicitado: {detalle.Cantidad}");

                    producto.Stock -= detalle.Cantidad;
                    _db.Productos.Update(producto);
                }

                comprobante.AgregarProducto(producto, detalle.Cantidad, porcentajeAjuste);
            }
            else
            {
                var productoGenerico = new Producto
                {
                    Id = 0,
                    Codigo = detalle.ProductoCodigo ?? "-",
                    Nombre = detalle.ProductoNombre ?? "Producto sin nombre",
                    Precio = detalle.PrecioUnitario,
                    Stock = 0,
                    IdCreado_por = userId
                };

                comprobante.AgregarProducto(productoGenerico, detalle.Cantidad, porcentajeAjuste);
            }
        }

        return null;
    }

    /// <summary>
    /// Resuelve tipo de documento, nÃºmero de documento y condiciÃ³n IVA del cliente para AFIP.
    /// </summary>
    private (int tipoDocumento, long numeroDocumento, int condicionIva) ResolverDatosDocumentoCliente(Cliente? cliente)
    {
        int tipoDocumento = AfipConstants.TipoDocumentoConsumidorFinal;
        long numeroDocumento = 0;
        int condicionIva = AfipConstants.CondicionIvaConsumidorFinal;

        if (cliente != null)
        {
            tipoDocumento = cliente.IdAfipTipoDocumento;

            var doc = cliente.Documento.Replace("-", "");
            if (!long.TryParse(doc, out numeroDocumento))
                numeroDocumento = 0;

            condicionIva = cliente.IdAfipCondicionIva;
        }

        return (tipoDocumento, numeroDocumento, condicionIva);
    }

    /// <summary>
    /// Valida comprobantes asociados (para notas de crÃ©dito): verifica existencia y CAE.
    /// </summary>
    private async Task<(List<CbteAsoc>? cbtesAsoc, ComprobanteError? error)> ValidarComprobantesAsociadosAsync(
        Comprobante comprobante, CreateComprobanteDto dto)
    {
        if (dto.ComprobantesAsociados == null || dto.ComprobantesAsociados.Count == 0)
            return (null, null);

        var cbtesAsoc = new List<CbteAsoc>();

        foreach (var cbteAsoc in dto.ComprobantesAsociados)
        {
            var comprobanteAsociado = await _db.Comprobantes
                .Include(f => f.TipoComprobante)
                .FirstOrDefaultAsync(f =>
                    f.PuntoVenta == cbteAsoc.PtoVta &&
                    f.NumeroComprobante == cbteAsoc.Nro &&
                    f.TipoComprobante!.CodigoAfip == cbteAsoc.Tipo &&
                    f.Eliminado_at == null);

            if (comprobanteAsociado == null)
                return (null, new ComprobanteError(
                    $"El comprobante asociado (Tipo: {cbteAsoc.Tipo}, PtoVta: {cbteAsoc.PtoVta}, Nro: {cbteAsoc.Nro}) no existe en el sistema."));

            if (string.IsNullOrEmpty(comprobanteAsociado.CAE))
                return (null, new ComprobanteError(
                    $"El comprobante asociado (Tipo: {cbteAsoc.Tipo}, PtoVta: {cbteAsoc.PtoVta}, Nro: {cbteAsoc.Nro}) no tiene CAE autorizado por AFIP."));

            cbtesAsoc.Add(new CbteAsoc
            {
                Tipo   = cbteAsoc.Tipo,
                PtoVta = cbteAsoc.PtoVta,
                Nro    = cbteAsoc.Nro,
                Cuit   = cbteAsoc.Cuit
            });

            if (comprobante.IdFacturaAsociada == null)
                comprobante.IdFacturaAsociada = comprobanteAsociado.Id;
        }

        return (cbtesAsoc, null);
    }

    /// <summary>
    /// Consulta el Ãºltimo comprobante autorizado, construye la solicitud CAE y la envÃ­a a AFIP.
    /// </summary>
    private async Task<ComprobanteError?> SolicitarAutorizacionAfipAsync(
        Comprobante comprobante, CreateComprobanteDto dto,
        TipoComprobante tipoComprobante, int puntoVenta,
        int idAfipTipoDocumento, long numeroDocumento, int idAfipCondicionIva,
        List<CbteAsoc>? cbtesAsoc)
    {
        // Consultar Ãºltimo comprobante autorizado
        FECompUltimoAutorizadoResponse ultimoComprobante;
        try
        {
            ultimoComprobante = await _afipService.FECompUltimoAutorizadoAsync(puntoVenta, tipoComprobante.CodigoAfip);

            if (ultimoComprobante.Errors != null && ultimoComprobante.Errors.Count > 0)
                return new ComprobanteError("Error al consultar Ãºltimo comprobante en AFIP",
                    ultimoComprobante.Errors.Select(e => e.Msg ?? e.Code.ToString() ?? "Error desconocido"));
        }
        catch (Exception ex)
        {
            return new ComprobanteError("Error de conexiÃ³n con AFIP al consultar Ãºltimo comprobante", new[] { ex.Message });
        }

        long numeroComprobante = ultimoComprobante.CbteNro + 1;

        // Construir solicitud CAE
        var caeSolicitud = new FECAERequest
        {
            PtoVta   = puntoVenta,
            CbteTipo = tipoComprobante.CodigoAfip,
            FeDetReq = new List<FECAEDetRequest>
            {
                new FECAEDetRequest
                {
                    Concepto  = dto.CodigoConcepto,
                    DocTipo   = idAfipTipoDocumento,
                    DocNro    = numeroDocumento,
                    CbteDesde = numeroComprobante,
                    CbteHasta = numeroComprobante,
                    CbteFch   = dto.Fecha.ToString("yyyyMMdd"),

                    ImpTotal   = comprobante.Total,
                    ImpTotConc = 0,
                    ImpNeto    = comprobante.Total,
                    ImpOpEx    = 0,
                    ImpTrib    = 0,
                    ImpIVA     = 0,

                    FchServDesde = dto.FechaServicioDesde?.ToString("yyyyMMdd") ?? string.Empty,
                    FchServHasta = dto.FechaServicioHasta?.ToString("yyyyMMdd") ?? string.Empty,
                    FchVtoPago   = dto.FechaVencimientoPago?.ToString("yyyyMMdd") ?? string.Empty,

                    MonId    = dto.CodigoMoneda,
                    MonCotiz = dto.CotizacionMoneda,

                    CondicionIVAReceptorId = idAfipCondicionIva,
                    Iva        = null,
                    CbtesAsoc  = cbtesAsoc
                }
            }
        };

        // Solicitar CAE
        FECAEResponse caeResponse;
        try
        {
            caeResponse = await _afipService.FECAESolicitarAsync(caeSolicitud);

            if (caeResponse.Errors != null && caeResponse.Errors.Count > 0)
                return new ComprobanteError("AFIP rechazÃ³ el comprobante",
                    caeResponse.Errors.Select(e => e.Msg ?? e.Code.ToString() ?? "Error desconocido"));

            if (caeResponse.FeCabResp?.Resultado != AfipConstants.ResultadoAprobado)
            {
                var observaciones = caeResponse.FeDetResp?.FirstOrDefault()?.Observaciones;
                var errors = observaciones != null
                    ? observaciones.Select(o => o.Msg ?? o.Code.ToString() ?? "Error desconocido")
                    : Array.Empty<string>();
                return new ComprobanteError($"AFIP no aprobÃ³ el comprobante. Resultado: {caeResponse.FeCabResp?.Resultado}", errors);
            }
        }
        catch (Exception ex)
        {
            return new ComprobanteError("Error de conexiÃ³n con AFIP al solicitar CAE", new[] { ex.Message });
        }

        // Confirmar CAE en el comprobante
        var detResponse = caeResponse.FeDetResp?.FirstOrDefault();
        if (detResponse == null || string.IsNullOrEmpty(detResponse.CAE))
            return new ComprobanteError("AFIP no devolviÃ³ un CAE vÃ¡lido");

        comprobante.ConfirmarCAE(
            detResponse.CAE,
            DateTime.ParseExact(detResponse.CAEFchVto!, "yyyyMMdd", CultureInfo.InvariantCulture),
            numeroComprobante,
            puntoVenta
        );

        return null;
    }

    /// <summary>
    /// Actualiza el estado de la factura original cuando se emite una nota de crÃ©dito.
    /// </summary>
    private async Task ActualizarEstadoFacturaAsociadaAsync(Comprobante comprobante)
    {
        var facturaOriginal = await _db.Comprobantes
            .Include(f => f.Detalles)
            .FirstOrDefaultAsync(f => f.Id == comprobante.IdFacturaAsociada!.Value);

        if (facturaOriginal == null) return;

        // Obtener notas de crÃ©dito previas contra la misma factura
        var notasPrevias = await _db.Comprobantes
            .Include(f => f.Detalles)
            .Where(f => f.IdFacturaAsociada == facturaOriginal.Id
                        && f.Id != comprobante.Id
                        && f.Eliminado_at == null)
            .ToListAsync();

        // Calcular cantidades originales
        var cantidadesOriginales = new Dictionary<string, decimal>();
        foreach (var det in facturaOriginal.Detalles)
        {
            string key = det.IdProducto?.ToString() ?? det.ProductoNombre;
            if (cantidadesOriginales.ContainsKey(key))
                cantidadesOriginales[key] += det.Cantidad;
            else
                cantidadesOriginales[key] = det.Cantidad;
        }

        // Descontar cantidades de esta nota de crÃ©dito
        foreach (var det in comprobante.Detalles)
        {
            string key = det.IdProducto?.ToString() ?? det.ProductoNombre;
            if (cantidadesOriginales.ContainsKey(key))
                cantidadesOriginales[key] -= det.Cantidad;
        }

        // Descontar cantidades de notas previas
        foreach (var nota in notasPrevias)
        {
            foreach (var det in nota.Detalles)
            {
                string key = det.IdProducto?.ToString() ?? det.ProductoNombre;
                if (cantidadesOriginales.ContainsKey(key))
                    cantidadesOriginales[key] -= det.Cantidad;
            }
        }

        // Determinar estado
        bool completamenteAnulada = cantidadesOriginales.Values.All(v => v <= 0);

        var estadoAnulada = await _db.EstadosComprobantes.FirstOrDefaultAsync(e => e.Nombre == EstadoComprobanteNombres.Anulada);
        var estadoParcial = await _db.EstadosComprobantes.FirstOrDefaultAsync(e => e.Nombre == EstadoComprobanteNombres.ParcialmenteAnulada);

        facturaOriginal.IdEstadoComprobante = completamenteAnulada
            ? estadoAnulada?.Id
            : estadoParcial?.Id;

        _db.Comprobantes.Update(facturaOriginal);
        await _db.SaveChangesAsync();
    }


    public async Task<(IEnumerable<Comprobante> data, int totalItems, int totalPages)> GetAllAsync(ComprobanteFiltroDto filtro)
    {
        if (filtro.Page < 1) filtro.Page = 1;
        if (filtro.PageSize < 1 || filtro.PageSize > 100) filtro.PageSize = 10;

        var query = _db.Comprobantes
            .Include(f => f.Cliente)
            .Include(f => f.TipoComprobante)
            .Include(f => f.EstadoComprobante)
            .Include(f => f.FormaPago)
            .Include(f => f.Creado_por)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filtro.Buscar))
        {
            var b = filtro.Buscar.ToLower();
            query = query.Where(f => 
                (f.ClienteNombre != null && f.ClienteNombre.ToLower().Contains(b)) ||
                (f.ClienteApellido != null && f.ClienteApellido.ToLower().Contains(b)) ||
                (f.ClienteDocumento != null && f.ClienteDocumento.ToLower().Contains(b)) ||
                (f.CAE != null && f.CAE.ToLower().Contains(b)) ||
                (f.NumeroComprobante.HasValue && f.NumeroComprobante.Value.ToString().Contains(b))
            );
        }

        if (!string.IsNullOrEmpty(filtro.ClienteDocumentoNombre))
        {
            var c = filtro.ClienteDocumentoNombre.ToLower();
            query = query.Where(f => 
                (f.ClienteNombre != null && f.ClienteNombre.ToLower().Contains(c)) ||
                (f.ClienteApellido != null && f.ClienteApellido.ToLower().Contains(c)) ||
                (f.ClienteDocumento != null && f.ClienteDocumento.ToLower().Contains(c))
            );
        }

        if (!string.IsNullOrEmpty(filtro.NumeroComprobante))
        {
            var n = filtro.NumeroComprobante.ToLower();
            query = query.Where(f => 
                (f.NumeroComprobante.HasValue && f.NumeroComprobante.Value.ToString().Contains(n)) ||
                (f.CAE != null && f.CAE.ToLower().Contains(n))
            );
        }

        if (filtro.FechaDesde.HasValue)
            query = query.Where(f => f.Fecha >= filtro.FechaDesde.Value);

        if (filtro.FechaHasta.HasValue)
            query = query.Where(f => f.Fecha <= filtro.FechaHasta.Value);

        if (filtro.IdCliente.HasValue)
            query = query.Where(f => f.IdCliente == filtro.IdCliente.Value);

        if (filtro.IdTipoComprobante.HasValue)
            query = query.Where(f => f.IdTipoComprobante == filtro.IdTipoComprobante.Value);

        if (filtro.IdEstadoComprobante.HasValue)
            query = query.Where(f => f.IdEstadoComprobante == filtro.IdEstadoComprobante.Value);

        if (filtro.IdFormaPago.HasValue)
            query = query.Where(f => f.IdFormaPago == filtro.IdFormaPago.Value);

        if (filtro.IdCondicionVenta.HasValue)
            query = query.Where(f => f.IdCondicionVenta == filtro.IdCondicionVenta.Value);

        if (filtro.TotalDesde.HasValue)
            query = query.Where(f => f.Total >= filtro.TotalDesde.Value);

        if (filtro.TotalHasta.HasValue)
            query = query.Where(f => f.Total <= filtro.TotalHasta.Value);

        query = query.OrderByDescending(f => f.Fecha);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)filtro.PageSize);

        var comprobantes = await query
            .Skip((filtro.Page - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .ToListAsync();

        return (comprobantes, totalItems, totalPages);
    }

    public async Task<Comprobante?> GetByIdAsync(int id)
    {
        return await _db.Comprobantes
            .Include(f => f.Cliente)
            .Include(f => f.TipoComprobante)
            .Include(f => f.EstadoComprobante)
            .Include(f => f.FormaPago)
            .Include(f => f.Creado_por)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<(Comprobante? comprobante, List<DetalleComprobante> details)> GetWithDetailsAsync(int id)
    {
        var comprobante = await GetByIdAsync(id);

        if (comprobante == null)
            return (null, new List<DetalleComprobante>());

        var detalles = await _db.DetallesComprobante
            .Include(d => d.Producto)
            .Where(d => d.IdComprobante == id)
            .ToListAsync();

        return (comprobante, detalles);
    }

    public async Task<(Comprobante? comprobante, List<DetalleSaldoDto> saldos)> GetSaldosComprobanteAsync(int id)
    {
        var comprobante = await GetByIdAsync(id);

        if (comprobante == null)
            return (null, new List<DetalleSaldoDto>());

        var detallesOriginales = await _db.DetallesComprobante
            .Include(d => d.Producto)
            .Where(d => d.IdComprobante == id)
            .ToListAsync();

        var notasCreditoPrevias = await _db.Comprobantes
            .Include(f => f.Detalles)
            .Where(f => f.IdFacturaAsociada == id && f.Eliminado_at == null)
            .ToListAsync();

        var saldos = new List<DetalleSaldoDto>();

        foreach (var detOriginal in detallesOriginales)
        {
            decimal cantidadRemanente = detOriginal.Cantidad;

            foreach (var nc in notasCreditoPrevias)
            {
                var detNC = nc.Detalles.FirstOrDefault(d => 
                    d.IdProducto == detOriginal.IdProducto && d.ProductoNombre == detOriginal.ProductoNombre);

                if (detNC != null)
                {
                    cantidadRemanente -= detNC.Cantidad;
                }
            }

            if (cantidadRemanente > 0)
            {
                saldos.Add(new DetalleSaldoDto
                {
                    Id = detOriginal.Id,
                    IdComprobante = detOriginal.IdComprobante,
                    IdProducto = detOriginal.IdProducto,
                    ProductoNombre = detOriginal.ProductoNombre,
                    ProductoCodigo = detOriginal.ProductoCodigo,
                    PrecioUnitario = detOriginal.PrecioUnitario,
                    Cantidad = cantidadRemanente
                });
            }
        }

        return (comprobante, saldos);
    }

    public async Task<(bool success, string message, Comprobante? comprobante, IEnumerable<string>? errors)> CreateSimpleAsync(Comprobante comprobante, int userId)
    {
        if (comprobante.IdCliente.HasValue)
        {
            var cliente = await _db.Clientes.FindAsync(comprobante.IdCliente.Value);
            if (cliente == null)
                return (false, "El cliente especificado no existe.", null, null);
            
            comprobante.ClienteDocumento = cliente.Documento;
            comprobante.ClienteNombre = cliente.Nombre;
            comprobante.ClienteApellido = cliente.Apellido;
            comprobante.ClienteTelefono = cliente.Telefono;
            comprobante.ClienteCorreo = cliente.Correo;
            comprobante.ClienteDireccion = cliente.Direccion;
        }

        if (!await _db.TiposComprobantes.AnyAsync(t => t.Id == comprobante.IdTipoComprobante))
            return (false, "El tipo de comprobante especificado no existe.", null, null);

        if (!await _db.FormasPago.AnyAsync(fp => fp.Id == comprobante.IdFormaPago))
            return (false, "La forma de pago especificada no existe.", null, null);

        if (comprobante.Creado_at == default)
            comprobante.Creado_at = DateTime.UtcNow;

        comprobante.IdCreado_por = userId;

        var sesionActiva = await _db.SesionesCaja
            .FirstOrDefaultAsync(s => s.UsuarioId == userId && s.Estado == EstadoSesionCaja.Abierta);
        comprobante.SesionCajaId = sesionActiva?.Id;

        try
        {
            _db.Comprobantes.Add(comprobante);
            await _db.SaveChangesAsync();
            return (true, "Comprobante creado exitosamente.", comprobante, null);
        }
        catch (Exception ex)
        {
            return (false, $"Error al crear el comprobante: {ex.Message}", null, null);
        }
    }

    public async Task<(bool success, string message, IEnumerable<string>? errors)> UpdateAsync(int id, Comprobante comprobante)
    {
        if (id != comprobante.Id)
            return (false, "ID del comprobante no coincide.", null);

        var existingComprobante = await _db.Comprobantes.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
        if (existingComprobante == null)
            return (false, "Comprobante no encontrado.", null);

        if (comprobante.IdCliente.HasValue && !await _db.Clientes.AnyAsync(c => c.Id == comprobante.IdCliente))
            return (false, "El cliente especificado no existe.", null);

        if (!await _db.TiposComprobantes.AnyAsync(t => t.Id == comprobante.IdTipoComprobante))
            return (false, "El tipo de comprobante especificado no existe.", null);

        if (!await _db.FormasPago.AnyAsync(fp => fp.Id == comprobante.IdFormaPago))
            return (false, "La forma de pago especificada no existe.", null);

        _db.Entry(comprobante).State = EntityState.Modified;
        
        _db.Entry(comprobante).Property(f => f.Creado_at).IsModified = false;
        _db.Entry(comprobante).Property(f => f.IdCreado_por).IsModified = false;

        try
        {
            await _db.SaveChangesAsync();
            return (true, "Comprobante actualizado exitosamente.", null);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _db.Comprobantes.AnyAsync(f => f.Id == id))
                return (false, "Comprobante no encontrado.", null);
            else
                throw;
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar el comprobante: {ex.Message}", null);
        }
    }

    public async Task<(bool success, string message)> DeleteAsync(int id)
    {
        var comprobante = await _db.Comprobantes.FindAsync(id);
        if (comprobante == null)
            return (false, "Comprobante no encontrado.");

        comprobante.Eliminado_at = DateTime.UtcNow;
        
        _db.Entry(comprobante).State = EntityState.Modified;

        try
        {
            await _db.SaveChangesAsync();
            return (true, "Comprobante eliminado exitosamente.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al eliminar el comprobante: {ex.Message}");
        }
    }

    public async Task<(bool success, string message, byte[]? fileBytes, string? fileName)> GenerarPdfAsync(int id)
    {
        try
        {
            var comprobante = await _db.Comprobantes.FindAsync(id);
            if (comprobante == null)
                return (false, "Comprobante no encontrado", null, null);

            var pdfBytes = await _pdfService.GenerarPdfComprobanteAsync(id);
            
            var fileName = $"Comprobante_{comprobante.PuntoVenta:D5}_{comprobante.NumeroComprobante:D8}.pdf";
            
            return (true, "PDF generado exitosamente", pdfBytes, fileName);
        }
        catch (Exception ex)
        {
            return (false, $"Error al generar PDF: {ex.Message}", null, null);
        }
    }
}