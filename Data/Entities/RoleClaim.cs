namespace SecurityAuditDashboard.Api.Data.Entities;

public class RoleClaim
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    
    public Guid ClaimId { get; set; }
    public Claim Claim { get; set; } = null!;
}