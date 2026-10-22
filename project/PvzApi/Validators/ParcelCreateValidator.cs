using FluentValidation;
using PvzApi.Models;

namespace PvzApi.Validators;

public class ParcelCreateValidator : AbstractValidator<ParcelCreateDto>
{
    public ParcelCreateValidator()
    {
        RuleFor(p => p.TrackNumber)
            .NotEmpty().WithMessage("Трек-номер обязателен")
            .MaximumLength(50)
            .Matches("^[A-Za-z0-9-]+$").WithMessage("Трек-номер: латиница, цифры и дефис");

        RuleFor(p => p.Weight)
            .GreaterThan(0).WithMessage("Вес должен быть больше нуля")
            .LessThanOrEqualTo(100).WithMessage("Вес не может превышать 100 кг");

        RuleFor(p => p.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Сумма не может быть отрицательной");

        RuleFor(p => p.StorageDays)
            .InclusiveBetween(1, 30).WithMessage("Срок хранения от 1 до 30 дней");

        RuleFor(p => p.PickupPointId).GreaterThan(0).WithMessage("Не указан ПВЗ");

        RuleFor(p => p.ClientId).GreaterThan(0).WithMessage("Не указан клиент");
    }
}
