using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WaitifyApi.Entities;

[Table("SubscriptionPlans")]
public class SubscriptionPlan
{
    public Guid Id { get; set; }

    [Column(TypeName = "varchar(100)")]
    [Required]
    public string Name { get; set; } = string.Empty;

    public int PriceCents { get; set; }

    /// <summary>-1 for unlimited</summary>
    public int MaxBusinesses { get; set; } = 1;

    public int SmsQuotaMonthly { get; set; } = 1000;

    [Column(TypeName = "jsonb")]
    public string? Features { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
