using Backend.DTOs.AfipConfiguracion;
using FluentValidation;

namespace Backend.Validators.AfipConfiguracion
{
    public class AfipConfiguracionValidator : AbstractValidator<AfipConfiguracionCreateUpdateDto>
    {
        public AfipConfiguracionValidator()
        {
            RuleFor(x => x.Cuit)
                .NotEmpty().WithMessage("El CUIT es obligatorio.")
                .MaximumLength(11).WithMessage("El CUIT no puede exceder 11 caracteres.")
                .Matches(@"^\d{11}$").WithMessage("El CUIT debe contener 11 dígitos numéricos.");

            RuleFor(x => x.RazonSocial)
                .NotEmpty().WithMessage("La razón social es obligatoria.")
                .MaximumLength(200).WithMessage("La razón social no puede exceder 200 caracteres.")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("La Razón Social solo puede contener caracteres alfabéticos.");

            RuleFor(x => x.NombreFantasia)
                .MaximumLength(100).WithMessage("El nombre de fantasía no puede exceder 100 caracteres.");

            RuleFor(x => x.IngresosBrutosNumero)
                .MaximumLength(50).WithMessage("El número de Ingresos Brutos no puede exceder 50 caracteres.");

            RuleFor(x => x.DireccionFiscal)
                .MaximumLength(200).WithMessage("La dirección fiscal no puede exceder 200 caracteres.");

            RuleFor(x => x.EmailContacto)
                .MaximumLength(100).WithMessage("El email de contacto no puede exceder 100 caracteres.")
                .EmailAddress().WithMessage("El email de contacto no es válido.")
                .When(x => !string.IsNullOrEmpty(x.EmailContacto));
            
            RuleFor(x => x.IdAfipCondicionIva)
                .GreaterThan(0).WithMessage("Debe seleccionar una condición de IVA válida.");
                

        }
    }
}
