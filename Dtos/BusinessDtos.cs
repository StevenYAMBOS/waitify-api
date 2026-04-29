using System.ComponentModel.DataAnnotations;

namespace WaitifyApi.Models;

public record BusinessRequest
{
    [Required(ErrorMessage = "Le nom est incorrecte.")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Le type est obligatoire.")]
    public string? BusinessType { get; set; }

    [Required(ErrorMessage = "Le format du numéro de téléphone est incorrecte.")]
    public string? PhoneNumber { get; set; }

    public IFormFile? Logo { get; set; }

    [Required(ErrorMessage = "L'adresse est incorrecte.")]
    public string? Address { get; set; }

    [Required(ErrorMessage = "La ville est obligatoire.")]
    public string? City { get; set; }

    [Required(ErrorMessage = "Le code postale est obligatoire.")]
    public string? ZipCode { get; set; }

    [Required(ErrorMessage = "Le pays est obligatoire.")]
    public string Country { get; set; } = "France";

    [Required(ErrorMessage = "Le QRCode est obligatoire.")]
    public Guid QrCodeToken { get; set; }
};

/* public record UpdateBusinessGeneralInfosRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }

    public string? Country { get; set; }
} */

public record UpdateBusinessLogoRequest
{
    public IFormFile? NewLogoFile { get; set; }
}

public record OpenOrCloseBusinessQueueRequest
{
    public bool IsQueueActive { get; set; }
}

public record BusinessResponseDto
{
    public Guid Id { get; set; }
    public string? OwnerId { get; set; }
    public string? Name { get; set; }
    public string? BusinessType { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Logo { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }
    public string Country { get; set; } = "France";
    public Guid QrCodeToken { get; set; }
    public int AverageServiceTime { get; set; } = 300;
    public bool IsQueueActive { get; set; } = false;
    public bool IsQueuePaused { get; set; } = false;
    public int MaxQueueSize { get; set; } = 50;
    public string? OpeningHours { get; set; }
    public string? CustomMessage { get; set; }
    public bool SmsNotificationsEnabled { get; set; } = true;
    public bool AutoAdvanceEnabled { get; set; } = true;
    public int ClientTimeoutMinutes { get; set; } = 5;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}