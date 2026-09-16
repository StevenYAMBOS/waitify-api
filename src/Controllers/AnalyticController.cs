using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using WaitifyApi.Constants;
using WaitifyApi.Entities;
using WaitifyApi.Enums;
using WaitifyApi.Helpers;
using WaitifyApi.Models;
using WaitifyApi.Repositories;
using WaitifyApi.Services;

namespace WaitifyApi.Controllers;

[Route("api/[controller]")]
[EnableRateLimiting("fixed")]
[ApiController]
public class AnalyticsController(
    IAnalyticsRepository analyticsService,
    TokenService tokenService,
    ILogger<AnalyticsController> logger
) : ControllerBase
{
    [HttpGet("{businessQrCodeToken}/live")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> GetLiveKpis(Guid businessQrCodeToken)
    {
        var userIdFromFromJwt = await tokenService.GetInformationFromToken(Request.HttpContext, AppConstants.Authorization.NameIdentifierClaim);
        if (userIdFromFromJwt == null)
        {
            logger.LogError("Erreur lors de la récupération de l'utilisateur  : {@0}", userIdFromFromJwt);
            return StatusCode(StatusCodes.Status404NotFound, "Utilisateur introuvable ou accès refusé.");
        }

        try
        {
            var kpis = await analyticsService.GetLiveKpisAsync(businessQrCodeToken, userIdFromFromJwt);
            logger.LogInformation("KPIs '{@0}' récupérés avec succès.", JsonResponseHelper.JsonConversion(kpis));

            return Ok(kpis);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogError("[ERREUR] Une erreur est survenue : {@0}", ex);
            StatusCode(StatusCodes.Status500InternalServerError);
            return NotFound();
        }
    }
}
