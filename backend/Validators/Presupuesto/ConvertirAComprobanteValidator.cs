using FluentValidation;
using Backend.DTOs.Presupuesto;

namespace Backend.Validators.Presupuesto;

public class ConvertirAComprobanteValidator : AbstractValidator<ConvertirAComprobanteDto>
{
    public ConvertirAComprobanteValidator()
    {
        RuleFor(x => x.IdTipoComprobante).GreaterThan(0).WithMessage("El tipo de comprobante es obligatorio.");
    }
}
