using Backend.DTOs.CondicionVenta;
using FluentValidation;

namespace Backend.Validators.CondicionVenta
{
    public class CondicionVentaValidator : AbstractValidator<CondicionVentaCreateUpdateDto>
    {
        public CondicionVentaValidator()
        {
            RuleFor(x => x.Descripcion)
                .NotEmpty().WithMessage("La descripción es obligatoria.")
                .MaximumLength(50).WithMessage("La descripción no puede exceder 50 caracteres.");

            RuleFor(x => x.DiasVencimiento)
                .GreaterThanOrEqualTo(0).WithMessage("Los días de vencimiento deben ser mayor o igual a 0.");
        }
    }
}
