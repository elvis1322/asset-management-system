using Application.DTO.Departments;
using FluentValidation;

namespace Application.Validators.Departments;

public class UpdateDepartmentCodeDtoValidator : AbstractValidator<UpdateDepartamentCodeDto>
{
    public UpdateDepartmentCodeDtoValidator()
    {
        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Kodi i departamentit është i detyrueshëm.")
            .MaximumLength(20).WithMessage("Kodi nuk mund të jetë më i gjatë se 20 karaktere.")
            .Matches(@"^[A-Z0-9_-]+$").WithMessage("Kodi duhet të përmbajë vetëm shkronja të mëdha, numra, vizë ose underscore.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Kodi i departamentit nuk mund të jetë 'string'.");
    }
}