using HotChocolate;

namespace SecurityAuditDashboard.Api.GraphQL;

/// <summary>
/// GraphQL Error Filter for handling exceptions
/// This is a placeholder - will be implemented in Task 11
/// </summary>
public class GraphQLErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        // Basic error handling - will be enhanced later
        return error;
    }
}
