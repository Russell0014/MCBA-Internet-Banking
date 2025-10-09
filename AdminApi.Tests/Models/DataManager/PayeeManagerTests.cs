using System;
using System.Linq;
using System.Threading.Tasks;
using AdminApi.Data;
using AdminApi.Models;
using AdminApi.Models.DataManager;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AdminApi.Tests.Models.DataManager;

public class PayeeManagerTests
{
    private static DatabaseContext CreateContext(string dbName)
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(connection)
            .Options;

        var ctx = new DatabaseContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllPayees()
    {
        using var ctx = CreateContext(nameof(GetAllAsync_ReturnsAllPayees));
        ctx.Payees.Add(new Payee { PayeeId = 1, Name = "A", Address = "a", City = "C", State = "VIC", Postcode = "3000", Phone = "(03) 9123 4567" });
        ctx.Payees.Add(new Payee { PayeeId = 2, Name = "B", Address = "b", City = "C", State = "VIC", Postcode = "4000", Phone = "(03) 9123 4567" });
        await ctx.SaveChangesAsync();

        var mgr = new PayeeManager(ctx);
        var all = (await mgr.GetAllAsync()).ToList();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task GetByPostcodeAsync_FiltersByPostcode()
    {
        using var ctx = CreateContext(nameof(GetByPostcodeAsync_FiltersByPostcode));
        ctx.Payees.Add(new Payee { PayeeId = 1, Name = "A", Address = "a", City = "C", State = "VIC", Postcode = "3000", Phone = "(03) 9123 4567" });
        ctx.Payees.Add(new Payee { PayeeId = 2, Name = "B", Address = "b", City = "C", State = "VIC", Postcode = "4000", Phone = "(03) 9123 4567" });
        await ctx.SaveChangesAsync();

        var mgr = new PayeeManager(ctx);
        var res = (await mgr.GetByPostcodeAsync("300")).ToList();
        Assert.Single(res);
        Assert.Equal("3000", res[0].Postcode);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEntity()
    {
        using var ctx = CreateContext(nameof(UpdateAsync_UpdatesEntity));
        ctx.Payees.Add(new Payee { PayeeId = 1, Name = "Old", Address = "addr", City = "City", State = "VIC", Postcode = "3000", Phone = "(03) 9123 4567" });
        await ctx.SaveChangesAsync();

        var mgr = new PayeeManager(ctx);
        var p = await mgr.GetByIdAsync(1);
        Assert.NotNull(p);
        p!.Name = "New";
        await mgr.UpdateAsync(p);

        var refreshed = await ctx.Payees.FindAsync(1);
        Assert.Equal("New", refreshed.Name);
    }
}
