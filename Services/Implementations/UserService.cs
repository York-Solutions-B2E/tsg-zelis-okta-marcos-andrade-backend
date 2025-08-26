using SecurityAuditDashboard.Api.Data.Entities;
using SecurityAuditDashboard.Api.Repositories.Interfaces;

using SecurityAuditDashboard.Api.Services.Interfaces;
namespace SecurityAuditDashboard.Api.Services.Implementations;

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

    public async Task<bool> AssignRoleAsync(Guid userId, Guid roleId, Guid authorUserId)
    {
        var user = await _userRepository.GetByIdWithRoleAndClaimsAsync(userId);
        if (user == null)
            return false;

        var oldRoleName = user.Role.Name;
        
        var newRole = await _roleRepository.GetByIdAsync(roleId);
        if (newRole == null)
            return false;

        user.RoleId = roleId;
        await _userRepository.UpdateAsync(user);

        await _securityEventService.LogRoleAssignedAsync(
            authorUserId, 
            userId, 
            oldRoleName, 
            newRole.Name
        );
        
        return true;
    }
    
    public async Task<IEnumerable<object>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(u => new
        {
            u.Id,
            u.Email,
            CurrentRole = u.Role?.Name ?? "Unknown"
        });
    }
}