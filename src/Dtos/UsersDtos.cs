using System.ComponentModel.DataAnnotations;

namespace WaitifyApi.Dtos;

public class AdminDeleteUserRequest
{
    [Required(ErrorMessage = "L'Id est obligatoire !")]
    public string UserId { get; set; } = string.Empty;
}

public record AdminDeleteUserResponse {
    public bool Success { get; set; } = false;
    public string? Message { get; set; }
}
