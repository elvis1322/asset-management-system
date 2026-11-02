using Application.DTO.Profile;
using FluentValidation;
namespace Application.Validators.Profile;

public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Emaili është i detyrueshëm.")
            .EmailAddress().WithMessage("Format jo i vlefshëm i email-it.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Emaili nuk mund të jetë 'string'.");
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token-i është i detyrueshëm.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Token-i nuk mund të jetë 'string'.");
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Fjalëkalimi i ri është i detyrueshëm.")
            .MinimumLength(8).WithMessage("Fjalëkalimi duhet të ketë së paku 8 karaktere.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Fjalëkalimi i ri nuk mund të jetë 'string'.");
    }
}