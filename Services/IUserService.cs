using System.Security.Claims;

namespace SecurityAuditDashboard.Api.Services;

public interface IUserService
{
    Task HandleLoginAsync(ClaimsPrincipal principal, string provider);
    Task AssignRoleAsync(Guid userId, Guid roleId, Guid authorUserId);
}