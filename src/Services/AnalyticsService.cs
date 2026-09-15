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
            logger.LogError("[ERREUR] Uitlisateur non récupéré : {@0}", user.Id);
            throw new UnauthorizedAccessException("Accès interdit");
        }

        if (business.Owner.ToString() != user.Id)
        {
            logger.LogError("[ERREUR] Accès interdit, l'utilisateur n'est pas le gérant : \n `business.Owner` = '{@0}', \n `user.id` = '{@1}'", business.Owner, user.Id);
            throw new UnauthorizedAccessException("Accès interdit");
        }

        if (business == null)
        {
            logger.LogError("[ERREUR] Entreprise non récupérée : {@0}", user.Id);
            throw new KeyNotFoundException("Entreprise non trouvée.");
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

        double averageWaitMinutes = await context.Queues
        .Where(q =>
            q.Business.QrCodeToken == businessQrCodeToken &&
            q.Status == AppConstants.Queues.Status.Waiting)
        .Average(q => q.EstimatedWaitTime);

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
