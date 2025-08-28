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
    private readonly IWebHostEnvironment _environment;

    public RolesController(
        IRoleRepository roleRepository,
        IUserService userService,
        ISecurityEventService securityEventService,
        IWebHostEnvironment environment)
    {
        _roleRepository = roleRepository;
        _userService = userService;
        _securityEventService = securityEventService;
        _environment = environment;
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
    [Authorize(Policy = "CanViewRoleChanges")]
    public IActionResult CheckAccess()
    {
        return Ok();
    }

    [HttpPost("make-me-admin")]
    public async Task<IActionResult> MakeMeAdmin()
    {
        // For dev - just hardcode your email
        var email = "MandradeC@yorksolutions.net";
        
        // Get the user by email
        var userRepo = HttpContext.RequestServices.GetRequiredService<IUserRepository>();
        var user = await userRepo.GetByEmailAsync(email);
        
        if (user == null)
        {
            return BadRequest($"User with email {email} not found in database");
        }

        // Get the SecurityAuditor role
        var roles = await _roleRepository.GetAllAsync();
        var securityAuditorRole = roles.FirstOrDefault(r => r.Name == "SecurityAuditor");
        
        if (securityAuditorRole == null)
        {
            return BadRequest("SecurityAuditor role not found");
        }

        // Update the user's role
        var result = await _userService.AssignRoleAsync(user.Id, securityAuditorRole.Id, user.Id);
        
        if (result)
        {
            return Ok(new { message = $"Successfully upgraded {user.Email} to SecurityAuditor role" });
        }

        return BadRequest("Failed to upgrade role");
    }

    [HttpPost("assign")]
    [Authorize(Policy = "CanViewRoleChanges")]
    public async Task<IActionResult> AssignRole([FromBody] RoleAssignmentDto dto)
    {

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