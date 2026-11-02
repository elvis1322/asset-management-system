using Application.DTO.Auth;
using FluentValidation;
namespace Application.Validators.Auth;

public class LogoutRequestDtoValidator : AbstractValidator<LogoutRequestDto>
{
    public LogoutRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh Token është i detyrueshëm.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Refresh Token nuk mund të jetë 'string'.");
    }
}