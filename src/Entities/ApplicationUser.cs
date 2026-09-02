using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using WaitifyApi.Enums;
namespace ASPNETCoreIdentityDemo.ViewModels;

namespace WaitifyApi.Entities;

public class ApplicationUser : IdentityUser
{
    [Column(TypeName = "varchar(100)")]
    public string? FirstName { get; set; }

    [Column(TypeName = "varchar(100)")]
    public string? LastName { get; set; }

    [Column(TypeName = "varchar(200)")]
    public string? ProfilePicture { get; set; }

    public Role Role { get; set; } = Role.Owner;

    public string? SubsriptionId { get; set; }
    public string? SubsriptionStatus { get; set; }
    public DateTime TrialEndsAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLogin { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public virtual ICollection<Business> Businesses { get; set; } = [];
    public string? GoogleId { get; set; }
    public string? AuthProvider { get; set; }
}

public class PeopleApiPhotos
{

    public string resourceName { get; set; }
    public string etag { get; set; }
    public List<Photo> photos { get; set; }

    public class Source
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class Metadata
    {
        public bool primary { get; set; }
        public Source source { get; set; }
    }

    public class Photo
    {
        public Metadata metadata { get; set; }
        public string url { get; set; }
    }
}

public record ForgotPassword
{
    [Required(ErrorMessage = "Champ email obligatoire.")]
    [EmailAddress(ErrorMessage = "Format invalide.")]
    public string Email { get; set; } = null!;
}


public class ResetPassword
{
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid Email address.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password, ErrorMessage = "Invalid Password format.")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Please confirm your password.")]
    [DataType(DataType.Password, ErrorMessage = "Invalid Password format.")]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "Password and Confirm Password must match.")]
    public string ConfirmPassword { get; set; } = null!;

    [Required(ErrorMessage = "The password reset token is required.")]
    public string Token { get; set; } = null!;

}
