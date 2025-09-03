using SecurityAuditDashboard.Api.Repositories.Interfaces;
using SecurityAuditDashboard.Api.Services.Interfaces;
using SecurityAuditDashboard.Shared.DTOs;
using SecurityAuditDashboard.Shared.Constants;
using HotChocolate;
using HotChocolate.Authorization;

namespace SecurityAuditDashboard.Api.GraphQL.Mutations;

/// <summary>
/// GraphQL Mutation root type
/// </summary>
public class Mutation
{
    /// <summary>
    /// Assign a role to a user (requires CanViewRoleChanges permission)
    /// </summary>
    [Authorize(Policy = "CanViewRoleChanges")]
    public async Task<AssignUserRoleResultDto> AssignUserRole(
        AssignUserRoleInputDto input,
        [Service] IUserRepository userRepository,
        [Service] IRoleRepository roleRepository,
        [Service] ISecurityEventRepository securityEventRepository)
    {
        try
        {
            var user = await userRepository.GetByIdAsync(input.UserId);
            if (user == null)
            {
                return new AssignUserRoleResultDto
                {
                    Success = false,
                    Message = "User not found"
                };
            }

            var role = await roleRepository.GetByIdAsync(input.RoleId);
            if (role == null)
            {
                return new AssignUserRoleResultDto
                {
                    Success = false,
                    Message = "Role not found"
                };
            }

            // Store previous role before updating
            var previousRoleId = user.RoleId;

            // Update user role
            user.RoleId = input.RoleId;
            await userRepository.UpdateAsync(user);

            // Log security event
            var securityEvent = new Data.Entities.SecurityEvent
            {
                AuthorUserId = user.Id, // Assuming the user is assigning their own role for now
                AffectedUserId = user.Id,
                EventType = EventTypes.RoleAssigned,
                Details = $"User role changed to {role.Name}",
                OccurredUtc = DateTime.UtcNow,
                PreviousRoleId = previousRoleId,
                NewRoleId = input.RoleId
            };

            await securityEventRepository.CreateAsync(securityEvent);

            return new AssignUserRoleResultDto
            {
                Success = true,
                Message = "Role assigned successfully",
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    RoleId = user.RoleId,
                    Role = new RoleDto
                    {
                        Id = role.Id,
                        Name = role.Name,
                        Description = role.Description
                    }
                },
                SecurityEvent = new SecurityEventDto
                {
                    Id = securityEvent.Id,
                    EventType = securityEvent.EventType,
                    OccurredUtc = securityEvent.OccurredUtc,
                    Details = securityEvent.Details ?? string.Empty,
                    UserEmail = user.Email,
                    ActorEmail = user.Email, // Assuming self-assignment for now
                    TargetEmail = user.Email
                }
            };
        }
        catch (Exception ex)
        {
            return new AssignUserRoleResultDto
            {
                Success = false,
                Message = $"Error assigning role: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Log user logout event (requires authentication)
    /// </summary>
    [Authorize]
    public async Task<LogoutResultDto> LogLogout(
        Guid userId,
        [Service] ISecurityEventService securityEventService)
    {
        try
        {
            await securityEventService.LogLogoutAsync(userId);
            return new LogoutResultDto
            {
                Success = true,
                Message = "Logout event logged successfully"
            };
        }
        catch (Exception ex)
        {
            return new LogoutResultDto
            {
                Success = false,
                Message = $"Error logging logout: {ex.Message}"
            };
        }
    }
}
