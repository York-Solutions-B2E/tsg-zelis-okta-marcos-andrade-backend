using System.ComponentModel.DataAnnotations;

namespace SecurityAuditDashboard.Api.Data.Entities;

public class Claim
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(100)]
    public string Type { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Value { get; set; } = string.Empty;
    
    public ICollection<RoleClaim> RoleClaims { get; set; } = new List<RoleClaim>();
}