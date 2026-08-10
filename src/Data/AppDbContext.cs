using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WaitifyApi.Entities;

namespace WaitifyApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{

    public DbSet<Business> Businesses { get; set; }
    public DbSet<QueueEntries> Queues { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<SmsLog> SmsLogs { get; set; }
    public DbSet<Contact> Contacts { get; set; }

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
    // optionsBuilder.LogTo(Console.WriteLine);

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(u => u.Role).HasConversion<string>();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.SubsriptionId);
            entity.HasIndex(u => u.SubsriptionStatus);
        });

        builder.Entity<IdentityRole>(entity =>
        {
            entity.ToTable("Roles");
        });

        builder.Entity<Business>(entity =>
        {
            entity.HasOne(b => b.Owner)
                .WithMany(o => o.Businesses)
                .HasForeignKey(b => b.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(b => b.OwnerId);
            entity.HasIndex(b => b.QrCodeToken).IsUnique();
            entity.HasIndex(b => b.IsActive);
        });

        builder.Entity<QueueEntries>(entity =>
        {
            entity.HasOne(q => q.Business)
                .WithMany()
                .HasForeignKey(q => q.BusinessQrCodeToken)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(q => q.BusinessQrCodeToken);
            entity.HasIndex(q => q.Status);
            // entity.HasIndex(q => new { q.BusinessQrCodeToken, q.Status });

            entity.Property(q => q.Status).HasDefaultValue("waiting");

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_QueueEntries_Status",
                "\"Status\" IN ('waiting', 'called', 'served', 'missed', 'cancelled')"
            ));
        });

        builder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasIndex(s => s.Name).IsUnique();

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_SubscriptionPlans_MaxBusinesses",
                "\"MaxBusinesses\" = -1 OR \"MaxBusinesses\" > 0"
            ));
        });

        builder.Entity<SmsLog>(entity =>
        {
            entity.HasOne(s => s.Business)
                .WithMany()
                .HasForeignKey(s => s.BusinessQrCodeToken)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.QueueEntry)
                .WithMany()
                .HasForeignKey(s => s.QueueEntryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(s => s.BusinessQrCodeToken);
            entity.HasIndex(s => s.QueueEntryId);
            entity.HasIndex(s => s.Status);
            entity.HasIndex(s => s.SentAt);

            entity.Property(s => s.Status).HasDefaultValue("pending");

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_SmsLogs_Status",
                "\"Status\" IN ('pending', 'sent', 'failed')"
            ));
        });

    }
}
