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
            DateFormatString = "dd/MM/yyyy hh:mm:ss tt",
            Converters = { new AccountTypeConverter() }
        };

        var customers = JsonConvert.DeserializeObject<Customer[]>(json, settings);

        if (context.Customers.Any())
            return;

        // Sets TransactionType to Deposit and calculates balances
        foreach (var customer in customers)
        foreach (var account in customer.Accounts)
        {
            decimal balance = 0;
            foreach (var transaction in account.Transactions)
            {
                transaction.TransactionType = TransactionType.Deposit;
                balance += transaction.Amount;
            }

            account.Balance = balance;
        }

        // Adds to db context
        foreach (var customer in customers) context.Customers.Add(customer);

        // Save changes to the database
        context.SaveChanges();

        Console.WriteLine($"Successfully seeded {customers.Length} customers to the database.");
    }
}