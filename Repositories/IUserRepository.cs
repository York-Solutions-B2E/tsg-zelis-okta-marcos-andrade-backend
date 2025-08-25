using SecurityAuditDashboard.Api.Data.Entities;

namespace SecurityAuditDashboard.Api.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByExternalIdAsync(string externalId);
    Task<User?> GetByIdWithRoleAndClaimsAsync(Guid id);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task<List<User>> GetAllWithRolesAsync();
}