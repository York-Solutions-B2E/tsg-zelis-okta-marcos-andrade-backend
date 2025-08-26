using Microsoft.EntityFrameworkCore;
using SecurityAuditDashboard.Api.Data.Context;
using SecurityAuditDashboard.Api.Data.Entities;

using SecurityAuditDashboard.Api.Repositories.Interfaces;
namespace SecurityAuditDashboard.Api.Repositories.Implementations;

public class SecurityEventRepository : ISecurityEventRepository
{
    private readonly AuditDbContext _context;

    public SecurityEventRepository(AuditDbContext context)
    {
        _context = context;
    }

    public async Task<SecurityEvent> CreateAsync(SecurityEvent securityEvent)
    {
        _context.SecurityEvents.Add(securityEvent);
        await _context.SaveChangesAsync();
        return securityEvent;
    }

    public async Task<List<SecurityEvent>> GetAuthEventsAsync()
    {
        return await _context.SecurityEvents
            .Include(e => e.AuthorUser)
            .Include(e => e.AffectedUser)
            .Where(e => e.EventType == "LoginSuccess" || e.EventType == "Logout")
            .OrderByDescending(e => e.OccurredUtc)
            .ToListAsync();
    }

    public async Task<List<SecurityEvent>> GetRoleChangeEventsAsync()
    {
        return await _context.SecurityEvents
            .Include(e => e.AuthorUser)
            .Include(e => e.AffectedUser)
            .Where(e => e.EventType == "RoleAssigned")
            .OrderByDescending(e => e.OccurredUtc)
            .ToListAsync();
    }

    public async Task<List<SecurityEvent>> GetAllAsync()
    {
        return await _context.SecurityEvents
            .Include(e => e.AuthorUser)
            .Include(e => e.AffectedUser)
            .OrderByDescending(e => e.OccurredUtc)
            .ToListAsync();
    }
}