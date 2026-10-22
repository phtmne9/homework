using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;

namespace ProductApi.Features.Products;

public record UpdateProductCommand(int Id, string Name, decimal Price, int Quantity)
    : IRequest<string>;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, string>
{
    private readonly AppDbContext _db;
    private readonly ILogger<UpdateProductHandler> _log;

    public UpdateProductHandler(AppDbContext db, ILogger<UpdateProductHandler> log)
    {
        _db = db;
        _log = log;
    }

    public async Task<string> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return "Название товара обязательно";

        if (request.Price < 0 || request.Quantity < 0)
            return "Цена и количество не могут быть отрицательными";

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.Id, ct);

        if (product is null)
        {
            _log.LogWarning("Товар {Id} не найден", request.Id);
            return $"Товар {request.Id} не найден";
        }

        product.Name = request.Name;
        product.Price = request.Price;
        product.Quantity = request.Quantity;
        await _db.SaveChangesAsync(ct);
        _log.LogInformation("Товар {Id} изменён", request.Id);

        return $"Товар {request.Id} изменён";
    }
}
