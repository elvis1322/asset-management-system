using Application.DTO.Assets;
using FluentValidation;

namespace Application.Validators.Assets;

public class AssignAssetDtoValidator : AbstractValidator<AssignAssetDto>
{
    public AssignAssetDtoValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("ID-ja e punonjësit është e detyrueshme.");

        When(x => x.Notes != null, () =>
        {
            RuleFor(x => x.Notes)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Shënimet nuk mund të jenë të zbrazëta.")
                .MaximumLength(500).WithMessage("Shënimet nuk mund të jenë më të gjata se 500 karaktere.")
                .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Shënimet nuk mund të jenë 'string'.");
        });
    }
}