using FluentValidation;
using Backend.DTOs.Presupuesto;

namespace Backend.Validators.Presupuesto;

public class PresupuestoConDetallesValidator : AbstractValidator<PresupuestoConDetallesDto>
{
    public PresupuestoConDetallesValidator()
    {
        RuleFor(x => x.IdFormaPago)
            .GreaterThan(0).WithMessage("Debe seleccionar una forma de pago.");

        RuleFor(x => x.IdCondicionVenta)
            .GreaterThan(0).WithMessage("Debe seleccionar una condiciÃ³n de venta.");

        RuleFor(x => x.Fecha)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("La fecha no puede ser futura (con mÃ¡s de 1 dÃ­a de margen).")
            .GreaterThan(DateTime.UtcNow.AddYears(-1)).WithMessage("La fecha es demasiado antigua.");

        When(x => x.IdCliente.HasValue, () => 
        {
            RuleFor(x => x.IdCliente).GreaterThan(0);
        });

        When(x => x.FechaVencimiento.HasValue, () =>
        {
            RuleFor(x => x.FechaVencimiento)
                .GreaterThanOrEqualTo(x => x.Fecha).WithMessage("La fecha de vencimiento debe ser posterior a la fecha del presupuesto.");
        });

        RuleFor(x => x.Detalles)
            .NotEmpty().WithMessage("El presupuesto debe tener al menos un producto.")
            .Must(d => d.Count <= 50).WithMessage("No se pueden incluir mÃ¡s de 50 Ã­tems por presupuesto.");

        RuleForEach(x => x.Detalles).SetValidator(new DetallePresupuestoValidator());
    }
}
