using Azure.Core;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WaitifyApi.Constants;
using WaitifyApi.Data;
using WaitifyApi.Entities;
using WaitifyApi.Enums;
using WaitifyApi.Helpers;
using WaitifyApi.Models;
using WaitifyApi.Repositories;
using WaitifyApi.Constants;

namespace WaitifyApi.Services;

public class AnalyticsService(AppDbContext context, IApplicationUserRepository userService, IBusinessRepository businessService, ILogger<AnalyticsService> logger) : IAnalyticsRepository
{
    public async Task<GetBusinessLiveKpisResponse> GetLiveKpisAsync(Guid businessQrCodeToken, string userId)
    {
        var user = await userService.FindUserByIdAsync(userId);
        var business = await businessService.FindBusinessByQrTokenAsync(businessQrCodeToken);
        var now = DateTime.UtcNow;

        logger.LogInformation("[LOG] Utilisateur : {@0}", user.FirstName);
        logger.LogInformation("[LOG] Entreprise : {@0}", business.Name);

        if (user == null)
        {
            logger.LogError("[ERREUR] Utilisateur non récupéré : {@0}", user.Id);
            throw new UnauthorizedAccessException("Accès interdit");
        }

        Guid existingUserId = Guid.Parse(user.Id);

        if (business == null)
        {
            logger.LogError("[ERREUR] Entreprise non récupérée : {@0}", user.Id);
            throw new KeyNotFoundException("Entreprise non trouvée.");
        }

        var existingBusinessId = Guid.Parse(business.Owner.Id);

        if (existingBusinessId != existingUserId)
        {
            logger.LogError("[ERREUR] Accès interdit, l'utilisateur n'est pas le gérant : \n `business.Owner` = '{@0}', \n `user.id` = '{@1}'", existingBusinessId, existingUserId);
            throw new UnauthorizedAccessException("Accès interdit");
        }

        int clientsWaiting = await context.Queues
        .Where(q =>
            q.Business.QrCodeToken == businessQrCodeToken &&
            q.Status == AppConstants.Queues.Status.Waiting)
        .CountAsync();

        int clientsServedToday = await context.Queues
        .Where(q =>
            q.Status == AppConstants.Queues.Status.Served &&
            q.ServedAt == now)
        .CountAsync();

        /*
         ⚠️ Sélectionner toutes les colonnes 'EstimatedWaitTime' de 'QueueEntries' et en faire une moyenne avec 'Average'.
         SELECT ROUND(AVG("EstimatedWaitTime"), 1) FROM "QueueEntries" WHERE "BusinessQrCodeToken" = 'cee2e51d-a152-47dd-8319-1e175b7f5e44';
        */

        // QueueEntries averageWaitMinutes = context.Queues
        // .FromSql($"SELECT ROUND(AVG(EstimatedWaitTime), 1) FROM QueueEntries WHERE BusinessQrCodeToken = {businessQrCodeToken}").ToListAsync();

        // logger.LogInformation($"[LOG] Temps moyen = {averageWaitMinutes}");

        // double averageWaitMinutes = await context.Queues
        // .Select(q => q.EstimatedWaitTime).Average();

        // .Where(q =>
        //     q.Business.QrCodeToken == businessQrCodeToken &&
        //     q.Status == AppConstants.Queues.Status.Waiting)
        // .Select(q => q.EstimatedWaitTime);
        // .Average();

        var response = new GetBusinessLiveKpisResponse
        {
            BusinessQrCodeToken = business.QrCodeToken,
            UpdatedAt = business.UpdatedAt,
            ClientsWaiting = clientsWaiting,
            ClientsServedToday = clientsServedToday,
            // AverageWaitMinutes = averageWaitMinutes,
            QueueOpenSince = now
        };

        logger.LogInformation("[LOG] KPIS : {@0}", response);

        return response;
    }
}
