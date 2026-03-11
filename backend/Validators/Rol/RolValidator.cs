using Backend.DTOs.Rol;
using FluentValidation;

namespace Backend.Validators.Rol
{
    public class RolValidator : AbstractValidator<RolCreateUpdateDto>
    {
        public RolValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del rol es obligatorio.")
                .MaximumLength(50).WithMessage("El nombre del rol no puede exceder 50 caracteres.");
        }
    }
}
