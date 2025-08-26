namespace SecurityAuditDashboard.Api.Services.Interfaces;

public interface IUserService
{
    Task<bool> AssignRoleAsync(Guid userId, Guid roleId, Guid authorUserId);
    Task<IEnumerable<object>> GetAllUsersAsync();
}