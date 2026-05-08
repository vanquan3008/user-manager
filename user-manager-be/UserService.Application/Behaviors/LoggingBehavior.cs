using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace UserService.Application.Behaviors;
public class LoggingBehavior<TRequest, TResponse> :
    IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Handling {RequestName} - {@Request}", requestName, request);

        try
        {
            var response = await next();
            stopwatch.Stop();
            _logger.LogInformation("Handled {RequestName} in {ElapsedMs}ms - {@Response}", requestName, stopwatch.ElapsedMilliseconds,  response);
            if (stopwatch.ElapsedMilliseconds > 3000)
            {
                _logger.LogWarning("SLOW REQUEST: {RequestName} took {ElapsedMs}ms", requestName, stopwatch.ElapsedMilliseconds);
            }
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,"Error handling {RequestName} after {ElapsedMs}ms", requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
