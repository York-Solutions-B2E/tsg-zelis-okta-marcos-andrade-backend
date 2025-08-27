using System.ComponentModel.DataAnnotations;

namespace SecurityAuditDashboard.Api.Data.Entities;

public class SecurityEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(50)]
    public string EventType { get; set; } = string.Empty;
    
    public Guid AuthorUserId { get; set; }
    public User AuthorUser { get; set; } = null!;
    
    public Guid AffectedUserId { get; set; }
    public User AffectedUser { get; set; } = null!;
    
    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
    
    [MaxLength(400)]
    public string? Details { get; set; }
}