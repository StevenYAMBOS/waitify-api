using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WaitifyApi.Data;
using WaitifyApi.Entities;
using WaitifyApi.Models;
using WaitifyApi.Repositories;

namespace WaitifyApi.Services;

public class QueueService(AppDbContext context, IApplicationUserRepository userService, IBusinessRepository businessService, ILogger<QueueService> logger) : IQueueRepository
{

  public async Task<QueueEntries?> FindQueueByIdAsync(Guid id)
  {
    var queue = await context.Queues.FindAsync(id);
    logger.LogInformation("File d'attente : {@0}", JsonConvert.SerializeObject(queue, Formatting.Indented));
    return queue;
  }

  public async Task<JoinQueueResponse> JoinQueueAsync(JoinQueueRequest request)
  {
    var business = await businessService.FindBusinessByQrTokenAsync(request.QrCodeToken);

    if (business == null)
    {
      logger.LogError("Entreprise non trouvée pour le QR token : `{@0}`.", request.QrCodeToken);
      throw new KeyNotFoundException("Entreprise non trouvée.");
    }

    if (!business.IsQueueActive)
    {
      logger.LogError("La file d'attente est fermée pour l'entreprise `{@0}`.", business.Id);
      throw new InvalidOperationException("La file d'attente est fermée.");
    }

    // Vérification client pas déjà inscrit (même numéro + business + waiting)
    bool alreadyInQueue = await context.Queues.AnyAsync(q =>
      q.Phone == request.Phone &&
      q.BusinessId == business.Id &&
      q.Status == "waiting");

    if (alreadyInQueue)
    {
      logger.LogError("Numéro `{@0}` déjà dans la file d'attente du business `{@1}`.", request.Phone, business.Id);
      throw new InvalidOperationException("Ce numéro est déjà dans la file d'attente.");
    }

    // Vérification file pas pleine + compte les clients en attente pour le calcul du temps
    int waitingCount = await context.Queues.CountAsync(q =>
      q.BusinessId == business.Id &&
      q.Status == "waiting");

    if (waitingCount >= business.MaxQueueSize)
    {
      logger.LogError("File d'attente pleine pour l'entreprise `{@0}`. Taille max : `{@1}`.", business.Id, business.MaxQueueSize);
      throw new InvalidOperationException("La file d'attente est pleine.");
    }

    // Calcul du temps d'attente estimé
    // Formule : (nombre_clients_avant * temps_service_moyen_en_secondes) / 60 → minutes
    int estimatedWaitTime = (waitingCount * business.AverageServiceTime) / 60;

    // Le trigger PostgreSQL calcule automatiquement la position
    var queueEntry = new QueueEntries
    {
      BusinessId = business.Id,
      Phone = request.Phone,
      ClientName = request.ClientName,
      Status = "waiting",
      EstimatedWaitTime = estimatedWaitTime,
      Position = 0,
      CreatedAt = DateTime.UtcNow,
      UpdatedAt = DateTime.UtcNow,
    };

    context.Queues.Add(queueEntry);
    await context.SaveChangesAsync();

    // Récupérer la position calculée par le trigger
    await context.Entry(queueEntry).ReloadAsync();

    logger.LogInformation("Client `{@0}` inscrit en position `{@1}` pour l'entreprise `{@2}`.", request.Phone, queueEntry.Position, business.Id);

    return new JoinQueueResponse
    {
      Id = queueEntry.Id,
      BusinessId = business.Id,
      BusinessName = business.Name,
      Position = queueEntry.Position,
      EstimatedWaitTime = estimatedWaitTime,
      Phone = queueEntry.Phone,
      ClientName = queueEntry.ClientName,
      Status = queueEntry.Status,
      CreatedAt = queueEntry.CreatedAt,
    };
  }

  public async Task<CallNextClientResponse> CallNextClientAsync(Guid businessId)
  {
    var business = await context.Businesses.FindAsync(businessId);

    if (business == null)
    {
      logger.LogError("Entreprise non trouvée : `{@0}`.", businessId);
      throw new KeyNotFoundException("Entreprise non trouvée.");
    }

    if (!business.IsQueueActive)
    {
      logger.LogError("La file d'attente est fermée pour l'entreprise `{@0}`.", businessId);
      throw new InvalidOperationException("La file d'attente est fermée.");
    }

    // Récupérer le premier client en attente (position la plus basse)
    var nextClient = await context.Queues
      .Where(q => q.BusinessId == businessId && q.Status == "waiting")
      .OrderBy(q => q.Position)
      .FirstOrDefaultAsync();

    if (nextClient == null)
    {
      logger.LogInformation("Aucun client en attente pour l'entreprise `{@0}`.", businessId);
      throw new InvalidOperationException("Aucun client en attente dans la file.");
    }

    nextClient.Status = "called";
    nextClient.CalledAt = DateTime.UtcNow;
    nextClient.UpdatedAt = DateTime.UtcNow;

    await context.SaveChangesAsync();

    // Recharger pour récupérer la position recalculée par les triggers PostgreSQL
    await context.Entry(nextClient).ReloadAsync();

    logger.LogInformation("Client `{@0}` appelé pour l'entreprise `{@1}`.", nextClient.Phone, businessId);

    return new CallNextClientResponse
    {
      Id = nextClient.Id,
      BusinessId = nextClient.BusinessId,
      Phone = nextClient.Phone,
      ClientName = nextClient.ClientName,
      Position = nextClient.Position,
      Status = nextClient.Status,
      CalledAt = nextClient.CalledAt,
    };
  }
}
