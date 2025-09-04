using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Authorization;
using SecurityAuditDashboard.Api.Data.Entities;
using SecurityAuditDashboard.Api.Repositories.Interfaces;
using System.Security.Claims;

namespace SecurityAuditDashboard.Api.GraphQL.Queries;

/// <summary>
/// GraphQL Query root type
/// </summary>
public class Query
{
    /// <summary>
    /// Get all users with their roles (requires CanViewAuthEvents permission)
    /// </summary>
    [Authorize(Policy = "CanViewAuthEvents")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<List<User>> GetUsers([Service] IUserRepository userRepository)
    {
        return await userRepository.GetAllWithRolesAsync();
    }

    /// <summary>
    /// Get a specific user by ID (requires CanViewAuthEvents permission)
    /// </summary>
    [Authorize(Policy = "CanViewAuthEvents")]
    [UseProjection]
    public async Task<User?> GetUser(
        [ID] Guid id,
        [Service] IUserRepository userRepository)
    {
        return await userRepository.GetByIdWithRoleAndClaimsAsync(id);
    }

    /// <summary>
    /// Get all security events (requires CanViewAuthEvents permission)
    /// </summary>
    [Authorize(Policy = "CanViewAuthEvents")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<List<SecurityEvent>> GetSecurityEvents([Service] ISecurityEventRepository securityEventRepository)
    {
        return await securityEventRepository.GetAllAsync();
    }

    /// <summary>
    /// Get security events related to authentication (requires CanViewAuthEvents permission)
    /// </summary>
    [Authorize(Policy = "CanViewAuthEvents")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<List<SecurityEvent>> GetAuthEvents([Service] ISecurityEventRepository securityEventRepository)
    {
        return await securityEventRepository.GetAuthEventsAsync();
    }

    /// <summary>
    /// Get security events related to role changes (requires CanViewRoleChanges permission)
    /// </summary>
    [Authorize(Policy = "CanViewRoleChanges")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<List<SecurityEvent>> GetRoleChangeEvents([Service] ISecurityEventRepository securityEventRepository)
    {
        return await securityEventRepository.GetRoleChangeEventsAsync();
    }

    /// <summary>
    /// Hello world query for testing
    /// </summary>
    public string GetHello() => "Hello GraphQL!";




    /// <summary>
    /// Get all available roles (requires authentication)
    /// </summary>
    [Authorize]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<List<Role>> GetRoles([Service] IRoleRepository roleRepository)
    {
        return await roleRepository.GetAllAsync();
    }

    /// <summary>
    /// Get current user's own information (requires only authentication, no special permissions)
    /// </summary>
    [Authorize]
    public async Task<User?> GetCurrentUser(
        ClaimsPrincipal claimsPrincipal,
        [Service] IUserRepository userRepository)
    {
        // Get current user's email from JWT claims
        var userEmail = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(userEmail))
        {
            return null;
        }

        // Find user by email (any authenticated user can get their own info)
        var users = await userRepository.GetAllWithRolesAsync();
        return users.FirstOrDefault(u => u.Email.Equals(userEmail, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// User permissions for authorization checks
/// </summary>
public class UserPermissions
{
    public bool CanViewAuthEvents { get; set; }
    public bool CanViewRoleChanges { get; set; }
}
