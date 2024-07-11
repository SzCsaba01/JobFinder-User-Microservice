using Microsoft.EntityFrameworkCore;
using User.Data.Object.Entities;

namespace User.Data.Access.Data;
public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options){}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserProfileSkillMapping>()
            .HasKey(x => new { x.UserProfileId, x.SkillName });

        modelBuilder.Entity<UserProfileSkillMapping>()
            .HasOne(x => x.UserProfile)
            .WithMany(x => x.Skills)
            .HasForeignKey(x => x.UserProfileId);

        modelBuilder.Entity<UserProfileSkillMapping>()
            .HasOne(x => x.Skill)
            .WithMany(x => x.UserProfiles)
            .HasForeignKey(x => x.SkillName);
    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<RoleEntity> Roles { get; set; }
    public DbSet<UserProfileEntity> UserProfiles { get; set; }
    public DbSet<SkillEntity> Skills { get; set; }
    public DbSet<UserProfileSkillMapping> UserProfileSkillMappings { get; set; }
}
