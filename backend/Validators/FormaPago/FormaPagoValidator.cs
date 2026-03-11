using FluentValidation;
using Backend.DTOs.FormaPago;

namespace Backend.Validators.FormaPago
{
    public class FormaPagoValidator : AbstractValidator<FormaPagoCreateUpdateDto>
    {
        public FormaPagoValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MaximumLength(50).WithMessage("El nombre no puede exceder 50 caracteres.");

            RuleFor(x => x.PorcentajeAjuste)
                .GreaterThanOrEqualTo(0).When(x => x.PorcentajeAjuste.HasValue)
                .WithMessage("El porcentaje de ajuste no puede ser negativo.");
        }
    }
}
