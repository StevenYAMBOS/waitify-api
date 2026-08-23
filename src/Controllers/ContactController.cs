using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WaitifyApi.Enums;
using WaitifyApi.Models;
using WaitifyApi.Repositories;
using WaitifyApi.Dtos;
using WaitifyApi.Constants;

namespace WaitifyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactController(IContactRepository contactService, ILogger<ContactController> logger) : ControllerBase
{
  [HttpPost]
  [EnableRateLimiting("fixed")]
  public async Task<IActionResult> SendContactForm([FromForm] SendContactInfoRequest request)
  {
    if (request.File?.Length > 1 * 1024 * 1024)
    {
      return StatusCode(StatusCodes.Status400BadRequest, "La taille du fichier ne doit pas excéder 1MB.");
    }

    try
    {
      var formrequest = await contactService.SendContactInfoAsync(request);
      if (formrequest is null)
      {
        logger.LogError("Erreur lors de l'envoie du formulaire : {@0}", request.Subject);
        return Conflict("Erreur lors de l'envoie du formulaire.");
      }

      logger.LogInformation("Formulaire envoyé avec succès.");
      return CreatedAtAction(nameof(SendContactForm), new { id = formrequest.Id }, formrequest);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Erreur lors de l'envoi du formulaire '{@0}'.", request.Subject);
      return StatusCode(StatusCodes.Status500InternalServerError, "Une erreur interne est survenue.");
    }
  }

  [HttpGet("{id}")]
  [Authorize(AuthenticationSchemes = "Bearer", Roles = AppConstants.Roles.Admin)]
  public async Task<IActionResult> FindContactByIdAsync(Guid id)
  {
    var contact = await contactService.FindContactByIdAsync(id);
    if (contact == null)
    {
      logger.LogInformation("Demande avec l'id : `{@0}` introuvable", id);
      return StatusCode(StatusCodes.Status404NotFound, "contact introuvable");
    }
    return Ok(contact);
  }

  [HttpGet("all")]
  [Authorize(AuthenticationSchemes = "Bearer", Roles = AppConstants.Roles.Admin)]
  public async Task<IActionResult> AdminGetAllWaitifyContacts()
  {
    var contacts = await contactService.AdminGetAllWaitifyContactsAsync();
    return Ok(contacts);
  }

[HttpGet("user/{id}")]
[Authorize(AuthenticationSchemes = "Bearer", Roles = AppConstants.Roles.Admin)]
public async Task<ActionResult<AdminGetAllWaitifyContactsResponse>> AdminGetContactsByUser(string id)
{
    /*
    Actuellement :
        Pour récupérer les contacts d'un utilisateur on utilise son `Id` afin de récupérer son email (table `Users` colonne `Email`).
        Table `Contacts` colonne Email, si l'email de l'utilisateur correspond avec l'email dans `Contacts` alors on affiche les demandes.
    Plus tard :
        Créer une table de liaison `UserContacts` avec les colonnes `Id`, `User`, `Contact`.
        Quand une demande sera soumise la table `Contacts` et `UserContacts` seront remplies.

    */
    try
    {
        var contacts = await contactService.AdminFindContactsListByUserAsync(id);
        return contacts;
    }
    catch (KeyNotFoundException ex)
    {
        logger.LogError("Ressource introuvable : {@0}", ex.Message);
        return NotFound(ex.Message);
    }
    catch (InvalidOperationException ex)
    {
        logger.LogError("Opération invalide : {@0}", ex.Message);
        return BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Erreur lors de la récupération des demandes.");
        return StatusCode(StatusCodes.Status500InternalServerError, "Une erreur est survenue.");
    }
}

    [HttpPatch("{id}")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> AdminValidateContact(Guid id)
    {
        try
        {
            var reponse = await contactService.AdminValideContactAsync(id);
            return Ok(reponse);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogError("Ressource introuvable : {@0}", ex.Message);
            return NotFound(ex.Message);
        }
            catch (InvalidOperationException ex)
        {
            logger.LogError("Opération invalide : {@0}", ex.Message);
            return BadRequest(ex.Message);
        }
            catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la validation de la demande.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Une erreur est survenue.");
        }
    }

  [HttpDelete("{id}")]
  [Authorize(AuthenticationSchemes = "Bearer")]
  [EnableRateLimiting("fixed")]
  public async Task<ActionResult<AdminDeleteContactResponse>> DeleteOneContact(Guid id)
  {
        try
        {
            var response = await contactService.AdminDeleteContatAsync(id);

            // logger.LogInformation(response);
            return response;

        }

        catch (KeyNotFoundException)
        {
            StatusCode(StatusCodes.Status500InternalServerError);
            return NotFound();
        }
  }
}
