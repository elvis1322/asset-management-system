using Application.DTO.Assets;
using Domain.Entities;
using FluentValidation;

namespace Application.Validators.Assets;

public class CreateManagerAssetDtoValidator : AbstractValidator<CreateManagerAssetDto>
{
    public CreateManagerAssetDtoValidator()
    {
        RuleFor(x => x.AssetTag)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Etiketa e asetit është e detyrueshme.")
            .MaximumLength(50).WithMessage("Etiketa e asetit nuk mund të jetë më e gjatë se 50 karaktere.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Etiketa e asetit nuk mund të jetë 'string'.");

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Emri i asetit është i detyrueshëm.")
            .MaximumLength(100).WithMessage("Emri i asetit nuk mund të jetë më i gjatë se 100 karaktere.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Emri i asetit nuk mund të jetë 'string'.");

     RuleFor(x => x.Category)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Kategoria është e detyrueshme.")
            .Must(categoryStr => Enum.TryParse<AssetCategory>(categoryStr, true, out var category) &&
                                 Enum.IsDefined(typeof(AssetCategory), category))
            .WithMessage("Kategoria e dhënë nuk është e vlefshme.");

        RuleFor(x => x.SerialNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Numri serik është i detyrueshëm.")
            .MaximumLength(100).WithMessage("Numri serik nuk mund të jetë më i gjatë se 100 karaktere.")
            .Must(val => !string.Equals(val?.Trim(), "string", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Numri serik nuk mund të jetë 'string'.");

        RuleFor(x => x.PurchaseValue)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Vlera e blerjes nuk mund të jetë negative.");

        RuleFor(x => x.PurchaseDate)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Data e blerjes është e detyrueshme.")
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Data e blerjes nuk mund të jetë në të ardhmen.");

        RuleFor(x => x.WarrantyExpirationDate)
            .GreaterThanOrEqualTo(x => x.PurchaseDate)
            .When(x => x.WarrantyExpirationDate.HasValue)
            .WithMessage("Data e skadimit të garancisë nuk mund të jetë para datës së blerjes.");
    }
}