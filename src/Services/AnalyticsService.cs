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

public class AnalyticsService(AppDbContext context, IApplicationUserRepository userService, ILogger<AnalyticsService> logger) : IAnalyticsRepository
{
    public async Task<Analytics?> GetLiveKpisAsync(Guid businessId)
    {
        var user = await userService.FindUserByIdAsync(userId);
        logger.LogInformation("[Requête] ROLE UTILISATEUR : {@0}", user.Role);
        logger.LogInformation("[Vérification] ROLE UTILISATEUR : {@0}", AppConstants.Roles.Admin);
        if (user == null)
        {
            logger.LogError("L'id utilisateur n'est pas correcte : {@0}", user.Id);
            throw new KeyNotFoundException("Utilisateur non trouvé");
        }

        if (user.Role.ToString() != AppConstants.Roles.Admin)
        {
            throw new UnauthorizedAccessException("Accès interdit");
        }

        var Analytics = await context.Analyticses.FindAsync(id);
        return Analytics;
    }
}
