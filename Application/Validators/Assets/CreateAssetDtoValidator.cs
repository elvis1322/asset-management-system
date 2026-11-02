using Application.DTO.Assets;
using Domain.Entities;
using FluentValidation;

namespace Application.Validators.Assets;

public class CreateAssetDtoValidator : AbstractValidator<CreateAssetDto>
{
    public CreateAssetDtoValidator()
    {
        RuleFor(x => x.AssetTag)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Tag-u i pajisjes (AssetTag) është i detyrueshëm.")
            .MaximumLength(50).WithMessage("AssetTag nuk mund të jetë më i gjatë se 50 karaktere.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("AssetTag nuk mund të jetë 'string'.");

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Emri i pajisjes është i detyrueshëm.")
            .MaximumLength(100).WithMessage("Emri nuk mund të jetë më i gjatë se 100 karaktere.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Emri nuk mund të jetë 'string'.");

        RuleFor(x => x.Category)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Kategoria është e detyrueshme.")
            .Must(categoryStr => Enum.TryParse<AssetCategory>(categoryStr, true, out var category) &&
                                 Enum.IsDefined(typeof(AssetCategory), category))
            .WithMessage("Kategoria e dhënë nuk është e vlefshme.");

        RuleFor(x => x.PurchaseValue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Vlera e blerjes nuk mund të jetë negative.");

        RuleFor(x => x.PurchaseDate)
            .NotEmpty()
            .WithMessage("Data e blerjes është e detyrueshme.")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Data e blerjes nuk mund të jetë në të ardhmen.");

        When(x => x.WarrantyExpirationDate.HasValue, () =>
        {
            RuleFor(x => x.WarrantyExpirationDate)
                .GreaterThanOrEqualTo(x => x.PurchaseDate)
                .WithMessage("Data e skadimit të garancisë duhet të jetë pas ose në datën e blerjes.");
        });

        When(x => x.IsShared, () =>
        {
            RuleFor(x => x.DepartmentId)
                .NotEmpty()
                .WithMessage("Aseti i përbashkët duhet t'i caktohet një departamenti.");
        });
    }
}