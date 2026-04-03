using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using WaitifyApi.Constants;
using WaitifyApi.Data;
using WaitifyApi.Entities;
using WaitifyApi.Helpers;
using WaitifyApi.Models;
using WaitifyApi.Repositories;

namespace WaitifyApi.Services;

public class QueueService(AppDbContext context, IApplicationUserRepository userService, IBusinessRepository businessService, ILogger<QueueService> logger) : IQueueRepository
{
  public async Task JoinQueueAsync(JoinQueueRequest request)
  {
    var business = await businessService.FindBusinessByIdAsync(request.BusinessId);
    if (business == null)
    {
      logger.LogError("Entreprise non trouvée.\n ID en base de données : `{@0}`.\n ID de la requête : `{@1}`.", business?.Id, request.BusinessId);
      throw new KeyNotFoundException("Entreprise non trouvée.");
    }

    if (business.IsQueueActive != true)
    {
      logger.LogError("La file d'attente est fermée pour l'entreprise `{@0}`.", business.Id);
      throw new InvalidOperationException("La file d'attente est fermée pour l'entreprise.");
    }

    var queueEntrie = new QueueEntries
    {
      Phone = request.Phone,
      ClientName = request.ClientName,
      Status = "waiting",
    };

    context.Queues.Add(queueEntrie);
    await context.SaveChangesAsync();
  }

}