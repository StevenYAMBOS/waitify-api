using System.ComponentModel.DataAnnotations;

namespace WaitifyApi.Dtos;

public record AdminDeleteUserRequest
{
    [Required(ErrorMessage = "L'id est obligatoire !")]
    public string UserId { get; set; } = string.Empty;
}

public record AdminDeleteUserResponse {
    public bool Success { get; set; } = false;
    public string? Message { get; set; }
}

public record ForgotPasswordRequestDto
{
    [Required(ErrorMessage = "Champ email obligatoire.")]
    [EmailAddress(ErrorMessage = "Format invalide.")]
    public string Email { get; set; } = string.Empty;
}

public record ResetPasswordRequestDto
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
