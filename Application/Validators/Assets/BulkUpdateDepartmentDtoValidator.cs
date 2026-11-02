using Application.DTO.Assets;
using FluentValidation;

namespace Application.Validators.Assets;

public class BulkUpdateDepartmentDtoValidator : AbstractValidator<BulkUpdateDepartmentDto>
{
    public BulkUpdateDepartmentDtoValidator()
    {
        RuleFor(x => x.AssetIds)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Lista e ID-ve të aseteve nuk mund të jetë bosh.")
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithMessage("Të gjitha ID-të e aseteve duhet të jenë të vlefshme.")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Lista nuk duhet të përmbajë ID të përsëritura.");

        RuleFor(x => x.NewDepartmentId)
            .NotEmpty().WithMessage("ID-ja e departamentit të ri është e detyrueshme.");
    }
}