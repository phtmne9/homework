using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;

namespace ProductApi.Features.Products;

public record DeleteProductCommand(int Id) : IRequest<string>;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, string>
{
    private readonly AppDbContext _db;
    private readonly ILogger<DeleteProductHandler> _log;

    public DeleteProductHandler(AppDbContext db, ILogger<DeleteProductHandler> log)
    {
        _db = db;
        _log = log;
    }

    public async Task<string> Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.Id, ct);

        if (product is null)
        {
            _log.LogWarning("Товар {Id} не найден", request.Id);
            return $"Товар {request.Id} не найден";
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync(ct);
        _log.LogInformation("Товар {Id} удалён", request.Id);

        return $"Товар {request.Id} удалён";
    }
}
