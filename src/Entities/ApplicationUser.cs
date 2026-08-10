using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using WaitifyApi.Enums;

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

