using SecurityAuditDashboard.Api.Data.Entities;

namespace SecurityAuditDashboard.Api.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id);
    Task<Role?> GetByNameAsync(string name);
    Task<List<Role>> GetAllWithClaimsAsync();
}