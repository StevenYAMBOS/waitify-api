using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using WaitifyApi.Constants;
using WaitifyApi.Data;
using WaitifyApi.Entities;
using WaitifyApi.Helpers;
using WaitifyApi.Models;
using WaitifyApi.Repositories;

namespace WaitifyApi.Services;

public class QueueService(AppDbContext context, IApplicationUserRepository userService, ILogger<QueueService> logger) : IQueueRepository
{
  public async Task<(bool Success, IEnumerable<string?> Errors)> JoinQueueAsync(Guid businessId, string userId, Guid qrCodeToken)
  {
    var business = await FindBusinessByIdAsync(businessId);
    if (business == null)
    {
      logger.LogError("Entreprise non trouvée.\n ID en base de données : `{@0}`.\n ID de la requête : `{@1}`.", business?.Id, businessId);
      throw new KeyNotFoundException("Entreprise non trouvée.");
    }

    var existingUser = userService.FindUserByIdAsync(userId);
    if (existingUser?.Id.ToString() == userId)
    {
      logger.LogError("Accès interdit. L'id utilisateur est incorrecte.\n ID en base de données : `{@0}`.\n ID de la requête : `{@1}`.", existingUser?.Id, userId);
      throw new KeyNotFoundException("Utilisateur non trouvé ou accès non autorisé.");
    }

    if (userId != business.OwnerId)
    {
      logger.LogError("Accès refusé !\n ID récupéré du JWT : `{@0}`.\n ID du gérant en BDD : `{@1}`.", userId, business.OwnerId);
      throw new KeyNotFoundException("Utilisateur non trouvé.");
    }

    var url = AppConstants.WaitifyUrl + "/q/" + qrCodeToken;
    var qrCodeGenerated = await qRCodeHelper.GenerateQRCode(url);

    logger.LogInformation("Nouveau QRCode généré : {@0}", JsonConvert.SerializeObject(qrCodeGenerated, Formatting.Indented));
    return qrCodeGenerated;
  }

}