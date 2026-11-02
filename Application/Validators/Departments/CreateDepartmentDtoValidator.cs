using Application.DTO.Departments;
using FluentValidation;

namespace Application.Validators.Departments;

public class CreateDepartmentDtoValidator : AbstractValidator<CreateDepartmentDto>
{
    public CreateDepartmentDtoValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Emri i departamentit është i detyrueshëm.")
            .Length(2, 100).WithMessage("Emri duhet të ketë midis 2 dhe 100 karaktere.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Emri i departamentit nuk mund të jetë 'string'.")
            .Matches(@"^[\p{L}\p{M}\d\s'-]+$")
            .WithMessage("Emri i departamentit përmban karaktere të pavlefshme.");

        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Kodi i departamentit është i detyrueshëm.")
            .MaximumLength(20).WithMessage("Kodi nuk mund të jetë më i gjatë se 20 karaktere.")
            .Matches(@"^[A-Z0-9_-]+$").WithMessage("Kodi duhet të përmbajë vetëm shkronja të mëdha, numra, vizë ose underscore.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Kodi i departamentit nuk mund të jetë 'string'.");

        When(x => !string.IsNullOrWhiteSpace(x.Description), () =>
        {
            RuleFor(x => x.Description)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(500).WithMessage("Përshkrimi nuk mund të jetë më i gjatë se 500 karaktere.")
                .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Përshkrimi nuk mund të jetë 'string'.");
        });
    }
}