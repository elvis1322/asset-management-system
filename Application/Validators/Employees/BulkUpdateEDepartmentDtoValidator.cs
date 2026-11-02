using Application.DTO.Employees;
using FluentValidation;

namespace Application.Validators.Employees;

public class BulkUpdateEDepartmentDtoValidator : AbstractValidator<BulkUpdateEDepartmentDto>
{
    public BulkUpdateEDepartmentDtoValidator()
    {
        RuleFor(x => x.EmployeeIds)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Lista e ID-ve të punonjësve është e detyrueshme.")
            .Must(ids => ids.All(id => id != Guid.Empty))
            .WithMessage("Të gjitha ID-të e punonjësve duhet të jenë të vlefshme.")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Lista nuk duhet të përmbajë ID të përsëritura.");

        RuleFor(x => x.NewDepartmentId)
            .NotEmpty().WithMessage("ID-ja e departamentit të ri është e detyrueshme.");
    }
}