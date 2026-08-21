using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WaitifyApi.Entities;

[Table("Contacts")]
public class Contact
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("Email", TypeName = "varchar(100)")]
    [Required]
    [MaxLength(200)]
    [EmailAddress]
    public string? Email { get; set; }

    [Column("Subject")]
    [Required]
    [MaxLength(200)]
    public string? Subject { get; set; }

    [Column("Content")]
    [Required]
    [MaxLength(1000)]
    public string? Content { get; set; }

    [Column("File", TypeName = "varchar(200)")]
    [MaxLength(200)]
    public string? File { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
