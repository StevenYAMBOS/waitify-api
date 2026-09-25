using Microsoft.AspNetCore.JsonPatch;

namespace WaitifyApi.Helpers;

/// <summary>
/// Restreint un JSON Patch à une liste blanche de propriétés : sans elle, un client peut
/// modifier n'importe quel champ de l'entité (PasswordHash, OwnerId, QrCodeToken…).
/// </summary>
public static class JsonPatchGuard
{
    public static bool OnlyTouches<T>(JsonPatchDocument<T> document, IReadOnlySet<string> allowedProperties) where T : class
    {
        return document.Operations.All(operation =>
        {
            var property = operation.path?.Trim('/').Split('/')[0];
            return property is not null && allowedProperties.Contains(property);
        });
    }
}
