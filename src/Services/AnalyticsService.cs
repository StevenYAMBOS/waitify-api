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
        DateTime now = DateTime.UtcNow;

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


        // Reproduire cette requête SELECT COUNT("Status") FROM "QueueEntries" WHERE "BusinessQrCodeToken" = '' AND "Status" = 'waiting';
        int clientsWaiting = await context.Queues
        .Where(q =>
            q.BusinessQrCodeToken == businessQrCodeToken &&
            q.Status == AppConstants.Queues.Status.Waiting)
        .Select(q => q.Status)
        .CountAsync();

        // int clientsWaiting = context.Queues
        // .Select(q => q.Status)
        // .Where(q =>
        //     q.BusinessQrCodeToken == businessQrCodeToken &&
        //     q.Status == AppConstants.Queues.Status.Waiting)
        // .CountAsync();

        logger.LogInformation("[LOG] Clients en attente : {@0}", clientsWaiting);


        int clientsServedToday = await context.Queues
        .Where(q =>
            q.Status == AppConstants.Queues.Status.Served &&
            q.ServedAt == now)
        .CountAsync();

        // var averageWaitMinutes = context.Queues.FromSql($"SELECT ROUND(AVG(EstimatedWaitTime), 1) FROM QueueEntries WHERE BusinessQrCodeToken = {businessQrCodeToken}");
        var averageWaitMinutes = Math.Round(context.Queues
            .Where(q => q.BusinessQrCodeToken == businessQrCodeToken)
            .Select(q => q.EstimatedWaitTime)
            .Average(), 1);

        var response = new GetBusinessLiveKpisResponse
        {
            BusinessQrCodeToken = business.QrCodeToken,
            UpdatedAt = business.UpdatedAt,
            ClientsWaiting = clientsWaiting,
            ClientsServedToday = clientsServedToday,
            AverageWaitMinutes = averageWaitMinutes,
            QueueOpenSince = now
        };

        logger.LogInformation("[LOG] KPIS : {@0}", response);

        return response;
    }
}
