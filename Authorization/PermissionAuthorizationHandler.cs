using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
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
        // Look for external ID first (most reliable)
        var externalId = context.User.FindFirst("uid")?.Value  // Okta uses 'uid'
                        ?? context.User.FindFirst("sub")?.Value  // Standard OIDC uses 'sub'
                        ?? context.User.FindFirst("oid")?.Value  // Microsoft short form (if mapped)
                        ?? context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value  // Microsoft full schema
                        ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Falls back to nameidentifier

        if (string.IsNullOrEmpty(externalId))
        {
            var logger = _serviceProvider.GetService<ILogger<PermissionAuthorizationHandler>>();
            logger?.LogWarning("No external ID claim found. Available claims: {Claims}", 
                string.Join(", ", context.User.Claims.Select(c => c.Type)));
            context.Fail();
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var user = await userRepo.GetByExternalIdAsync(externalId);

        if (user?.Role?.RoleClaims == null)
        {
            var logger = _serviceProvider.GetService<ILogger<PermissionAuthorizationHandler>>();
            logger?.LogWarning("User not found or has no role/claims. ExternalId: {ExternalId}, UserFound: {UserFound}, Role: {Role}", 
                externalId, user != null, user?.Role?.Name);
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

