using FluentValidation;
using Backend.DTOs.Productos;

namespace Backend.Validators.Productos;

public class ProductoValidator : AbstractValidator<ProductoCreateUpdateDto>
{
    public ProductoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres.");

        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("El código es obligatorio.")
            .MaximumLength(50).WithMessage("El código no puede exceder 50 caracteres.");

        RuleFor(x => x.Precio)
            .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo.");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo.");

        RuleFor(x => x.Proveedor)
            .MaximumLength(100).WithMessage("El proveedor no puede exceder 100 caracteres.");

        RuleFor(x => x.StockNegro)
            .GreaterThanOrEqualTo(0).WithMessage("El stock negro no puede ser negativo.")
            .When(x => x.StockNegro.HasValue);
    }
}