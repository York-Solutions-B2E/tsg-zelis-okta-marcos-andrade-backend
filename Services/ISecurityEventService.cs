using SecurityAuditDashboard.Api.Data.Entities;

namespace SecurityAuditDashboard.Api.Services;

public interface ISecurityEventService
{
    Task LogLoginSuccessAsync(Guid userId, string provider);
    Task LogLogoutAsync(Guid userId);
    Task LogRoleAssignedAsync(Guid authorUserId, Guid affectedUserId, string fromRole, string toRole);
    Task<IEnumerable<SecurityEvent>> GetAuthEventsAsync();
    Task<IEnumerable<SecurityEvent>> GetRoleChangeEventsAsync();
}