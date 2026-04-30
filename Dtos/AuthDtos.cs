using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WaitifyApi.Enums;

namespace WaitifyApi.Models;

public record RegisterRequest
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

    public IFormFile? ProfilePicture { get; set; }

    [PasswordPropertyText]
    [Required]
    public string? Password { get; set; }

    public string? Role { get; set; }
};

public record LoginRequest
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    [PasswordPropertyText]
    public string? Password { get; set; }
}

public record AuthResponse
{
    public string? FistName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Token { get; set; }
}

public record UpdateProfilDTO
{
    public string? Id { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public string? FistName { get; set; }
    public string? LastName { get; set; }
}

public record TokenResponseDTO
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}

public record RefreshTokenRequestDTO
{
    public required string UserId { get; set; }
    public required string RefreshToken { get; set; }
}

public record GoogleLoginRequest
{
    public string? IdToken { get; set; }
}