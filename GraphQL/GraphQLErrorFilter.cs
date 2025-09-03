using HotChocolate;
using Microsoft.Extensions.Logging;

namespace SecurityAuditDashboard.Api.GraphQL;

/// <summary>
/// GraphQL Error Filter for handling exceptions with detailed logging
/// </summary>
public class GraphQLErrorFilter : IErrorFilter
{
    private readonly ILogger<GraphQLErrorFilter> _logger;

    public GraphQLErrorFilter(ILogger<GraphQLErrorFilter> logger)
    {
        _logger = logger;
    }

    public IError OnError(IError error)
    {
        // Log the full error details
        _logger.LogError(error.Exception, 
            "GraphQL Error: {Message}. Code: {Code}. Path: {Path}", 
            error.Message, 
            error.Code,
            error.Path?.ToString());

        // In development, include exception details
        if (error.Exception != null)
        {
            return error.WithMessage($"{error.Message} - {error.Exception.Message}")
                       .SetExtension("stackTrace", error.Exception.StackTrace);
        }

        return error;
    }
}
