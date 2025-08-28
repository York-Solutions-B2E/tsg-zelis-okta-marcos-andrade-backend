using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SecurityAuditDashboard.Api.Repositories.Interfaces;

namespace SecurityAuditDashboard.Api.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider;

    public PermissionAuthorizationHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var email = context.User.FindFirst("email")?.Value
                    ?? context.User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
        {
            context.Fail();
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var user = await userRepo.GetByEmailAsync(email);

        if (user?.Role?.RoleClaims == null)
        {
            context.Fail();
            return;
        }

        var hasPermission = user.Role.RoleClaims
            .Any(rc => rc.Claim.Type == "permissions" && rc.Claim.Value == requirement.Permission);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}

