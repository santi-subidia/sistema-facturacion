using FluentValidation;
using Backend.DTOs.Facturacion;

namespace Backend.Validators.Facturacion;

public class DetalleComprobanteValidator : AbstractValidator<DetalleComprobanteDto>
{
    public DetalleComprobanteValidator()
    {
        RuleFor(x => x.IdProducto).GreaterThan(0).WithMessage("El producto es obligatorio.");
        RuleFor(x => x.Cantidad).GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.");
        RuleFor(x => x.PrecioUnitario).GreaterThanOrEqualTo(0).WithMessage("El precio unitario no puede ser negativo.");

        When(x => !x.IdProducto.HasValue, () =>
        {
            RuleFor(x => x.ProductoNombre)
                .NotEmpty().WithMessage("Si el producto es genérico, debe especificar un nombre.")
                .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");
        });
        When(x => x.IdProducto.HasValue, () =>
        {
            RuleFor(x => x.IdProducto)
                .GreaterThan(0).WithMessage("El ID del producto no es válido.");
        });
    }
}