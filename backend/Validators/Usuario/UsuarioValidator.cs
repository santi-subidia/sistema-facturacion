
using Backend.Models;
using FluentValidation;

namespace Backend.Validators.Usuario
{
	public class UsuarioValidator : AbstractValidator<Backend.DTOs.Usuario.UsuarioCreateDto>
	{
		public UsuarioValidator()
		{
			RuleFor(u => u.Username)
				.NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
				.MinimumLength(3).WithMessage("El nombre de usuario debe tener al menos 3 caracteres.");

			RuleFor(u => u.PasswordHash)
				.NotEmpty().WithMessage("La contraseña es obligatoria.")
				.MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.");

			RuleFor(u => u.Nombre)
				.NotEmpty().WithMessage("El nombre es obligatorio.")
				.MinimumLength(3).WithMessage("El nombre debe tener al menos 3 caracteres.")
				.Matches(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$").WithMessage("El nombre solo puede contener letras y espacios.");
		}
	}
}
