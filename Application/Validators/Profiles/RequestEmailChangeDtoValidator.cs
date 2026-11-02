using Application.DTO.Profile;
using FluentValidation;

namespace Application.Validators.Profile;

public class RequestEmailChangeDtoValidator : AbstractValidator<RequestEmailChangeDto>
{
    public RequestEmailChangeDtoValidator()
    {
        RuleFor(x => x.NewEmail)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Emaili i ri është i detyrueshëm.")
            .EmailAddress().WithMessage("Ju lutem vendosni një adresë të vlefshme email-i.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Emaili i ri nuk mund të jetë 'string'.");
    }
}