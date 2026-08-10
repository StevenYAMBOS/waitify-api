namespace WaitifyApi.Exceptions;

public class ExternalLoginProviderException(string provider, string message) :
    Exception($"External login provider: {provider} error occurred: {message}");