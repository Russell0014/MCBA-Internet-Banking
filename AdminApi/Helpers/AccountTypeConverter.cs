using AdminApi.Models;
using Newtonsoft.Json;

namespace AdminApi.Helpers;

public class AccountTypeConverter : JsonConverter<AccountType>
{
    public override AccountType ReadJson(JsonReader reader, Type objectType, AccountType existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        var value = reader.Value?.ToString();
        return value switch
        {
            "S" => AccountType.Savings,
            "C" => AccountType.Checking,
            _ => throw new JsonSerializationException($"Unknown AccountType value: {value}")
        };
    }

    public override void WriteJson(JsonWriter writer, AccountType value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}