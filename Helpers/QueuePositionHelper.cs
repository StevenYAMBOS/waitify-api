using Microsoft.EntityFrameworkCore;
using WaitifyApi.Data;

namespace WaitifyApi.Helpers;

/// <summary>
/// Helper statique pour le calcul et le recalcul des positions dans les files d'attente.
/// Remplace l'ancien trigger PostgreSQL <c>recalculate_positions_after_change</c>.
/// </summary>
/// <remarks>
/// Logique équivalente au trigger supprimé :
/// <code>
/// ROW_NUMBER() OVER (ORDER BY "CreatedAt" ASC)
/// WHERE "BusinessId" = @businessQrCodeToken AND "Status" = 'waiting'
/// </code>
/// </remarks>
public static class QueuePositionHelper
{
    /// <summary>
    /// Recalcule les positions de toutes les entrées en statut <c>waiting</c>
    /// pour l'établissement spécifié, dans l'ordre chronologique d'inscription (<c>CreatedAt ASC</c>).
    /// </summary>
    /// <remarks>
    /// Cette méthode modifie uniquement les entités trackées par EF Core en mémoire.
    /// Le <c>SaveChangesAsync</c> doit être appelé par la méthode appelante pour persister les changements.
    /// <para>
    /// À appeler après chaque transition de statut qui retire un client de la file <c>waiting</c> :
    /// <list type="bullet">
    ///   <item><c>waiting</c> → <c>called</c></item>
    ///   <item><c>waiting</c> → <c>cancelled</c></item>
    ///   <item><c>waiting</c> → <c>missed</c></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="context">Contexte de base de données EF Core.</param>
    /// <param name="businessQrCodeToken">Identifiant de l'établissement concerné.</param>
    public static async Task RecalculatePositionsAsync(AppDbContext context, Guid businessQrCodeToken)
    {
        var waitingEntries = await context.Queues
            .Where(q => q.BusinessQrCodeToken == businessQrCodeToken && q.Status == "waiting")
            .OrderBy(q => q.CreatedAt)
            .ToListAsync();

        for (int i = 0; i < waitingEntries.Count; i++)
        {
            waitingEntries[i].Position = i + 1;
        }
    }

    /// <summary>
    /// Calcule la position à assigner à un nouveau client rejoignant la file.
    /// </summary>
    /// <param name="waitingCount">Nombre de clients actuellement en statut <c>waiting</c>.</param>
    /// <returns>La position du nouveau client (<c>waitingCount + 1</c>).</returns>
    public static int CalculateNewPosition(int waitingCount) => waitingCount + 1;
}
