using MediatR;
using PvzApi.Services;

namespace PvzApi.Features.Parcels;

public record ReturnParcelCommand(int Id) : IRequest<string>;

public class ReturnParcelHandler : IRequestHandler<ReturnParcelCommand, string>
{
    private readonly IParcelService _parcels;

    public ReturnParcelHandler(IParcelService parcels) => _parcels = parcels;

    public Task<string> Handle(ReturnParcelCommand request, CancellationToken ct) =>
        _parcels.ReturnAsync(request.Id, ct);
}
