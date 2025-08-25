using SecurityAuditDashboard.Api.Data.Entities;
using SecurityAuditDashboard.Api.Repositories;

namespace SecurityAuditDashboard.Api.Services;

public class SecurityEventService : ISecurityEventService
{
    private readonly ISecurityEventRepository _securityEventRepository;

    public SecurityEventService(ISecurityEventRepository securityEventRepository)
    {
        _securityEventRepository = securityEventRepository;
    }

    public async Task LogLoginSuccessAsync(Guid userId, string provider)
    {
        var securityEvent = new SecurityEvent
        {
            EventType = "LoginSuccess",
            AuthorUserId = userId,
            AffectedUserId = userId,
            Details = $"provider={provider}",
            OccurredUtc = DateTime.UtcNow
        };

        await _securityEventRepository.CreateAsync(securityEvent);
    }

    public async Task LogLogoutAsync(Guid userId)
    {
        var securityEvent = new SecurityEvent
        {
            EventType = "Logout",
            AuthorUserId = userId,
            AffectedUserId = userId,
            Details = "local sign-out",
            OccurredUtc = DateTime.UtcNow
        };

        await _securityEventRepository.CreateAsync(securityEvent);
    }

    public async Task LogRoleAssignedAsync(Guid authorUserId, Guid affectedUserId, string fromRole, string toRole)
    {
        var securityEvent = new SecurityEvent
        {
            EventType = "RoleAssigned",
            AuthorUserId = authorUserId,
            AffectedUserId = affectedUserId,
            Details = $"from={fromRole} to={toRole}",
            OccurredUtc = DateTime.UtcNow
        };

        await _securityEventRepository.CreateAsync(securityEvent);
    }

    public async Task<IEnumerable<SecurityEvent>> GetAuthEventsAsync()
    {
        return await _securityEventRepository.GetAuthEventsAsync();
    }

    public async Task<IEnumerable<SecurityEvent>> GetRoleChangeEventsAsync()
    {
        return await _securityEventRepository.GetRoleChangeEventsAsync();
    }
}