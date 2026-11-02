using Application.DTO.Profile;
using FluentValidation;

namespace Application.Validators.Profile;

public class ConfirmEmailChangeDtoValidator : AbstractValidator<ConfirmEmailChangeDto>
{
    public ConfirmEmailChangeDtoValidator()
    {
        RuleFor(x => x.NewEmail)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Emaili i ri është i detyrueshëm.")
            .EmailAddress().WithMessage("Format jo i vlefshëm i email-it.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Emaili i ri nuk mund të jetë 'string'.");

        RuleFor(x => x.Token)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Token-i verifikues është i detyrueshëm.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Token-i verifikues nuk mund të jetë 'string'.");
    }
}