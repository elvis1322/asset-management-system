using Application.DTO.Assets;
using FluentValidation;

namespace Application.Validators.Assets;

public class UnassignAssetDtoValidator : AbstractValidator<UnassignAssetDto>
{
    public UnassignAssetDtoValidator()
    {
        RuleFor(x => x.EmployeeId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("ID e punonjësit është e detyrueshme.");

        When(x => x.Notes != null, () =>
        {
            RuleFor(x => x.Notes)
                .Cascade(CascadeMode.Stop)
                .Must(notes => !string.IsNullOrWhiteSpace(notes))
                .WithMessage("Shënimet nuk mund të jenë bosh.")
                .MaximumLength(500)
                .WithMessage("Shënimet nuk mund të jenë më të gjata se 500 karaktere.")
                .Must(notes => !string.Equals(
                    notes?.Trim(),
                    "string",
                    StringComparison.OrdinalIgnoreCase))
                .WithMessage("Shënimet nuk mund të jenë 'string'.");
        });
    }
}