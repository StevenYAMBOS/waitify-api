using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WaitifyApi.Models;
using WaitifyApi.Repositories;

namespace WaitifyApi.Controllers;

[Route("api/[controller]")]
[EnableRateLimiting("fixed")]
[ApiController]
public class QueueController(
    IQueueRepository queueService,
    ILogger<QueueController> logger
) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<IActionResult> FindBusinessById(Guid id)
    {
        var queue = await queueService.FindQueueByIdAsync(id);
        if (queue == null)
        {
            logger.LogInformation("File d'attente avec l'id : `{id}` introuvable", id);
            return StatusCode(StatusCodes.Status404NotFound, "File d'attente introuvable");
        }
        return Ok(queue);
    }

    [HttpPost("join")]
    public async Task<IActionResult> JoinQueue([FromBody] JoinQueueRequest request)
    {
        try
        {
            var result = await queueService.JoinQueueAsync(request);
            return Ok(result);
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
            logger.LogError(ex, "Erreur lors de l'inscription dans la file d'attente.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Une erreur est survenue.");
        }
    }
}
