using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WaitifyApi.Entities;

[Table("Businesses")]
public class Business
{
    public Guid Id { get; set; }
    public string? OwnerId { get; set; }
    public virtual ApplicationUser? Owner { get; set; }
    [Column(TypeName = "varchar(255)")]
    [Required]
    public string? Name { get; set; }
    [Column(TypeName = "varchar(100)")]
    [Required]
    public string? BusinessType { get; set; }
    [Column(TypeName = "varchar(20)")]
    public string? PhoneNumber { get; set; }
    [Column(TypeName = "varchar(255)")]
    public string? Logo { get; set; }
    [Column(TypeName = "text")]
    public string? Address { get; set; }
    [Column(TypeName = "varchar(100)")]
    public string? City { get; set; }
    [Column(TypeName = "varchar(10)")]
    public string? ZipCode { get; set; }
    [Column(TypeName = "varchar(50)")]
    public string Country { get; set; } = "France";
    [Required]
    public Guid QrCodeToken { get; set; }
    public int AverageServiceTime { get; set; } = 300;
    public bool IsQueueActive { get; set; } = false;
    public bool IsQueuePaused { get; set; } = false;
    public int MaxQueueSize { get; set; } = 50;
    [Column(TypeName = "jsonb")]
    public string? OpeningHours { get; set; }
    [Column(TypeName = "text")]
    public string? CustomMessage { get; set; }
    public bool SmsNotificationsEnabled { get; set; } = true;
    public bool AutoAdvanceEnabled { get; set; } = true;
    public int ClientTimeoutMinutes { get; set; } = 5;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}