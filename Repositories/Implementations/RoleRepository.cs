using Microsoft.EntityFrameworkCore;
using SecurityAuditDashboard.Api.Data.Context;
using SecurityAuditDashboard.Api.Data.Entities;

using SecurityAuditDashboard.Api.Repositories.Interfaces;
namespace SecurityAuditDashboard.Api.Repositories.Implementations;

public class RoleRepository : IRoleRepository
{
    private readonly AuditDbContext _context;

    public RoleRepository(AuditDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(Guid id)
    {
        return await _context.Roles
            .Include(r => r.RoleClaims)
            .ThenInclude(rc => rc.Claim)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles
            .Include(r => r.RoleClaims)
            .ThenInclude(rc => rc.Claim)
            .FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<List<Role>> GetAllWithClaimsAsync()
    {
        return await _context.Roles
            .Include(r => r.RoleClaims)
            .ThenInclude(rc => rc.Claim)
            .ToListAsync();
    }
    
    public async Task<List<Role>> GetAllAsync()
    {
        return await _context.Roles.ToListAsync();
    }
}