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
    [HttpGet("{businessId}/live")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> GetLiveKpis(Guid businessId)
    {
        var userIdFromFromJwt = await tokenService.GetInformationFromToken(Request.HttpContext, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdFromFromJwt == null)
        {
            logger.LogError("Erreur lors de la récupération de l'utilisateur  : {@0}", userIdFromFromJwt);
            return StatusCode(StatusCodes.Status404NotFound, "Utilisateur introuvable ou accès refusé.");
        }

        var analytics = await analyticsService.GenerateNewQRCodeAsync(analyticsQRCodeToken, userIdFromFromJwt);
        if (analytics == null)
        {
            logger.LogInformation("QRCode non généré : {@0}", analytics);
            return StatusCode(StatusCodes.Status404NotFound, "QRCode non généré.");
        }
        return Ok(analytics);
    }

}
