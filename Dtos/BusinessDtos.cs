using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WaitifyApi.Models;

public class BusinessRequest
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
    public string QrCodeToken { get; set; } = string.Empty;
};
