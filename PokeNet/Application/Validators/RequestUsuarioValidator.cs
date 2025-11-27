using FluentValidation;
using PokeNet.Application.DTO.Request;

namespace PokeNet.Application.Validators
{
    public class RequestUsuarioValidator : AbstractValidator<UsuarioRequest>
    {
        public RequestUsuarioValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório.")
                .MaximumLength(250).WithMessage("Nome deve ter no máximo 250 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-mail é obrigatório.")
                .MaximumLength(255).WithMessage("E-mail deve ter no máximo 255 caracteres.")
                .EmailAddress().WithMessage("E-mail inválido.");

            RuleFor(x => x.Senha)
                .NotEmpty().WithMessage("Senha é obrigatória.")
                .MaximumLength(255).WithMessage("Senha deve ter no máximo 255 caracteres.")
                .Must(s =>
                    s.Any(char.IsUpper) &&                       // Pelo menos uma letra maiúscula
                    s.Any(ch => !char.IsLetterOrDigit(ch)) &&    // Pelo menos um caractere especial
                    s.Length > 8)                                // Maior que 8 caracteres
                .WithMessage("Senha deve conter pelo menos uma letra maiúscula, um caractere especial e ter mais de 8 caracteres.");

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("Role inválida.");

            // 🟢 Regra que você pediu — limite de 6 pokémons no time
            RuleFor(x => x.Time)
                .Must(lista => lista == null || lista.Count <= 6)
                .WithMessage("O time não pode ter mais que 6 pokémons.");
        }
    }
}
