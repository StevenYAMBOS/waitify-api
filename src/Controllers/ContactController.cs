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
