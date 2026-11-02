using Application.DTO.Employees;
using FluentValidation;

namespace Application.Validators.Employees;

public class CreateEmployeeDtoValidator : AbstractValidator<CreateEmployeeDto>
{
    public CreateEmployeeDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Emri është i detyrueshëm.")
            .Length(2, 50).WithMessage("Emri duhet të ketë midis 2 dhe 50 karaktere.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Emri nuk mund të jetë 'string'.")
            .Matches(@"^[\p{L}\p{M}\s'-]+$").WithMessage("Emri përmban karaktere të pavlefshme.");

        RuleFor(x => x.LastName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Mbiemri është i detyrueshëm.")
            .Length(2, 50).WithMessage("Mbiemri duhet të ketë midis 2 dhe 50 karaktere.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Mbiemri nuk mund të jetë 'string'.")
            .Matches(@"^[\p{L}\p{M}\s'-]+$").WithMessage("Mbiemri përmban karaktere të pavlefshme.");

        RuleFor(x => x.Position)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Pozicioni është i detyrueshëm.")
            .MaximumLength(100).WithMessage("Pozicioni nuk mund të kalojë 100 karaktere.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Pozicioni nuk mund të jetë 'string'.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email-i është i detyrueshëm.")
            .EmailAddress().WithMessage("Formati i email-it nuk është valid.")
            .MaximumLength(100).WithMessage("Email-i nuk mund të kalojë 100 karaktere.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Email-i nuk mund të jetë 'string'.");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Fjalëkalimi është i detyrueshëm.")
            .MinimumLength(6).WithMessage("Fjalëkalimi duhet të ketë së paku 6 karaktere.")
            .MaximumLength(100).WithMessage("Fjalëkalimi nuk mund të kalojë 100 karaktere.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Fjalëkalimi nuk mund të jetë 'string'.");

        RuleFor(x => x.Gender)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Gjinia është e detyrueshme.")
            .Must(val => val != null &&
                (val.Equals("Male", StringComparison.OrdinalIgnoreCase) ||
                 val.Equals("Female", StringComparison.OrdinalIgnoreCase)))
            .WithMessage("Gjinia mund të jetë vetëm 'Male' ose 'Female'.");

        RuleFor(x => x.Role)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Roli është i detyrueshëm.")
            .Must(role => role != null &&
                (role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                 role.Equals("Manager", StringComparison.OrdinalIgnoreCase) ||
                 role.Equals("Employee", StringComparison.OrdinalIgnoreCase)))
            .WithMessage("Roli duhet të jetë 'Admin', 'Manager' ose 'Employee'.");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Departamenti është i detyrueshëm.");
    }
}