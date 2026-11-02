using Application.DTO.Employees;
using FluentValidation;

namespace Application.Validators.Employees;

public class UpdateUserRoleDtoValidator : AbstractValidator<UpdateUserRoleDto>
{
    public UpdateUserRoleDtoValidator()
    {
        RuleFor(x => x.NewRole)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Roli i ri është i detyrueshëm.")
            .Must(role => role != null &&
                (role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                 role.Equals("Manager", StringComparison.OrdinalIgnoreCase) ||
                 role.Equals("Employee", StringComparison.OrdinalIgnoreCase)))
            .WithMessage("Roli duhet të jetë 'Admin', 'Manager' ose 'Employee'.");

        RuleFor(x => x.Position)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Pozicioni është i detyrueshëm.")
            .MaximumLength(100).WithMessage("Pozicioni nuk mund të kalojë 100 karaktere.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Pozicioni nuk mund të jetë 'string'.");
    }
}