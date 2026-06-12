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
public class BusinessController(
    IBusinessRepository businessService,
    TokenService tokenService,
    ILogger<BusinessController> logger
) : ControllerBase
{
    [HttpPost("generate:{id}/qrcode")]
    public async Task<IActionResult> GenerateNewQRCode(Guid id, Guid qrCodeToken)
    {
        var userIdFromFromJwt = await tokenService.GetInformationFromToken(Request.HttpContext, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdFromFromJwt == null)
        {
            logger.LogError("Erreur lors de la récupération de l'utilisateur  : {@0}", userIdFromFromJwt);
            return StatusCode(StatusCodes.Status404NotFound, "Utilisateur introuvable ou accès refusé.");
        }

        var business = await businessService.GenerateNewQRCodeAsync(id, userIdFromFromJwt, qrCodeToken);
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

    [HttpGet("all")]
    public async Task<IActionResult> GetAllOwnerBusinesses()
    {
        var ownerIdFromToken = await tokenService.GetInformationFromToken(Request.HttpContext, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (ownerIdFromToken == null)
        {
            logger.LogError("Impossible de récupérer l'ID de l'utilisateur depuis le token JWT.");
            return Unauthorized("Utilisateur non authentifié.");
        }

        var businesses = await businessService.GetAllOwnerBusinessesAsync(ownerIdFromToken);
        /*       if (businesses == null)
              {
                  logger.LogInformation("Entreprise avec l'id : `{id}` introuvable");
                  return StatusCode(StatusCodes.Status404NotFound, "Entreprise introuvable");
              } */
        logger.LogInformation("ENTREPRISES : {@0}", JsonConvert.SerializeObject(businesses, Formatting.Indented));
        return Ok(businesses);
    }

    [HttpGet("admin-all")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> GetAllWaitifyBusinesses()
    {
        var ownerIdFromToken = await tokenService.GetInformationFromToken(Request.HttpContext, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (ownerIdFromToken == null)
        {
            logger.LogError("Impossible de récupérer l'ID de l'utilisateur depuis le token JWT.");
            return Unauthorized("Utilisateur non authentifié.");
        }

        var businesses = await businessService.GetAllWaitifyBusinessesAsync(ownerIdFromToken);
        logger.LogInformation("ENTREPRISES WAITIFY : {@0}", JsonResponseHelper.JsonConversion(businesses));
        return Ok(businesses);
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

    [HttpPatch("{id}")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> UpdateBusiness(Guid id, [FromBody] JsonPatchDocument<Business> patchedDocument)
    {
        var ownerIdFromToken = await tokenService.GetInformationFromToken(Request.HttpContext, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (ownerIdFromToken == null)
        {
            logger.LogError("Impossible de récupérer l'ID de l'utilisateur depuis le token JWT.");
            return Unauthorized("Utilisateur non authentifié.");
        }

        if (patchedDocument == null)
        {
            logger.LogError("La requête est nulle : {@0}", patchedDocument);
            return BadRequest();
        }

        var (success, existingUser, error) = await businessService.UpdateBusinessAsync(id, patchedDocument);

        if (!success)
        {
            logger.LogError("Une erreur est sruvenue");
            return NotFound(new { error });
        }

        return Ok(existingUser);
    }

    [HttpPatch("{id}/logo")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> UpdateBusinessLogo(Guid id, [FromForm] UpdateBusinessLogoRequest request)
    {

        // var ownerIdFromToken = await tokenService.GetInformationFromToken(Request.HttpContext, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        // if (ownerIdFromToken == null)
        // {
        //     logger.LogError("Impossible de récupérer l'ID de l'utilisateur depuis le token JWT.");
        //     return Unauthorized("Utilisateur non authentifié.");
        // }

        try
        {
            var business = await businessService.UpdateBusinessLogoAsync(
                // ownerIdFromToken, 
                id,
                request);
            if (business == null)
            {
                logger.LogError("Entreprise {@0} non trouvée.", business?.Id);
                return NotFound("Entreprise non trouvée ou accès refusé.");
            }
            return Ok(business);
        }
        catch (UnauthorizedAccessException)
        {
            // logger.LogWarning("Utilisateur {@0} non autorisé à modifier l'entreprise.", ownerIdFromToken);
            return Forbid("Vous n'êtes pas autorisé à modifier cette entreprise.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la mise à jour du logo de l'entreprise {@0}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Une erreur est survenue.");
        }
    }

    [HttpPatch("{id}/queue")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [EnableRateLimiting("fixed")]
    public async Task<IActionResult> OpenOrCloseBusinessQueue(Guid id, [FromBody] OpenOrCloseBusinessQueueRequest request)
    {
        var ownerIdFromToken = await tokenService.GetInformationFromToken(
            Request.HttpContext,
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

        if (ownerIdFromToken == null)
        {
            logger.LogError("Impossible de récupérer l'ID de l'utilisateur depuis le token JWT.");
            return Unauthorized("Utilisateur non authentifié.");
        }

        try
        {
            var result = await businessService.OpenOrCloseBusinessQueueAsync(id, request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid("Vous n'êtes pas autorisé à modifier cette entreprise.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la mise à jour de la file d'attente {@0}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Une erreur est survenue.");
        }
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