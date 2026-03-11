using Backend.DTOs.Usuario;
using FluentValidation;

namespace Backend.Validators.Usuario
{
    public class UsuarioUpdateValidator : AbstractValidator<UsuarioUpdateDto>
    {
        public UsuarioUpdateValidator()
        {
            RuleFor(u => u.Username)
                .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
                .MinimumLength(3).WithMessage("El nombre de usuario debe tener al menos 3 caracteres.");

            RuleFor(u => u.Nombre)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MinimumLength(3).WithMessage("El nombre debe tener al menos 3 caracteres.")
                .Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$").WithMessage("El nombre solo puede contener letras y espacios.");

            RuleFor(u => u.PasswordHash)
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.")
                .When(u => !string.IsNullOrWhiteSpace(u.PasswordHash));
        }
    }
}
