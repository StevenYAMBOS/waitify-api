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
        try
        {
            var user = await userService.FindUserByIdAsync(userId);
            var business = await businessService.FindBusinessByQrTokenAsync(businessQrCodeToken);
            DateTime dateTimeNow = DateTime.UtcNow.Date;
            var dateTimeNowFormat = dateTimeNow.GetDateTimeFormats();

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

            if (business.IsQueueActive == false)
            {
                logger.LogError("[ERREUR] La file d'attente n'est pas ouverte.");
                throw new UnauthorizedAccessException("La file d'attente n'est pas ouverte.");
            }

            // Reproduire cette requête SELECT COUNT("Status") FROM "QueueEntries" WHERE "BusinessQrCodeToken" = '' AND "Status" = 'waiting';
            int clientsWaiting = await context.Queues
            .Where(q =>
                q.BusinessQrCodeToken == businessQrCodeToken &&
                q.Status == AppConstants.Queues.Status.Waiting)
            .Select(q => q.Status)
            .CountAsync();

            // SELECT COUNT("Status") FROM "QueueEntries" WHERE "BusinessQrCodeToken" = '' AND "Status" = 'served' AND "ServedAt" = CURRENT_DATE;
            int clientsServedToday = await context.Queues
            .Where(q =>
                q.BusinessQrCodeToken == businessQrCodeToken &&
                q.Status == AppConstants.Queues.Status.Served &&
                q.ServedAt == dateTimeNow)
            .Select(q => q.Status)
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
                QueueOpenSince = dateTimeNow
            };

            logger.LogInformation("[LOG] KPIS : {@0}", response);

            return response;
        } catch (Exception exeption)
        {
            logger.LogError($"[ERROR] Une erreur est survenue lors de la récupération des KPIs en temps réel : {exeption}");
            throw new InvalidOperationException("Une erreur est survenue lors de la récupération des KPIs en temps réel.", exeption);
        }
    }
}
