using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using WaitifyApi.Entities;
using WaitifyApi.Helpers;
using WaitifyApi.Models;
using WaitifyApi.Repositories;
using WaitifyApi.Services;

namespace WaitifyApi.Controllers;

[Route("api/admin/business")]
[EnableRateLimiting("fixed")]
[ApiController]
public class AdminBusinessController(
    IAdminBusinessRepository adminBusinessService,
    TokenService tokenService,
    ILogger<BusinessController> logger
) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateBusiness([FromForm] AdminBusinessRequest request)
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

        var business = await adminBusinessService.AdminCreateBusinessAsync(idFromFromJwt, request);

        if (business == null)
        {
            logger.LogError("Erreur lors de la création de l'entreprise.");
            return StatusCode(StatusCodes.Status404NotFound, "Erreur lors de la création de l'entreprise.");
        }

        logger.LogInformation("Entreprise créé : {@0}", JsonResponseHelper.JsonConversion(business));
        return Ok(business);
    }
}