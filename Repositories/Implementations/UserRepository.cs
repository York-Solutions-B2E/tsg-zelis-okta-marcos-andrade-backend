using Microsoft.EntityFrameworkCore;
using SecurityAuditDashboard.Api.Data.Context;
using SecurityAuditDashboard.Api.Data.Entities;

using SecurityAuditDashboard.Api.Repositories.Interfaces;
namespace SecurityAuditDashboard.Api.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly AuditDbContext _context;

    public UserRepository(AuditDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Role)
            .ThenInclude(r => r.RoleClaims)
            .ThenInclude(rc => rc.Claim)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByExternalIdAsync(string externalId)
    {
        return await _context.Users
            .Include(u => u.Role)
            .ThenInclude(r => r.RoleClaims)
            .ThenInclude(rc => rc.Claim)
            .FirstOrDefaultAsync(u => u.ExternalId == externalId);
    }

    public async Task<User?> GetByIdWithRoleAndClaimsAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.Role)
            .ThenInclude(r => r.RoleClaims)
            .ThenInclude(rc => rc.Claim)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<List<User>> GetAllWithRolesAsync()
    {
        return await _context.Users
            .Include(u => u.Role)
            .ToListAsync();
    }
    
    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Role)
            .ToListAsync();
    }
    
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}