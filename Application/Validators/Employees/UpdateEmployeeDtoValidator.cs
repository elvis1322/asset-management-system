using Application.DTO.Employees;
using FluentValidation;

namespace Application.Validators.Employees;

public class UpdateEmployeeDtoValidator : AbstractValidator<UpdateEmployeeDto>
{
    public UpdateEmployeeDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Emri është i detyrueshëm.")
            .Length(2, 50).WithMessage("Emri duhet të ketë midis 2 dhe 50 karaktere.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Emri nuk mund të jetë 'string'.")
            .Matches(@"^[\p{L}\p{M}\s'-]+$")
            .WithMessage("Emri përmban karaktere të pavlefshme.");

        RuleFor(x => x.LastName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Mbiemri është i detyrueshëm.")
            .Length(2, 50).WithMessage("Mbiemri duhet të ketë midis 2 dhe 50 karaktere.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Mbiemri nuk mund të jetë 'string'.")
            .Matches(@"^[\p{L}\p{M}\s'-]+$")
            .WithMessage("Mbiemri përmban karaktere të pavlefshme.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email-i është i detyrueshëm.")
            .EmailAddress().WithMessage("Formati i email-it nuk është valid.")
            .MaximumLength(100).WithMessage("Email-i nuk mund të kalojë 100 karaktere.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Email-i nuk mund të jetë 'string'.");

        RuleFor(x => x.Gender)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Gjinia është e detyrueshme.")
            .Must(val => val != null &&
                (val.Equals("Male", StringComparison.OrdinalIgnoreCase) ||
                 val.Equals("Female", StringComparison.OrdinalIgnoreCase)))
            .WithMessage("Gjinia mund të jetë vetëm 'Male' ose 'Female'.");
    }
}