using Microsoft.EntityFrameworkCore;
using SecurityAuditDashboard.Api.Data.Entities;

namespace SecurityAuditDashboard.Api.Data.Context;

public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Claim> Claims { get; set; }
    public DbSet<RoleClaim> RoleClaims { get; set; }
    public DbSet<SecurityEvent> SecurityEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.ExternalId);
            
            entity.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.Name).IsUnique();
        });

        // Claim configuration
        modelBuilder.Entity<Claim>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => new { c.Type, c.Value }).IsUnique();
        });

        // RoleClaim configuration (many-to-many)
        modelBuilder.Entity<RoleClaim>(entity =>
        {
            entity.HasKey(rc => new { rc.RoleId, rc.ClaimId });
            
            entity.HasOne(rc => rc.Role)
                .WithMany(r => r.RoleClaims)
                .HasForeignKey(rc => rc.RoleId);
            
            entity.HasOne(rc => rc.Claim)
                .WithMany(c => c.RoleClaims)
                .HasForeignKey(rc => rc.ClaimId);
        });

        // SecurityEvent configuration
        modelBuilder.Entity<SecurityEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OccurredUtc).IsDescending();
            entity.HasIndex(e => e.EventType);
            
            entity.HasOne(e => e.AuthorUser)
                .WithMany(u => u.AuthorEvents)
                .HasForeignKey(e => e.AuthorUserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.AffectedUser)
                .WithMany(u => u.AffectedEvents)
                .HasForeignKey(e => e.AffectedUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Define GUIDs for consistent seeding
        var basicUserRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var authObserverRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var securityAuditorRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        
        var viewAuthEventsClaimId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var roleChangesClaimId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        // Seed Roles
        modelBuilder.Entity<Role>().HasData(
            new Role 
            { 
                Id = basicUserRoleId, 
                Name = "BasicUser", 
                Description = "Default role for all new users with no special permissions" 
            },
            new Role 
            { 
                Id = authObserverRoleId, 
                Name = "AuthObserver", 
                Description = "Can view authentication events" 
            },
            new Role 
            { 
                Id = securityAuditorRoleId, 
                Name = "SecurityAuditor", 
                Description = "Can view all events and manage roles" 
            }
        );

        // Seed Claims
        modelBuilder.Entity<Claim>().HasData(
            new Claim 
            { 
                Id = viewAuthEventsClaimId, 
                Type = "permissions", 
                Value = "Audit.ViewAuthEvents" 
            },
            new Claim 
            { 
                Id = roleChangesClaimId, 
                Type = "permissions", 
                Value = "Audit.RoleChanges" 
            }
        );

        // Seed RoleClaims (mapping)
        modelBuilder.Entity<RoleClaim>().HasData(
            // AuthObserver has ViewAuthEvents
            new RoleClaim 
            { 
                RoleId = authObserverRoleId, 
                ClaimId = viewAuthEventsClaimId 
            },
            // SecurityAuditor has both ViewAuthEvents and RoleChanges
            new RoleClaim 
            { 
                RoleId = securityAuditorRoleId, 
                ClaimId = viewAuthEventsClaimId 
            },
            new RoleClaim 
            { 
                RoleId = securityAuditorRoleId, 
                ClaimId = roleChangesClaimId 
            }
        );
    }
}