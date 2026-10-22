using ContactApi.Models;
using FluentValidation;

namespace ContactApi.Validators;

public class ContactValidator : AbstractValidator<Contact>
{
    private const string Phone = @"^\+?[1-9]\d{9,14}$";
    private const string Name = @"^\p{L}[\p{L}\s'-]*$";

    public ContactValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .Length(2, 100)
            .Matches(Name);

        RuleFor(c => c.Phone)
            .NotEmpty()
            .Matches(Phone);

        RuleFor(c => c.AltPhone)
            .Matches(Phone)
            .NotEqual(c => c.Phone)
            .When(c => !string.IsNullOrWhiteSpace(c.AltPhone));

        RuleFor(c => c.Email)
            .NotEmpty()
            .MaximumLength(254)
            .EmailAddress();

        RuleFor(c => c.Note)
            .MaximumLength(500);
    }
}
