using System.Security.Claims;
using SecurityAuditDashboard.Api.Data.Entities;
using SecurityAuditDashboard.Api.Repositories;

namespace SecurityAuditDashboard.Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ISecurityEventService _securityEventService;

    public UserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ISecurityEventService securityEventService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _securityEventService = securityEventService;
    }

    public async Task HandleLoginAsync(ClaimsPrincipal principal, string provider)
    {
        var email = principal.FindFirst(ClaimTypes.Email)?.Value 
                    ?? principal.FindFirst("email")?.Value
                    ?? throw new InvalidOperationException("Email claim not found");
        
        var externalId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? principal.FindFirst("sub")?.Value
                         ?? throw new InvalidOperationException("Subject claim not found");

        var user = await _userRepository.GetByExternalIdAsync(externalId);
        
        if (user == null)
        {
            var basicRole = await _roleRepository.GetByNameAsync("BasicUser");
            if (basicRole == null)
                throw new InvalidOperationException("BasicUser role not found");

            user = new User
            {
                Email = email,
                ExternalId = externalId,
                RoleId = basicRole.Id
            };
            
            user = await _userRepository.CreateAsync(user);
        }

        await _securityEventService.LogLoginSuccessAsync(user.Id, provider);
    }

    public async Task AssignRoleAsync(Guid userId, Guid roleId, Guid authorUserId)
    {
        var user = await _userRepository.GetByIdWithRoleAndClaimsAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        var oldRoleName = user.Role.Name;
        
        var newRole = await _roleRepository.GetByIdAsync(roleId);
        if (newRole == null)
            throw new InvalidOperationException("Role not found");

        user.RoleId = roleId;
        await _userRepository.UpdateAsync(user);

        await _securityEventService.LogRoleAssignedAsync(
            authorUserId, 
            userId, 
            oldRoleName, 
            newRole.Name
        );
    }
}