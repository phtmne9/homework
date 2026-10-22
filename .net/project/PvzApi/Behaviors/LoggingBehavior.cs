using System.Diagnostics;
using MediatR;

namespace PvzApi.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _log;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> log) => _log = log;

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var name = typeof(TRequest).Name;
        var timer = Stopwatch.StartNew();

        _log.LogInformation("Начало {Request}", name);

        var response = await next();

        timer.Stop();
        _log.LogInformation("Конец {Request} за {Ms} мс", name, timer.ElapsedMilliseconds);

        return response;
    }
}
