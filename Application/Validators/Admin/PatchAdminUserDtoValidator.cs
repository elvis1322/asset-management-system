using Application.DTO.Admin;
using FluentValidation;

namespace Application.Validators.Admin;

public class PatchAdminUserDtoValidator : AbstractValidator<PatchAdminUserDto>
{
    public PatchAdminUserDtoValidator()
    {
        When(x => x.FirstName != null, () =>
        {
            RuleFor(x => x.FirstName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Emri nuk mund të jetë bosh nëse dërgohet.")
                .Length(2, 50)
                .WithMessage("Emri duhet të ketë midis 2 dhe 50 karaktere.")
                .Matches(@"^[\p{L}\p{M}\s'-]+$")
                .WithMessage("Emri mund të përmbajë vetëm shkronja, hapësira, apostrofë dhe viza.")
                .Must(val => !string.Equals(
                    val?.Trim(),
                    "string",
                    StringComparison.OrdinalIgnoreCase))
                .WithMessage("Emri nuk mund të jetë 'string'.");
        });

        When(x => x.LastName != null, () =>
        {
            RuleFor(x => x.LastName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Mbiemri nuk mund të jetë bosh nëse dërgohet.")
                .Length(2, 50)
                .WithMessage("Mbiemri duhet të ketë midis 2 dhe 50 karaktere.")
                .Matches(@"^[\p{L}\p{M}\s'-]+$")
                .WithMessage("Mbiemri mund të përmbajë vetëm shkronja, hapësira, apostrofë dhe viza.")
                .Must(val => !string.Equals(
                    val?.Trim(),
                    "string",
                    StringComparison.OrdinalIgnoreCase))
                .WithMessage("Mbiemri nuk mund të jetë 'string'.");
        });

        When(x => x.Email != null, () =>
        {
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Email nuk mund të jetë bosh nëse dërgohet.")
                .EmailAddress()
                .WithMessage("Formati i email-it nuk është i saktë.")
                .MaximumLength(100)
                .WithMessage("Email-i nuk mund të jetë më i gjatë se 100 karaktere.")
                .Must(val => !string.Equals(
                    val?.Trim(),
                    "string",
                    StringComparison.OrdinalIgnoreCase))
                .WithMessage("Email nuk mund të jetë 'string'.");
        });

        RuleFor(x => x.Genre)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Gjinia është e detyrueshme.")
            .Must(g => new[] { "Male", "Female", "M", "F" }
                .Contains(g.Trim(), StringComparer.OrdinalIgnoreCase))
            .WithMessage("Gjinia duhet të jetë Male, Female, M ose F.");
    }
}