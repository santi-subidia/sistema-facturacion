using FluentValidation;
using Backend.DTOs.Caja;

namespace Backend.Validators.Caja
{
    public class CrearCajaValidator : AbstractValidator<CrearCajaDto>
    {
        public CrearCajaValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre de la caja es obligatorio.")
                .MaximumLength(150).WithMessage("El nombre no puede tener más de 150 caracteres.");

            RuleFor(x => x.PuntoVenta)
                .InclusiveBetween(1, 99998).WithMessage("El punto de venta debe estar entre 1 y 99998.");
        }
    }
}
