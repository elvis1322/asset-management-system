using Application.DTO.Profile;
using FluentValidation;

namespace Application.Validators.Profile;

public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Fjalëkalimi aktual është i detyrueshëm.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Fjalëkalimi aktual nuk mund të jetë 'string'.");

        RuleFor(x => x.NewPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Fjalëkalimi i ri është i detyrueshëm.")
            .MinimumLength(6).WithMessage("Fjalëkalimi i ri duhet të ketë së paku 6 karaktere.")
            .MaximumLength(100).WithMessage("Fjalëkalimi i ri nuk mund të jetë më i gjatë se 100 karaktere.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Fjalëkalimi i ri nuk mund të jetë 'string'.");

        RuleFor(x => x.ConfirmPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Konfirmimi i fjalëkalimit është i detyrueshëm.")
            .Equal(x => x.NewPassword).WithMessage("Fjalëkalimi i ri dhe konfirmimi nuk përputhen.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Konfirmimi i fjalëkalimit nuk mund të jetë 'string'.");
    }
}