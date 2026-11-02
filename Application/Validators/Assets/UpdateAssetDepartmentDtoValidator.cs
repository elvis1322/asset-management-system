
using Application.DTO.Assets;
using FluentValidation;

namespace Application.Validators.Assets;

public class UpdateAssetDepartmentDtoValidator: AbstractValidator<UpdateAssetDepartmentDto>
{
    public UpdateAssetDepartmentDtoValidator()
    {
        RuleFor(x => x.NewDepartmentId)
            .NotEmpty()
            .WithMessage("ID-ja e departamentit të ri është e detyrueshme.");
    }
}
