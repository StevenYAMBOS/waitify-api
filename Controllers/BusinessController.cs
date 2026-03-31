using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using WaitifyApi.Models;
using WaitifyApi.Repositories;
using WaitifyApi.Services;

namespace WaitifyApi.Controllers;

[Route("api/[controller]")]
[EnableRateLimiting("fixed")]
[ApiController]
public class BusinessController(
    IBusinessRepository businessService,
    TokenService tokenService,
    ILogger<BusinessController> logger
) : ControllerBase
{

    // [HttpPost("generate:{id}/qrcode")]
    [HttpPost("{id}")]
    public async Task<IActionResult> GenerateNewQRCode(Guid qrCodeToken)
    {
        var business = await businessService.GenerateNewQRCodeAsync(qrCodeToken);
        if (business == null)
        {
            logger.LogInformation("QRCode non généré : {@0}", business);
            return StatusCode(StatusCodes.Status404NotFound, "QRCode non généré.");
        }
        return Ok(business);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBusiness(Guid id)
    {
        var business = await businessService.FindBusinessByIdAsync(id);
        if (business == null)
        {
            logger.LogInformation("Entreprise avec l'id : `{id}` introuvable", id);
            return StatusCode(StatusCodes.Status404NotFound, "Entreprise introuvable");
        }
        logger.LogInformation("ENTREPRISE : {@0}", JsonConvert.SerializeObject(business, Formatting.Indented));
        return Ok(business);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBusiness([FromForm] BusinessRequest request)
    {
        var idFromFromJwt = await tokenService.GetInformationFromToken(Request.HttpContext, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (idFromFromJwt == null)
        {
            logger.LogError("Erreur lors de la récupération de l'utilisateur  : {@0}", idFromFromJwt);
            return StatusCode(StatusCodes.Status404NotFound, "Utilisateur introuvable");
        }

        logger.LogInformation("TOKEN décodé : {@0}", idFromFromJwt);

        if (request.Logo?.Length > 1 * 1024 * 1024)
        {
            return StatusCode(StatusCodes.Status400BadRequest, "La taille du fichier ne doit pas excéder 1MB.");
        }

        var business = await businessService.CreateBusinessAsync(idFromFromJwt, request);

        if (business == null)
        {
            logger.LogError("Erreur lors de la création de l'entreprise.");
            return StatusCode(StatusCodes.Status404NotFound, "Erreur lors de la création de l'entreprise.");
        }

        logger.LogInformation("Entreprise créé : {@0}", JsonConvert.SerializeObject(business, Formatting.Indented));
        return Ok(business);
    }

    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> DeleteOneBusiness(Guid id)
    {
        try
        {
            logger.LogInformation("Entreprise '{@0}' supprimée avec succès.", id);
            await businessService.DeleteBusinessAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            StatusCode(StatusCodes.Status500InternalServerError);
            return NotFound();
        }
    }
}