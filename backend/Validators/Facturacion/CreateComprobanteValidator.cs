using FluentValidation;
using Backend.DTOs.Facturacion;

namespace Backend.Validators.Facturacion;

public class CreateComprobanteValidator : AbstractValidator<CreateComprobanteDto>
{
    public CreateComprobanteValidator()
    {
        RuleFor(x => x.IdCliente).GreaterThan(0).WithMessage("El cliente es obligatorio.");
        RuleFor(x => x.IdTipoComprobante).GreaterThan(0).WithMessage("El tipo de comprobante es obligatorio.");
        RuleFor(x => x.Detalles).NotEmpty().WithMessage("El comprobante debe tener al menos un detalle.");
        RuleForEach(x => x.Detalles).SetValidator(new DetalleComprobanteValidator());

        // Assuming these rules are meant to be part of the new validator
        RuleFor(x => x.Fecha) // Assuming 'Fecha' property exists in CreateComprobanteDto
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("La fecha no puede ser futura (con mÃ¡s de 1 dÃ­a de margen).")
            .GreaterThan(DateTime.UtcNow.AddYears(-1)).WithMessage("La fecha es demasiado antigua.");

        RuleFor(x => x.CodigoConcepto) // Assuming 'CodigoConcepto' property exists
            .InclusiveBetween(1, 3).WithMessage("El cÃ³digo de concepto debe ser 1 (Productos), 2 (Servicios) o 3 (Productos y Servicios).");
        When(x => x.CodigoConcepto >= 2, () =>
        {
            RuleFor(x => x.FechaServicioDesde)
                .NotNull().WithMessage("La fecha de inicio de servicio es requerida para servicios.");
            RuleFor(x => x.FechaServicioHasta)
                .NotNull().WithMessage("La fecha de fin de servicio es requerida para servicios.")
                .GreaterThanOrEqualTo(x => x.FechaServicioDesde).WithMessage("La fecha de fin de servicio debe ser mayor o igual a la de inicio.");
            RuleFor(x => x.FechaVencimientoPago)
                .NotNull().WithMessage("La fecha de vencimiento de pago es requerida para servicios.");
        });

        When(x => x.IdCliente.HasValue, () =>
        {
            RuleFor(x => x.IdCliente).GreaterThan(0);
        });

        // Comprobantes asociados (notas de crÃ©dito)
        When(x => x.ComprobantesAsociados != null && x.ComprobantesAsociados.Count > 0, () =>
        {
            RuleForEach(x => x.ComprobantesAsociados).SetValidator(new ComprobanteAsociadoValidator());
        });

        RuleFor(x => x.Detalles)
            .NotEmpty().WithMessage("La factura debe tener al menos un producto.")
            .Must(d => d.Count <= 50).WithMessage("No se pueden facturar mÃ¡s de 50 Ã­tems por comprobante (limitaciÃ³n AFIP/Performance).");


    }
}

public class ComprobanteAsociadoValidator : AbstractValidator<ComprobanteAsociadoDto>
{
    public ComprobanteAsociadoValidator()
    {
        RuleFor(x => x.Tipo)
            .GreaterThan(0).WithMessage("El tipo de comprobante asociado debe ser mayor a 0.");
        RuleFor(x => x.PtoVta)
            .GreaterThan(0).WithMessage("El punto de venta del comprobante asociado debe ser mayor a 0.");
        RuleFor(x => x.Nro)
            .GreaterThan(0).WithMessage("El nÃºmero del comprobante asociado debe ser mayor a 0.");
    }
}
