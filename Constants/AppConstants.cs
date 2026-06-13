
namespace WaitifyApi.Constants;

public static class AppConstants
{
    public record Config
    {
        public const string WaitifyUrl = "https://waitify.fr";

    }

    public record Azure
    {
        public static string UsersContainer = Environment.GetEnvironmentVariable("AzureBlobUsersContainer")!;
        public static string BusinessesContainer = Environment.GetEnvironmentVariable("AzureBlobBusinessesContainer")!;
        public static string ContactsContainer = Environment.GetEnvironmentVariable("AzureBlobContactContainer")!;
    }

    public record Roles
    {
        public const string Admin = "Admin";
    }

    public record Authorization
    {
        public const string Denied = "Accès non autorisé";
    }
}