using FluentValidation;
using Backend.DTOs.Presupuesto;

namespace Backend.Validators.Presupuesto;

public class DetallePresupuestoValidator : AbstractValidator<DetallePresupuestoDto>
{
    public DetallePresupuestoValidator()
    {
        RuleFor(x => x.Cantidad)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.");

        RuleFor(x => x.Precio)
            .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo.");

        When(x => !x.IdProducto.HasValue, () =>
        {
            RuleFor(x => x.ProductoNombre)
                .NotEmpty().WithMessage("El nombre del producto es obligatorio cuando no se especifica un producto existente.");
        });
    }
}
