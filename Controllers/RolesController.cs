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

    // Note: GetRoles and role assignment now handled via GraphQL queries/mutations
    // Only keeping dev endpoint for initial admin setup

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

    // Note: Role assignment now handled via GraphQL assignUserRole mutation
}