using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PriorAuthSystem.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
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

        _logger.LogInformation("Handling {RequestName} with data {@RequestData}",
            requestName, request);

        var response = await next(cancellationToken);

        _logger.LogInformation("Handled {RequestName} with response {@Response}",
            requestName, response);

        return response;
    }
}
