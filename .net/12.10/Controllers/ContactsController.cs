using ContactApi.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ContactApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactsController : ControllerBase
{
    private readonly IValidator<Contact> _validator;

    public ContactsController(IValidator<Contact> validator) => _validator = validator;

    [HttpPost]
    public async Task<ActionResult<Contact>> Create(Contact contact, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(contact, ct);

        if (!result.IsValid)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(e.PropertyName, e.ErrorMessage);

            return ValidationProblem(ModelState);
        }

        return Ok(contact);
    }
}
