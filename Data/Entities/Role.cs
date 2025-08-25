using System.ComponentModel.DataAnnotations;

namespace SecurityAuditDashboard.Api.Data.Entities;

public class Role
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<RoleClaim> RoleClaims { get; set; } = new List<RoleClaim>();
}