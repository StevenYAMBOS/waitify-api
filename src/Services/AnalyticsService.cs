using Microsoft.EntityFrameworkCore;
using WaitifyApi.Constants;
using WaitifyApi.Data;
using WaitifyApi.Models;
using WaitifyApi.Repositories;

namespace WaitifyApi.Services;

public class AnalyticsService(AppDbContext context, IBusinessRepository businessService, ILogger<AnalyticsService> logger) : IAnalyticsRepository
{
    public async Task<GetBusinessLiveKpisResponse> GetLiveKpisAsync(Guid businessQrCodeToken, string userId)
    {
        var business = await businessService.FindBusinessByQrTokenAsync(businessQrCodeToken)
            ?? throw new KeyNotFoundException("Entreprise non trouvée.");

        if (business.OwnerId != userId)
        {
            logger.LogWarning("Accès refusé aux KPIs de `{QrCodeToken}` pour l'utilisateur `{UserId}`.", businessQrCodeToken, userId);
            throw new UnauthorizedAccessException("Accès interdit");
        }

        // Les KPIs « live » n'existent que pour une file ouverte.
        if (!business.IsQueueActive)
        {
            throw new InvalidOperationException("La file d'attente n'est pas ouverte.");
        }

        // Journée UTC en cours : [minuit, minuit + 1 jour[.
        DateTime todayStart = DateTime.UtcNow.Date;
        DateTime tomorrowStart = todayStart.AddDays(1);

        var entries = context.Queues.Where(q => q.BusinessQrCodeToken == businessQrCodeToken);
        var waiting = entries.Where(q => q.Status == AppConstants.Queues.Status.Waiting);

        int clientsWaiting = await waiting.CountAsync();

        int clientsServedToday = await entries
            .CountAsync(q =>
                q.Status == AppConstants.Queues.Status.Served &&
                q.ServedAt >= todayStart &&
                q.ServedAt < tomorrowStart);

        // Spec ANALYTICS.md §3.1 : AVG(EstimatedWaitTime) WHERE Status='waiting'. 0 si personne n'attend.
        double averageWaitMinutes = clientsWaiting == 0
            ? 0
            : Math.Round(await waiting.AverageAsync(q => q.EstimatedWaitTime), 1);

        return new GetBusinessLiveKpisResponse
        {
            BusinessQrCodeToken = business.QrCodeToken,
            UpdatedAt = DateTime.UtcNow,
            ClientsWaiting = clientsWaiting,
            ClientsServedToday = clientsServedToday,
            AverageWaitMinutes = averageWaitMinutes,
            QueueOpenSince = business.QueueOpenedAt
        };
    }
}
