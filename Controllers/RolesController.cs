using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityAuditDashboard.Api.Repositories.Interfaces;
using SecurityAuditDashboard.Api.Services.Interfaces;

namespace SecurityAuditDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUserService _userService;
    private readonly ISecurityEventService _securityEventService;

    public RolesController(
        IRoleRepository roleRepository,
        IUserService userService,
        ISecurityEventService securityEventService)
    {
        _roleRepository = roleRepository;
        _userService = userService;
        _securityEventService = securityEventService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _roleRepository.GetAllAsync();
        return Ok(roles.Select(r => new
        {
            r.Id,
            r.Name,
            r.Description
        }));
    }

    [HttpGet("check-access")]
    public IActionResult CheckAccess()
    {
        if (!User.HasClaim("permissions", "Audit.RoleChanges"))
        {
            return Forbid();
        }
        return Ok();
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignRole([FromBody] RoleAssignmentDto dto)
    {
        // Check permission
        if (!User.HasClaim("permissions", "Audit.RoleChanges"))
        {
            return Forbid("You need the Audit.RoleChanges permission to assign roles.");
        }

        if (!Guid.TryParse(dto.UserId, out var userId) || !Guid.TryParse(dto.RoleId, out var roleId))
        {
            return BadRequest("Invalid user or role ID.");
        }

        // Get current user (actor)
        var actorId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
        if (!Guid.TryParse(actorId, out var actorGuid))
        {
            // For now, use the target user as actor if we can't determine the actual actor
            actorGuid = userId;
        }

        var result = await _userService.AssignRoleAsync(userId, roleId, actorGuid);
        if (result)
        {
            return Ok(new { message = "Role assigned successfully." });
        }

        return BadRequest("Failed to assign role.");
    }

    public class RoleAssignmentDto
    {
        public string UserId { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
    }
}