
using Application.DTO.Employees;
using FluentValidation;

namespace Application.Validators.Employees;

public class UpdateEmployeeDepartmentDtoValidator
    : AbstractValidator<UpdateEmployeeDepartmentDto>
{
    public UpdateEmployeeDepartmentDtoValidator()
    {
        RuleFor(x => x.NewDepartmentId)
            .NotEmpty()
            .WithMessage("ID-ja e departamentit të ri është e detyrueshme.");
    }
}
