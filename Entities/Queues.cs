using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WaitifyApi.Entities;

[Table("queue_entries")]
public class Queues
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("business_id")]
    [Required]
    public string? BusinessId { get; set; }
    public virtual Business? Business { get; set; }

    [Column("phone", TypeName = "varchar(20)")]
    [Required]
    public string? Phone { get; set; }

    [Column("client_name", TypeName = "varchar(100)")]
    public string? ClientName { get; set; }

    [Column("position")]
    [Required]
    public int Position { get; set; }

    [Column("estimated_wait_time")]
    public int EstimatedWaitTime { get; set; }

    [Column("status", TypeName = "varchar(50)")]
    public string? Status { get; set; }

    [Column("called_at")]
    public DateTime? CalledAt { get; set; }

    [Column("served_at")]
    public DateTime? ServedAt { get; set; }

    [Column("actual_service_time")]
    public int ActualServiceTime { get; set; }

    [Column("sms_sent_count")]
    public int SmsSentCount { get; set; }

    [Column("last_sms_sent_at")]
    public DateTime? LastSmsSentAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}