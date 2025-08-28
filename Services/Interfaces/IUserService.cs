using SecurityAuditDashboard.Api.Data.Entities;

namespace SecurityAuditDashboard.Api.Services.Interfaces;

public interface IUserService
{
    Task<bool> AssignRoleAsync(Guid userId, Guid roleId, Guid authorUserId);
    Task<IEnumerable<object>> GetAllUsersAsync();
    Task<User> CreateOrUpdateUserAsync(string email, string externalId, string name, string provider);
}