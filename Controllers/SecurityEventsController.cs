using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityAuditDashboard.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace SecurityAuditDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + ",Cookies")]
public class SecurityEventsController : ControllerBase
{
    private readonly ISecurityEventService _securityEventService;

    public SecurityEventsController(ISecurityEventService securityEventService)
    {
        _securityEventService = securityEventService;
    }

    [HttpGet("auth")]
    public async Task<IActionResult> GetAuthEvents()
    {
        // Check if user has Audit.ViewAuthEvents claim
        if (!User.HasClaim("permissions", "Audit.ViewAuthEvents"))
        {
            return Forbid();
        }

        var events = await _securityEventService.GetAuthenticationEventsAsync();
        return Ok(events);
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoleEvents()
    {
        // Check if user has Audit.RoleChanges claim
        if (!User.HasClaim("permissions", "Audit.RoleChanges"))
        {
            return Forbid();
        }

        var events = await _securityEventService.GetRoleChangeEventDtosAsync();
        return Ok(events);
    }
}