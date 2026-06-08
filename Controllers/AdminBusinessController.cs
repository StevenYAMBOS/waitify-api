using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WaitifyApi.Helpers;
using WaitifyApi.Models;
using WaitifyApi.Repositories;
using WaitifyApi.Services;

namespace WaitifyApi.Controllers;

[Route("api/admin-business")]
[EnableRateLimiting("fixed")]
[ApiController]
public class AdminBusinessController(
    IAdminBusinessRepository adminBusinessService,
    TokenService tokenService,
    ILogger<BusinessController> logger
) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> AdminCreateBusiness([FromForm] AdminBusinessRequest request)
    {
        var idFromFromJwt = await tokenService.GetInformationFromToken(Request.HttpContext, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (idFromFromJwt == null)
        {
            logger.LogError("Erreur lors de la récupération de l'utilisateur  : {@0}", idFromFromJwt);
            return StatusCode(StatusCodes.Status404NotFound, "Utilisateur introuvable");
        }

        if (request.Logo?.Length > 1 * 1024 * 1024)
        {
            return StatusCode(StatusCodes.Status400BadRequest, "La taille du fichier ne doit pas excéder 1MB.");
        }

        var newBusinessCreated = await adminBusinessService.AdminCreateBusinessAsync(idFromFromJwt, request);

        if (newBusinessCreated == null)
        {
            logger.LogError("Erreur lors de la création de l'entreprise.");
            return StatusCode(StatusCodes.Status404NotFound, "Erreur lors de la création de l'entreprise.");
        }

        logger.LogInformation("Entreprise créé : {@0}", JsonResponseHelper.JsonConversion(newBusinessCreated));
        return Ok(newBusinessCreated);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> AdminGetBusinessById(Guid id)
    {
        var business = await adminBusinessService.AdminFindBusinessByIdAsync(id);
        if (business == null)
        {
            logger.LogInformation("Entreprise avec l'id : `{id}` introuvable", id);
            return StatusCode(StatusCodes.Status404NotFound, "Entreprise introuvable");
        }
        logger.LogInformation("ENTREPRISE : {@0}", JsonResponseHelper.JsonConversion(business));
        return Ok(business);
    }

}