using Microsoft.Extensions.Logging;
using SecurityAuditDashboard.Api.Data.Entities;
using SecurityAuditDashboard.Api.Repositories.Interfaces;
using SecurityAuditDashboard.Api.Services.Interfaces;
namespace SecurityAuditDashboard.Api.Services.Implementations;

public class SecurityEventService : ISecurityEventService
{
    private readonly ISecurityEventRepository _securityEventRepository;
    private readonly ILogger<SecurityEventService> _logger;

    public SecurityEventService(ISecurityEventRepository securityEventRepository, ILogger<SecurityEventService> logger)
    {
        _securityEventRepository = securityEventRepository;
        _logger = logger;
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
    
    public async Task<IEnumerable<object>> GetAuthenticationEventsAsync()
    {
        var events = await _securityEventRepository.GetAuthEventsAsync();
        return events.Select(e => new
        {
            e.Id,
            e.EventType,
            e.OccurredUtc,
            e.Details,
            UserEmail = e.AffectedUser?.Email ?? "Unknown"
        });
    }
    
    public async Task<IEnumerable<object>> GetRoleChangeEventDtosAsync()
    {
        var events = await _securityEventRepository.GetRoleChangeEventsAsync();
        return events.Select(e => new
        {
            e.Id,
            e.EventType,
            e.OccurredUtc,
            e.Details,
            ActorEmail = e.AuthorUser?.Email ?? "Unknown",
            TargetEmail = e.AffectedUser?.Email ?? "Unknown"
        });
    }
}