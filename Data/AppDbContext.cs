using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WaitifyApi.Entities;

namespace WaitifyApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{

    public DbSet<Business> Businesses { get; set; }
    public DbSet<Queue> Queues { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("users");
            entity.Property(u => u.Role)
                  .HasConversion<string>();
        });

        builder.Entity<IdentityRole>(entity =>
        {
            entity.ToTable("roles");
        });
        builder.Entity<Business>(entity =>
        {
            entity.HasOne(business => business.Owner)
            .WithMany(owner => owner.Businesses)
            .HasForeignKey(business => business.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
        });
    }
}