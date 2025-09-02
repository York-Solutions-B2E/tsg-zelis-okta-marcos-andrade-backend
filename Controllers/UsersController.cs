using Microsoft.AspNetCore.Mvc;
using SecurityAuditDashboard.Api.Services.Interfaces;
using SecurityAuditDashboard.Shared.DTOs;

namespace SecurityAuditDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ISecurityEventService _securityEventService;

    public UsersController(
        IUserService userService,
        ISecurityEventService securityEventService)
    {
        _userService = userService;
        _securityEventService = securityEventService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.ExternalId))
        {
            return BadRequest("Invalid login request");
        }

        // Create or update user
        var user = await _userService.CreateOrUpdateUserAsync(
            request.Email,
            request.ExternalId,
            request.Name ?? request.Email,
            request.Provider);

        // Log the login event
        await _securityEventService.LogLoginSuccessAsync(
            user.Id,
            request.Provider);

        // Return user info with role
        var response = new
        {
            Id = user.Id.ToString(),
            Email = user.Email,
            Role = user.Role?.Name ?? "BasicUser"
        };

        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
    {
        if (request == null || request.UserId == Guid.Empty)
        {
            return BadRequest("Invalid logout request");
        }

        // Log the logout event
        await _securityEventService.LogLogoutAsync(request.UserId);

        return Ok();
    }
}