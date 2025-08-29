using SecurityAuditDashboard.Api.Repositories.Interfaces;
using SecurityAuditDashboard.Shared.DTOs;
using SecurityAuditDashboard.Shared.Constants;
using HotChocolate;

namespace SecurityAuditDashboard.Api.GraphQL.Mutations;

/// <summary>
/// GraphQL Mutation root type
/// This is a placeholder - will be implemented in Task 3
/// </summary>
public class Mutation
{
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
                    CurrentRole = role.Name
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
}
