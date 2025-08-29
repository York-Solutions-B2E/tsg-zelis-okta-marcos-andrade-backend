using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Authorization;
using SecurityAuditDashboard.Api.Data.Entities;
using SecurityAuditDashboard.Api.Repositories.Interfaces;

namespace SecurityAuditDashboard.Api.GraphQL.Queries;

/// <summary>
/// GraphQL Query root type
/// </summary>
public class Query
{
    /// <summary>
    /// Get all users with their roles (requires CanViewAuthEvents permission)
    /// </summary>
    // [Authorize(Policy = "CanViewAuthEvents")] // Temporarily disabled for testing
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
    // [Authorize(Policy = "CanViewAuthEvents")] // Temporarily disabled for testing
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
    // [Authorize(Policy = "CanViewAuthEvents")] // Temporarily disabled for testing
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
    // [Authorize(Policy = "CanViewAuthEvents")] // Temporarily disabled for testing
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
    // [Authorize(Policy = "CanViewRoleChanges")] // Temporarily disabled for testing
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
    /// Get all available roles
    /// </summary>
    // [Authorize] // Will re-enable later for auth
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public async Task<List<Role>> GetRoles([Service] IRoleRepository roleRepository)
    {
        return await roleRepository.GetAllAsync();
    }
}
