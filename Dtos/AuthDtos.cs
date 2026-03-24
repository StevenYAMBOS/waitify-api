using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WaitifyApi.Enums;

namespace WaitifyApi.Models;

public class RegisterRequest
{
    [EmailAddress]
    [Required(ErrorMessage = "Ajouter une adresse email valide.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Le prénom doit être compris entre 2 100 caractères.")]
    [MaxLength(100)]
    [MinLength(2)]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Le nom de famille doit être compris entre 2 100 caractères.")]
    [MaxLength(100)]
    [MinLength(2)]
    public string? LastName { get; set; }

    public string? ProfilePicture { get; set; }

    [Required]
    [PasswordPropertyText]
    public string? Password { get; set; }

    [Required]
    public string? Role { get; set; }
};

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    [PasswordPropertyText]
    public string? Password { get; set; }
}

public class AuthResponse
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Token { get; set; }
}

public class UpdateProfilDTO
{
    public string? Id { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public string? Username { get; set; }
}

public class TokenResponseDTO
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}

public class RefreshTokenRequestDTO
{
    public required string UserId { get; set; }
    public required string RefreshToken { get; set; }
}