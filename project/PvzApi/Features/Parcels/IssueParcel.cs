using MediatR;
using PvzApi.Services;

namespace PvzApi.Features.Parcels;

public record IssueParcelCommand(int Id, string Phone) : IRequest<string>;

public class IssueParcelHandler : IRequestHandler<IssueParcelCommand, string>
{
    private readonly IParcelService _parcels;

    public IssueParcelHandler(IParcelService parcels) => _parcels = parcels;

    public Task<string> Handle(IssueParcelCommand request, CancellationToken ct) =>
        _parcels.IssueAsync(request.Id, request.Phone, ct);
}
