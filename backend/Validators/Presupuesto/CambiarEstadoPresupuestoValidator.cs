using FluentValidation;
using Backend.DTOs.Presupuesto;

namespace Backend.Validators.Presupuesto;

public class CambiarEstadoPresupuestoValidator : AbstractValidator<CambiarEstadoPresupuestoDto>
{
    public CambiarEstadoPresupuestoValidator()
    {
        RuleFor(x => x.NuevoEstado)
            .GreaterThan(0).WithMessage("El estado especificado no es válido.");
    }
}
