
using Application.DTO.Employees;
using FluentValidation;

namespace Application.Validators.Employees;

public class PromoteUserDtoValidator : AbstractValidator<PromoteUserDto>
{
    public PromoteUserDtoValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .WithMessage("ID-ja e departamentit është e detyrueshme.");
    }
}
