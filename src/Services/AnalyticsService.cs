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
using System.Globalization;
using System;

namespace WaitifyApi.Services;

public class AnalyticsService(AppDbContext context, IApplicationUserRepository userService, IBusinessRepository businessService, ILogger<AnalyticsService> logger) : IAnalyticsRepository
{
    public async Task<GetBusinessLiveKpisResponse> GetLiveKpisAsync(Guid businessQrCodeToken, string userId)
    {
        var user = await userService.FindUserByIdAsync(userId);
        var business = await businessService.FindBusinessByQrTokenAsync(businessQrCodeToken);
        DateTime now = DateTime.UtcNow;

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

        // Format date BDD = "ServedAt": "2026-09-17T23:54:35.0808710Z"
        // int clientsServedToday = await context.Queues
        // .Where(q =>
        //     q.BusinessQrCodeToken == businessQrCodeToken &&
        //     q.Status == AppConstants.Queues.Status.Served &&
        //     q.ServedAt.ToString().Remove(9) == now)
        // .CountAsync();

        var clientsServedToday = await context.Queues
        .Where(q =>
            q.BusinessQrCodeToken == businessQrCodeToken &&
            q.Status == AppConstants.Queues.Status.Served &&
            q.ServedAt != null)
        .ToListAsync();

        logger.LogInformation("[LOG CODE] Clients servis : {@0}", now.ToString("yyyy-MM-dd"));
        logger.LogInformation("[LOG DATABASE] Clients servis formatté : {@0}", clientsServedToday[0].ServedAt.ToString());

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
            ClientsServedToday = 2237,
            // ClientsServedToday = clientsServedToday,
            AverageWaitMinutes = averageWaitMinutes,
            QueueOpenSince = now
        };

        // logger.LogInformation("[LOG] KPIS : {@0}", response);

        return response;
    }
}
