using Newtonsoft.Json;

namespace WaitifyApi.Helpers;

public static class JsonResponseHelper
{
    public static object JsonConversion(object data)
    {
        return JsonConvert.SerializeObject(data, Formatting.Indented);
    }
}