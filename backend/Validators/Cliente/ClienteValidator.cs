using FluentValidation;
using Backend.DTOs.Cliente;

namespace Backend.Validators.Cliente
{
    public class ClienteValidator : AbstractValidator<ClienteCreateUpdateDto>
    {
        public ClienteValidator()
        {
            RuleFor(x => x.Documento)
                .NotEmpty().WithMessage("El documento es obligatorio.")
                .MaximumLength(13).WithMessage("El documento no puede exceder 13 caracteres.")
                .Matches(@"^(\d{7,8}|\d{2}-\d{8}-\d{1})$").WithMessage("Debe ser un DNI (7-8 dígitos) o CUIT (XX-XXXXXXXX-X).");

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres.")
                .Matches(@"^[a-zA-ZÀ-ÿ\s]+$").WithMessage("El nombre solo puede contener letras y espacios.");

            RuleFor(x => x.Apellido)
                .NotEmpty().WithMessage("El apellido es obligatorio.")
                .MaximumLength(100).WithMessage("El apellido no puede exceder 100 caracteres.")
                .Matches(@"^[a-zA-ZÀ-ÿ\s]+$").WithMessage("El apellido solo puede contener letras y espacios.");

            RuleFor(x => x.Telefono)
                .NotEmpty().WithMessage("El teléfono es obligatorio.")
                .MaximumLength(20).WithMessage("El teléfono no puede exceder 20 caracteres.");

            RuleFor(x => x.Correo)
                .MaximumLength(100).WithMessage("El correo no puede exceder 100 caracteres.")
                .EmailAddress().WithMessage("El formato del correo es inválido.")
                .When(x => !string.IsNullOrWhiteSpace(x.Correo));

            RuleFor(x => x.Direccion)
                .NotEmpty().WithMessage("La dirección es obligatoria.")
                .MaximumLength(200).WithMessage("La dirección no puede exceder 200 caracteres.");

            RuleFor(x => x.IdAfipCondicionIva)
                .GreaterThan(0).WithMessage("La condición de IVA es obligatoria.");
        }
    }
}
