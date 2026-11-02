using Application.DTO.Assets;
using FluentValidation;
namespace Application.Validators.Assets;

public class AssetFilterDtoValidator : AbstractValidator<AssetFilterDto>
{
    public AssetFilterDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Numri i faqes (PageNumber) duhet të jetë së paku 1.");
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Madhësia e faqes (PageSize) duhet të jetë midis 1 dhe 100.");
        When(x => x.SearchTerm != null, () =>
        {
            RuleFor(x => x.SearchTerm)
                .MaximumLength(100).WithMessage("Termi i kërkimit nuk mund të jetë më i gjatë se 100 karaktere.");
        });
    }
}