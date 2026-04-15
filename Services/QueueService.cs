using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WaitifyApi.Data;
using WaitifyApi.Entities;
using WaitifyApi.Helpers;
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

    // Calcul direct de la position via QueuePositionHelper (remplace l'ancien trigger PostgreSQL)
    int position = QueuePositionHelper.CalculateNewPosition(waitingCount);

    var queueEntry = new QueueEntries
    {
      BusinessId = business.Id,
      Phone = request.Phone,
      ClientName = request.ClientName,
      Status = "waiting",
      EstimatedWaitTime = estimatedWaitTime,
      Position = position,
      CreatedAt = DateTime.UtcNow,
      UpdatedAt = DateTime.UtcNow,
    };

    context.Queues.Add(queueEntry);
    await context.SaveChangesAsync();

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
    // var business = await context.Businesses.FindAsync(businessId); // BusinessId == QrCodeToken !
    // Pour le moment on utilise l'id (`BusinessId`) de l'entreprise au lieu du `QRCodeToken` pour identifier l'entreprise.
    var business = await businessService.FindBusinessByIdAsync(businessId);


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

    // Recalcul des positions des clients restants en attente (remplace l'ancien trigger PostgreSQL)
    await QueuePositionHelper.RecalculatePositionsAsync(context, businessId);
    await context.SaveChangesAsync();

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

  public async Task<CancelQueueEntryResponse> CancelQueueEntryAsync(Guid id)
  {
    var entry = await context.Queues.FindAsync(id);

    if (entry == null)
    {
      logger.LogError("Entrée de file d'attente introuvable : `{@0}`.", id);
      throw new KeyNotFoundException("Entrée de file d'attente introuvable.");
    }

    if (entry.Status != "waiting")
    {
      logger.LogError("Impossible d'annuler l'entrée `{@0}` avec le statut `{@1}`.", id, entry.Status);
      throw new InvalidOperationException($"Impossible d'annuler une entrée avec le statut '{entry.Status}'.");
    }

    entry.Status = "cancelled";
    entry.UpdatedAt = DateTime.UtcNow;

    await context.SaveChangesAsync();

    // Recalcul des positions des clients restants en attente (remplace l'ancien trigger PostgreSQL)
    await QueuePositionHelper.RecalculatePositionsAsync(context, entry.BusinessId);
    await context.SaveChangesAsync();

    logger.LogInformation("Entrée `{@0}` annulée pour le client `{@1}`.", id, entry.Phone);

    return new CancelQueueEntryResponse
    {
      Id = entry.Id,
      BusinessId = entry.BusinessId,
      Phone = entry.Phone,
      ClientName = entry.ClientName,
      Status = entry.Status,
      UpdatedAt = entry.UpdatedAt,
    };
  }

  public async Task<MarkClientAsServedResponse> MarkClientAsServedAsync(Guid id, MarkClientAsServedRequest request)
  {
    var entry = await context.Queues.FindAsync(id);

    if (entry == null)
    {
      logger.LogError("Entrée de file d'attente introuvable : `{@0}`.", id);
      throw new KeyNotFoundException("Entrée de file d'attente introuvable.");
    }

    if (entry.Status != "called")
    {
      logger.LogError("Impossible de marquer comme servi l'entrée `{@0}` avec le statut `{@1}`.", id, entry.Status);
      throw new InvalidOperationException($"Impossible de marquer comme servi une entrée avec le statut '{entry.Status}'.");
    }

    entry.Status = "served";
    entry.ServedAt = DateTime.UtcNow;
    entry.UpdatedAt = DateTime.UtcNow;

    if (request.ActualServiceTime.HasValue)
      entry.ActualServiceTime = request.ActualServiceTime.Value;

    await context.SaveChangesAsync();

    logger.LogInformation("Client `{@0}` marqué comme servi pour l'entrée `{@1}`.", entry.Phone, id);

    return new MarkClientAsServedResponse
    {
      Id = entry.Id,
      BusinessId = entry.BusinessId,
      Phone = entry.Phone,
      ClientName = entry.ClientName,
      Status = entry.Status,
      CalledAt = entry.CalledAt,
      ServedAt = entry.ServedAt,
      ActualServiceTime = entry.ActualServiceTime == 0 ? null : entry.ActualServiceTime,
    };
  }
}
