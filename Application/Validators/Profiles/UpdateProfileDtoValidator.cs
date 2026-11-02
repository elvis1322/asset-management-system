using Application.DTO.Profile;
using FluentValidation;

namespace Application.Validators.Profile;

public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
{
    public UpdateProfileDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Emri është i detyrueshëm.")
            .Length(2, 50).WithMessage("Emri duhet të ketë midis 2 dhe 50 karaktere.")
            .Must(val => !string.Equals(val.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Emri nuk mund të jetë 'string'.")
            .Matches(@"^[\p{L}\p{M}\s'-]+$")
            .WithMessage("Emri përmban karaktere të pavlefshme.");

        RuleFor(x => x.LastName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Mbiemri është i detyrueshëm.")
            .Length(2, 50).WithMessage("Mbiemri duhet të ketë midis 2 dhe 50 karaktere.")
            .Must(val => !string.Equals(val.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Mbiemri nuk mund të jetë 'string'.")
            .Matches(@"^[\p{L}\p{M}\s'-]+$")
            .WithMessage("Mbiemri përmban karaktere të pavlefshme.");

        RuleFor(x => x.Gender)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Gjinia është e detyrueshme.")
            .Must(g =>
                g != null &&
                (g.Equals("Male", StringComparison.OrdinalIgnoreCase) ||
                 g.Equals("Female", StringComparison.OrdinalIgnoreCase)))
            .WithMessage("Gjinia mund të jetë vetëm 'Male' ose 'Female'.");
    }
}