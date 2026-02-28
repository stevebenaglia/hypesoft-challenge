using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Hypesoft.Application.Behaviors;

public sealed class LoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int SlowRequestThresholdMs = 500;
    private readonly ILogger<LoggingPipelineBehavior<TRequest, TResponse>> _logger;

    public LoggingPipelineBehavior(ILogger<LoggingPipelineBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();

        var response = await next();

        sw.Stop();

        if (sw.ElapsedMilliseconds > SlowRequestThresholdMs)
            _logger.LogWarning("Slow request {RequestName} completed in {ElapsedMs}ms", requestName, sw.ElapsedMilliseconds);
        else
            _logger.LogInformation("Request {RequestName} completed in {ElapsedMs}ms", requestName, sw.ElapsedMilliseconds);

        return response;
    }
}
