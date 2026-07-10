using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WaitifyApi.Entities;

[Table("QueueEntries")]
public class QueueEntries
{
    public Guid Id { get; set; }
    public Guid BusinessQrCodeToken { get; set; }
    public virtual Business? Business { get; set; }

    [Column(TypeName = "varchar(20)")]
    [Required]
    public string? Phone { get; set; }

    [Column(TypeName = "varchar(100)")]
    public string? ClientName { get; set; }

    [Required]
    public int Position { get; set; }

    public int EstimatedWaitTime { get; set; }

    [Column(TypeName = "varchar(50)")]
    public string? Status { get; set; }

    public DateTime? CalledAt { get; set; }

    public DateTime? ServedAt { get; set; }

    public int ActualServiceTime { get; set; }

    public int SmsSentCount { get; set; }

    public DateTime? LastSmsSentAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}