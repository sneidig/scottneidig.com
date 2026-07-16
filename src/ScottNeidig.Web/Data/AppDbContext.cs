using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ScottNeidig.Web.Data.Entities;

namespace ScottNeidig.Web.Data;

/// <summary>
/// Inherits IdentityDbContext so the Identity tables live in the same database as the
/// site content. One database, one migration history, one connection to manage.
/// </summary>
public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectImage> ProjectImages => Set<ProjectImage>();
    public DbSet<ProjectPoint> ProjectPoints => Set<ProjectPoint>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<SkillGroup> SkillGroups => Set<SkillGroup>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Slugs are URLs. A duplicate would make one of the two pages unreachable,
        // so the database enforces uniqueness rather than trusting app code.
        b.Entity<Project>().HasIndex(p => p.Slug).IsUnique();
        b.Entity<BlogPost>().HasIndex(p => p.Slug).IsUnique();
        b.Entity<Category>().HasIndex(c => c.Slug).IsUnique();

        // Deleting a category should not delete the work that was in it.
        b.Entity<Project>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Projects)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Images and points have no meaning without their project, so they go with it.
        b.Entity<ProjectImage>()
            .HasOne(i => i.Project)
            .WithMany(p => p.Images)
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<ProjectPoint>()
            .HasOne(pt => pt.Project)
            .WithMany(p => p.Points)
            .HasForeignKey(pt => pt.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Skill>()
            .HasOne(s => s.SkillGroup)
            .WithMany(g => g.Skills)
            .HasForeignKey(s => s.SkillGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // The public site only ever reads published rows, and always in display order.
        b.Entity<Project>().HasIndex(p => new { p.Published, p.SortOrder });
        b.Entity<BlogPost>().HasIndex(p => new { p.Published, p.PublishedUtc });
    }
}
