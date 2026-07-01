namespace EnterpriseKit.Application.Common.Behaviours;

using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

/// <summary>
/// Logs the request name, execution time, and any slow-query warnings.
/// Sits first in the pipeline so it captures total processing time.
/// </summary>
public sealed class LoggingBehaviour<TRequest, TResponse>(
    ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const long SlowRequestThresholdMs = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();

        logger.LogInformation("→ Handling {RequestName}", requestName);

        try
        {
            var response = await next();
            sw.Stop();

            if (sw.ElapsedMilliseconds > SlowRequestThresholdMs)
            {
                logger.LogWarning(
                    "SLOW REQUEST {RequestName} completed in {Elapsed}ms (threshold: {Threshold}ms)",
                    requestName, sw.ElapsedMilliseconds, SlowRequestThresholdMs);
            }
            else
            {
                logger.LogInformation("← {RequestName} completed in {Elapsed}ms", requestName, sw.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "✗ {RequestName} failed after {Elapsed}ms", requestName, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
