using MCBA.Helpers;
using MCBA.Models;
using Newtonsoft.Json;

// Add this

namespace MCBA.Data;

public class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        const string url = "https://coreteaching01.csit.rmit.edu.au/~e103884/wdt/services/customers/";
        var context = serviceProvider.GetRequiredService<DatabaseContext>();

        using var client = new HttpClient();
        var json = client.GetStringAsync(url).Result;

        var settings = new JsonSerializerSettings
        {
            DateFormatString = "dd/MM/yyyy",
            Converters = { new AccountTypeConverter() }
        };

        var customers = JsonConvert.DeserializeObject<Customer[]>(json, settings);

        if (context.Customers.Any())
            return;

        Console.WriteLine(JsonConvert.SerializeObject(customers, Formatting.Indented));
    }
}