using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WaitifyApi.Enums;

namespace WaitifyApi.Entities;

[Table("businesses")]
public class Business
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("owner_id")]
    public string? OwnerId { get; set; }
    public virtual ApplicationUser? Owner { get; set; }

    [Column("name", TypeName = "varchar(255)")]
    [Required]
    public string? Name { get; set; }

    [Column("business_type", TypeName = "varchar(100)")]
    [Required]
    public string? BusinessType { get; set; }

    [Column("phone_number", TypeName = "varchar(20)")]
    public string? PhoneNumber { get; set; }

    [Column("logo", TypeName = "varchar(255)")]
    public string? Logo { get; set; }

    [Column("address", TypeName = "text")]
    public string? Address { get; set; }

    [Column("city", TypeName = "varchar(100)")]
    public string? City { get; set; }

    [Column("zip_code", TypeName = "varchar(10)")]
    public string? ZipCode { get; set; }

    [Column("country", TypeName = "varchar(50)")]
    public string Country { get; set; } = "France";

    [Column("qr_code_token", TypeName = "varchar(255)")]
    [Required]
    public string QrCodeToken { get; set; } = string.Empty;

    [Column("average_service_time")]
    public int AverageServiceTime { get; set; } = 300;

    [Column("is_queue_active")]
    public bool IsQueueActive { get; set; } = false;

    [Column("is_queue_paused")]
    public bool IsQueuePaused { get; set; } = false;

    [Column("max_queue_size")]
    public int MaxQueueSize { get; set; } = 50;

    [Column("opening_hours", TypeName = "jsonb")]
    public string? OpeningHours { get; set; }

    [Column("custom_message", TypeName = "text")]
    public string? CustomMessage { get; set; }

    [Column("sms_notifications_enabled")]
    public bool SmsNotificationsEnabled { get; set; } = true;

    [Column("auto_advance_enabled")]
    public bool AutoAdvanceEnabled { get; set; } = true;

    [Column("client_timeout_minutes")]
    public int ClientTimeoutMinutes { get; set; } = 5;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}