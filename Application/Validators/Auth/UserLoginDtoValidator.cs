using Application.DTO.Auth;
using FluentValidation;

namespace Application.Validators.Auth;

public class UserLoginDtoValidator : AbstractValidator<UserLoginDto>
{
    public UserLoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email-i është i detyrueshëm.")
            .EmailAddress().WithMessage("Formati i email-it nuk është i saktë.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Email-i nuk mund të jetë 'string'.");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Fjalëkalimi është i detyrueshëm.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Fjalëkalimi nuk mund të jetë 'string'.");
    }
}