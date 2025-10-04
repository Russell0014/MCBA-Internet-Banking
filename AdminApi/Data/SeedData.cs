using AdminApi.Helpers;
using AdminApi.Models;
using Newtonsoft.Json;

// Add this

namespace AdminApi.Data;

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
            DateFormatString = "dd/MM/yyyy hh:mm:ss tt",
            Converters = { new AccountTypeConverter() }
        };

        //Adds payee seed data
        context.AddRange(
            new Payee
            {
                Name = "Telstra",
                Address = "242 Exhibition Street",
                City = "Melbourne",
                State = "VIC",
                Postcode = "3000",
                Phone = "1300 368 387"
            },
            new Payee
            {
                Name = "Optus",
                Address = "1 Lyonpark Road",
                City = "Sydney",
                State = "NSW",
                Postcode = "2113",
                Phone = "02 7233 2612"
            },
            new Payee
            {
                Name = "AGL Energy",
                Address = "699 Bourke St",
                City = "Docklands",
                State = "VIC",
                Postcode = "3008",
                Phone = "1800 775 329"
            }
        );

        // Save changes to the database
        context.SaveChanges();

    }
}