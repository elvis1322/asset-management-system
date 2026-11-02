using Application.DTO.Assets;
using Domain.Entities;
using FluentValidation;

namespace Application.Validators.Assets;

public class UpdateAssetDtoValidator : AbstractValidator<UpdateAssetDto>
{
    public UpdateAssetDtoValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Emri i pajisjes është i detyrueshëm.")
            .MaximumLength(100)
            .WithMessage("Emri nuk mund të jetë më i gjatë se 100 karaktere.")
            .Must(val => !string.Equals(
                val?.Trim(),
                "string",
                StringComparison.OrdinalIgnoreCase))
            .WithMessage("Emri i pajisjes nuk mund të jetë 'string'.");

        RuleFor(x => x.Category)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Kategoria është e detyrueshme.")
            .Must(category =>
                Enum.TryParse<AssetCategory>(category, true, out var parsedCategory) &&
                Enum.IsDefined(typeof(AssetCategory), parsedCategory))
            .WithMessage("Kategoria e dhënë nuk është e vlefshme.");

        RuleFor(x => x.PurchaseValue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Vlera e blerjes nuk mund të jetë negative.");

        When(x => x.WarrantyExpirationDate.HasValue, () =>
        {
            RuleFor(x => x.WarrantyExpirationDate)
                .GreaterThan(DateTime.MinValue)
                .WithMessage("Data e skadimit të garancisë nuk është e vlefshme.");
        });
    }
}