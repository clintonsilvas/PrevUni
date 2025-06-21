using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text.Json;

namespace Front.Models
{
    public class MensagemChat
    {
        public string Pergunta { get; set; }
        public string Resposta { get; set; }
    }
}

public static class TempDataExtensions
{
    public static void SetObject<T>(this ITempDataDictionary tempData, string key, T value)
    {
        tempData[key] = JsonSerializer.Serialize(value);
    }

    public static T? GetObject<T>(this ITempDataDictionary tempData, string key)
    {
        if (tempData.TryGetValue(key, out var o) && o is string json)
            return JsonSerializer.Deserialize<T>(json);
        return default;
    }
}


public static class SessionExtensions
{
    public static void SetObject<T>(this ISession session, string key, T value) =>
        session.SetString(key, JsonSerializer.Serialize(value));

    public static T? GetObject<T>(this ISession session, string key)
    {
        var json = session.GetString(key);
        return json is null ? default : JsonSerializer.Deserialize<T>(json);
    }
}
