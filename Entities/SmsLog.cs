using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WaitifyApi.Entities;

[Table("SmsLogs")]
public class SmsLog
{
    public Guid Id { get; set; }

    [Required]
    public Guid BusinessId { get; set; }
    public virtual Business? Business { get; set; }

    public Guid? QueueEntryId { get; set; }
    public virtual QueueEntries? QueueEntry { get; set; }

    [Column(TypeName = "varchar(20)")]
    [Required]
    public string? Phone { get; set; }

    [Column(TypeName = "varchar(50)")]
    public string? MessageType { get; set; }

    [Column(TypeName = "text")]
    public string? MessageContent { get; set; }

    /// <summary>pending | sent | failed</summary>
    [Column(TypeName = "varchar(20)")]
    public string Status { get; set; } = "pending";

    [Column(TypeName = "jsonb")]
    public string? ProviderResponse { get; set; }

    public int CostCents { get; set; } = 3;

    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
