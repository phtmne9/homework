using FluentValidation;
using PvzApi.Models;

namespace PvzApi.Validators;

public class ClientValidator : AbstractValidator<ClientDto>
{
    public ClientValidator()
    {
        RuleFor(c => c.FullName)
            .NotEmpty().WithMessage("Имя клиента обязательно")
            .MaximumLength(150);

        RuleFor(c => c.Phone)
            .NotEmpty().WithMessage("Телефон обязателен")
            .Matches(@"^\+7\d{10}$").WithMessage("Телефон в формате +7XXXXXXXXXX");
    }
}
