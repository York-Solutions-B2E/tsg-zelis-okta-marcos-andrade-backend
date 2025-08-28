using System.ComponentModel.DataAnnotations;

namespace SecurityAuditDashboard.Api.Data.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(200)]
    public string ExternalId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
    
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    
    public ICollection<SecurityEvent> AuthorEvents { get; set; } = new List<SecurityEvent>();
    public ICollection<SecurityEvent> AffectedEvents { get; set; } = new List<SecurityEvent>();
}