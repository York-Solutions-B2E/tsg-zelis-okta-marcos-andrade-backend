using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SecurityAuditDashboard.Api.Services;

namespace SecurityAuditDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISecurityEventService _securityEventService;

    public AuthController(ISecurityEventService securityEventService)
    {
        _securityEventService = securityEventService;
    }

    [HttpGet("signin/{provider}")]
    public IActionResult SignIn(string provider)
    {
        var redirectUrl = Url.Action(nameof(SignInCallback), "Auth");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, provider);
    }

    [HttpGet("signin-callback")]
    public IActionResult SignInCallback()
    {
        return Ok(new { message = "Sign in successful" });
    }

    [HttpPost("signout")]
    public new async Task<IActionResult> SignOut()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            if (Guid.TryParse(userId, out var userGuid))
            {
                await _securityEventService.LogLogoutAsync(userGuid);
            }
        }

        await HttpContext.SignOutAsync();
        return Ok(new { message = "Sign out successful" });
    }
}