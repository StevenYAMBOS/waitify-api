using Microsoft.AspNetCore.Identity;

namespace WaitifyApi;

public class ApplicationUser : IdentityUser
{
    public string? GoogleId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfilePicture { get; set; }
    public string? AuthProvider { get; set; }
    public string? Role { get; set; }
    public string? SubsriptionId { get; set; }
    public string? SubsriptionStatus { get; set; }
    public DateTime TrialEndsAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLogin { get; set; } = DateTime.UtcNow;
    public DateOnly Date { get; set; }
}
