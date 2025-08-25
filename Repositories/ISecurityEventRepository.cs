using SecurityAuditDashboard.Api.Data.Entities;

namespace SecurityAuditDashboard.Api.Repositories;

public interface ISecurityEventRepository
{
    Task<SecurityEvent> CreateAsync(SecurityEvent securityEvent);
    Task<List<SecurityEvent>> GetAuthEventsAsync();
    Task<List<SecurityEvent>> GetRoleChangeEventsAsync();
    Task<List<SecurityEvent>> GetAllAsync();
}