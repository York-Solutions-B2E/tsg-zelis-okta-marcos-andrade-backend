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

    public async Task<User> CreateOrUpdateUserAsync(string email, string externalId, string name, string provider)
    {
        // Check if user exists by external ID
        var user = await _userRepository.GetByExternalIdAsync(externalId);
        
        if (user == null)
        {
            // First time login - create new user with BasicUser role
            var basicRole = await _roleRepository.GetByNameAsync("BasicUser");
            if (basicRole == null)
            {
                throw new InvalidOperationException("BasicUser role not found in database");
            }

            user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                ExternalId = externalId,
                Name = name,
                RoleId = basicRole.Id,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(user);
        }
        else
        {
            // Existing user - update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }

        // Load the role for the return value
        user.Role = await _roleRepository.GetByIdAsync(user.RoleId);
        
        return user;
    }
}