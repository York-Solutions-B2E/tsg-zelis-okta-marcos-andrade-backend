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
    [Authorize(Policy = "CanViewAuthEvents")]
    public async Task<IActionResult> GetAuthEvents()
    {
        var events = await _securityEventService.GetAuthenticationEventsAsync();
        return Ok(events);
    }

    [HttpGet("roles")]
    [Authorize(Policy = "CanViewRoleChanges")]
    public async Task<IActionResult> GetRoleEvents()
    {

        var events = await _securityEventService.GetRoleChangeEventDtosAsync();
        return Ok(events);
    }
}