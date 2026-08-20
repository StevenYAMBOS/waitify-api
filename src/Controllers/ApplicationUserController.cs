using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WaitifyApi.Constants;
using WaitifyApi.Dtos;
using WaitifyApi.Entities;
using WaitifyApi.Repositories;
using WaitifyApi.Services;
// using Microsoft.AspNetCore.RateLimiting;

namespace WaitifyApi.Controllers;

[ApiController]
[Route("api/user/profile")]
public class ApplicationUserProfileController(TokenService tokenService, IApplicationUserRepository userProfilService, ILogger<ApplicationUserProfileController> logger) : ControllerBase
{
    [HttpGet()]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> GetUserProfil()
    {

        var idFromFromJwt = await tokenService.GetInformationFromToken(Request.HttpContext, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (idFromFromJwt == null)
        {
            logger.LogError("Erreur lors de la récupération de l'utilisateur  : {@0}", idFromFromJwt);
            return StatusCode(StatusCodes.Status404NotFound, "Utilisateur introuvable");
        }

        logger.LogInformation("TOKEN décodé : {@0}", idFromFromJwt);

        var user = await userProfilService.FindUserByIdAsync(idFromFromJwt);
        if (user == null)
        {
            logger.LogError("Utilisateur introuvable.");
            return StatusCode(StatusCodes.Status404NotFound, "Utilisateur introuvable");
        }

        logger.LogInformation("Informations utilisateur : {@0}", JsonConvert.SerializeObject(user, Formatting.Indented));
        return Ok(user);
    }

    [HttpPatch()]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> UpdateUserProfil([FromBody] JsonPatchDocument<ApplicationUser> patchDocument)
    {
        if (patchDocument == null)
        {
            logger.LogError("La requête est nulle : {@0}", patchDocument);
            return BadRequest();
        }

        var idFromFromJwt = await tokenService.GetInformationFromToken(Request.HttpContext, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (idFromFromJwt == null)
        {
            logger.LogError("Erreur lors de la récupération de l'utilisateur  : {@0}", idFromFromJwt);
            return StatusCode(StatusCodes.Status404NotFound, "Utilisateur introuvable");
        }

        var (success, existingUser, error) = await userProfilService.UpdateProfilAsync(idFromFromJwt, patchDocument);

        if (!success)
        {
            logger.LogError("Une erreur est sruvenue");
            return NotFound(new { error });
        }

        logger.LogInformation("Utilisateur mis à jour avec succès : {@0}", existingUser);
        return Ok(existingUser);
    }

    [HttpDelete()]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> DeleteUserProfile()
    {
        var idFromFromJwt = await tokenService.GetInformationFromToken(Request.HttpContext, "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (idFromFromJwt == null)
        {
            logger.LogError("Erreur lors de la récupération de l'utilisateur  : {@0}", idFromFromJwt);
            return StatusCode(StatusCodes.Status404NotFound, "Utilisateur introuvable");
        }

        try
        {
            logger.LogInformation("Utilisateur '{0}' supprimé avec succès.", idFromFromJwt);
            await userProfilService.DeleteProfilAsync(idFromFromJwt);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            StatusCode(StatusCodes.Status500InternalServerError);
            return NotFound();
        }
    }

    [HttpDelete("admin")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> AdminDeleteUserProfile(AdminDeleteUserRequest request)
    {
        try
        {
            logger.LogInformation("Utilisateur '{@0}' supprimé avec succès.", request?.UserId);
            await userProfilService.AdminDeleteUserAsync(request.UserId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            StatusCode(StatusCodes.Status500InternalServerError);
            return NotFound();
        }
    }
}
