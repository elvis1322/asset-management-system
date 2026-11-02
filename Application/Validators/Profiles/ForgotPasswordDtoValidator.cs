using Application.DTO.Profile;
using FluentValidation;
namespace Application.Validators.Profile;

public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Emaili është i detyrueshëm.")
            .EmailAddress()
            .WithMessage("Emaili duhet të jetë në format të vlefshëm.");
    }
}