using FluentValidation;
using Backend.DTOs.Productos;

namespace Backend.Validators.Productos;

public class AjusteMasivoRequestValidator : AbstractValidator<AjusteMasivoRequest>
{
    public AjusteMasivoRequestValidator()
    {
        RuleFor(x => x.ProductosIds)
            .NotEmpty().WithMessage("Debe seleccionar al menos un producto.")
            .Must(ids => ids != null && ids.Any()).WithMessage("Debe seleccionar al menos un producto.");

        RuleFor(x => x.Porcentaje)
            .NotEqual(0).WithMessage("El porcentaje no puede ser 0.")
            .GreaterThan(-100).WithMessage("El porcentaje no puede ser menor a -100%.");

        RuleFor(x => x.Redondeo)
            .GreaterThan(0).When(x => x.Redondeo.HasValue)
            .WithMessage("El redondeo debe ser mayor a 0.");
    }
}

public class AjusteStockItemValidator : AbstractValidator<AjusteStockItem>
{
    public AjusteStockItemValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("El ID del producto debe ser mayor a 0.");

        RuleFor(x => x.TipoAjuste)
            .NotEmpty().WithMessage("El tipo de ajuste es obligatorio.")
            .Must(tipo => new[] { "ingreso", "egreso", "fisico" }.Contains(tipo.ToLower()))
            .WithMessage("El tipo de ajuste debe ser 'ingreso', 'egreso' o 'fisico'.");

        RuleFor(x => x.Cantidad)
            .GreaterThanOrEqualTo(0).WithMessage("La cantidad no puede ser negativa.")
            .When(x => x.TipoAjuste.ToLower() != "fisico");

        RuleFor(x => x.StockNuevo)
            .GreaterThanOrEqualTo(0).WithMessage("El stock nuevo no puede ser negativo.")
            .When(x => x.TipoAjuste.ToLower() == "fisico");
    }
}

public class AjusteStockRequestValidator : AbstractValidator<AjusteStockRequest>
{
    public AjusteStockRequestValidator()
    {
        RuleFor(x => x.Ajustes)
            .NotEmpty().WithMessage("Debe proporcionar al menos un ajuste.")
            .Must(ajustes => ajustes != null && ajustes.Any())
            .WithMessage("Debe proporcionar al menos un ajuste.");

        RuleForEach(x => x.Ajustes)
            .SetValidator(new AjusteStockItemValidator());
    }
}
