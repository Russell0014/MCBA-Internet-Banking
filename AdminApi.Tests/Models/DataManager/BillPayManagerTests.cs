using System;
using System.Linq;
using System.Threading.Tasks;
using AdminApi.Data;
using AdminApi.Models;
using AdminApi.Models.DataManager;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AdminApi.Tests.Models.DataManager;

public class BillPayManagerTests
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
        // seed one Account and one Payee so BillPay foreign keys are valid
        ctx.Accounts.Add(new AdminApi.Models.Account { AccountNumber = 1234, AccountType = AdminApi.Models.AccountType.Checking, Balance = 100m });
        ctx.Payees.Add(new AdminApi.Models.Payee { PayeeId = 1, Name = "TestPayee", Address = "Addr", City = "City", State = "VIC", Postcode = "3000", Phone = "(03) 9999 9999" });
        ctx.SaveChanges();
        return ctx;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsBillPays()
    {
        using var ctx = CreateContext(nameof(GetAllAsync_ReturnsBillPays));
        var acct = ctx.Accounts.First();
        var payee = ctx.Payees.First();
        ctx.BillPays.Add(new BillPay { BillPayId = 1, AccountNumber = acct.AccountNumber, Account = acct, PayeeId = payee.PayeeId, Payee = payee, Amount = 5m, ScheduleTimeUtc = DateTime.UtcNow, Period = PeriodType.OneOff, Status = StatusType.Pending });
        ctx.BillPays.Add(new BillPay { BillPayId = 2, AccountNumber = acct.AccountNumber, Account = acct, PayeeId = payee.PayeeId, Payee = payee, Amount = 10m, ScheduleTimeUtc = DateTime.UtcNow, Period = PeriodType.OneOff, Status = StatusType.Pending });
        await ctx.SaveChangesAsync();

        var mgr = new BillPayManager(ctx);
        var all = (await mgr.GetAllAsync()).ToList();
        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task UpdateBillStatusAsync_InvalidId_ThrowsKeyNotFound()
    {
        using var ctx = CreateContext(nameof(UpdateBillStatusAsync_InvalidId_ThrowsKeyNotFound));
        var mgr = new BillPayManager(ctx);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => mgr.UpdateBillStatusAsync(999, StatusType.Blocked));
    }

    [Fact]
    public async Task UpdateBillStatusAsync_BlockingNonPending_ThrowsInvalidOperation()
    {
        using var ctx = CreateContext(nameof(UpdateBillStatusAsync_BlockingNonPending_ThrowsInvalidOperation));
        var acct2 = ctx.Accounts.First();
        var payee2 = ctx.Payees.First();
        ctx.BillPays.Add(new BillPay { BillPayId = 1, AccountNumber = acct2.AccountNumber, Account = acct2, PayeeId = payee2.PayeeId, Payee = payee2, Amount = 1m, ScheduleTimeUtc = DateTime.UtcNow, Period = PeriodType.OneOff, Status = StatusType.Completed });
        await ctx.SaveChangesAsync();

        var mgr = new BillPayManager(ctx);
        await Assert.ThrowsAsync<InvalidOperationException>(() => mgr.UpdateBillStatusAsync(1, StatusType.Blocked));
    }

    [Fact]
    public async Task UpdateBillStatusAsync_UnblockWrongState_ThrowsInvalidOperation()
    {
        using var ctx = CreateContext(nameof(UpdateBillStatusAsync_UnblockWrongState_ThrowsInvalidOperation));
        var acct3 = ctx.Accounts.First();
        var payee3 = ctx.Payees.First();
        ctx.BillPays.Add(new BillPay { BillPayId = 2, AccountNumber = acct3.AccountNumber, Account = acct3, PayeeId = payee3.PayeeId, Payee = payee3, Amount = 1m, ScheduleTimeUtc = DateTime.UtcNow, Period = PeriodType.OneOff, Status = StatusType.Pending });
        await ctx.SaveChangesAsync();

        var mgr = new BillPayManager(ctx);
        // trying to set Pending when it's not Blocked should throw
        await Assert.ThrowsAsync<InvalidOperationException>(() => mgr.UpdateBillStatusAsync(2, StatusType.Pending));
    }

    [Fact]
    public async Task UpdateBillStatusAsync_BlockThenUnblock_Works()
    {
        using var ctx = CreateContext(nameof(UpdateBillStatusAsync_BlockThenUnblock_Works));
        var acct4 = ctx.Accounts.First();
        var payee4 = ctx.Payees.First();
        ctx.BillPays.Add(new BillPay { BillPayId = 3, AccountNumber = acct4.AccountNumber, Account = acct4, PayeeId = payee4.PayeeId, Payee = payee4, Amount = 1m, ScheduleTimeUtc = DateTime.UtcNow, Period = PeriodType.OneOff, Status = StatusType.Pending });
        await ctx.SaveChangesAsync();

        var mgr = new BillPayManager(ctx);
        await mgr.UpdateBillStatusAsync(3, StatusType.Blocked);
        var afterBlock = await ctx.BillPays.FindAsync(3);
        Assert.NotNull(afterBlock);
        Assert.Equal(StatusType.Blocked, afterBlock!.Status);

        await mgr.UpdateBillStatusAsync(3, StatusType.Pending);
        var afterUnblock = await ctx.BillPays.FindAsync(3);
        Assert.NotNull(afterUnblock);
        Assert.Equal(StatusType.Pending, afterUnblock!.Status);
    }
}
