using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using WaitifyApi.Enums;

namespace WaitifyApi.Entities;

public class ApplicationUser : IdentityUser
{
    [Column("first_name", TypeName = "varchar(100)")]
    [MaxLength(100)]
    [Required]
    public string? FirstName { get; set; }

    [Column("last_name", TypeName = "varchar(100)")]
    [MaxLength(100)]
    [Required]
    public string? LastName { get; set; }

    [Column("profile_picture", TypeName = "varchar(200)")]
    [MaxLength(100)]
    public string? ProfilePicture { get; set; }

    [Column("role")]
    [Required]
    public Role Role { get; set; } = Role.Owner;

    [Column("subsription_id")]
    public string? SubsriptionId { get; set; }
    [Column("subsription_status")]
    public string? SubsriptionStatus { get; set; }
    [Column("trial_ends_at")]
    public DateTime TrialEndsAt { get; set; } = DateTime.UtcNow;
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    [Column("last_login")]
    public DateTime? LastLogin { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    // public virtual ICollection<Business> Businesses { get; set; } = [];
    // public string? GoogleId { get; set; }
    // public string? AuthProvider { get; set; }
}
