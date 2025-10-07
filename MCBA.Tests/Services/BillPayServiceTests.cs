using MCBA.Data;
using MCBA.Models;
using MCBA.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MCBA.Tests.Services;

public class BillPayServiceTests : IDisposable
{
    private readonly DatabaseContext _context;
    private readonly BillPayService _service;

    public BillPayServiceTests()
    {
        _context = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite($"Data Source=file:{Guid.NewGuid()}?mode=memory&cache=shared").Options);

        _context.Database.EnsureCreated();

        // Create a service provider that can resolve DatabaseContext
        var services = new ServiceCollection();
        services.AddScoped(_ => _context);
        var serviceProvider = services.BuildServiceProvider();

        SeedData.Initialize(serviceProvider);

        _service = new BillPayService(_context);
    }


    // Test that GetCustomerAsync returns the customer for a valid customer ID
    [Fact]
    public async Task GetCustomerAsync_ValidCustomerId_ReturnsCustomer()
    {
        // Arrange
        var customerId = 2100;

        // Act
        var result = await _service.GetCustomerAsync(customerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customerId, result.CustomerId);
    }



    // Test that GetAccountsAsync returns a list of accounts for a valid customer ID
    [Fact]
    public async Task GetAccountsAsync_ValidCustomerId_ReturnsAccountsList()
    {
        // Arrange
        var customerId = 2100;

        // Act
        var result = await _service.GetAccountsAsync(customerId);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, account => Assert.Equal(customerId, account.CustomerId));
    }

    // Test that GetAccountsAsync returns all accounts for a customer with multiple accounts
    [Fact]
    public async Task GetAccountsAsync_CustomerWithMultipleAccounts_ReturnsAllAccounts()
    {
        // Arrange
        var customerId = 2100;
        var expectedAccountCount = _context.Accounts.Count(a => a.CustomerId == customerId);

        // Act
        var result = await _service.GetAccountsAsync(customerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedAccountCount, result.Count);
    }


    // Test that GetBillPaysAsync returns a view model with empty list for customer with no bills
    [Fact]
    public async Task GetBillPaysAsync_CustomerWithNoBills_ReturnsViewModelWithEmptyList()
    {
        // Arrange
        var customerId = 2100;

        // Act
        var result = await _service.GetBillPaysAsync(customerId);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.BillPayList);
        Assert.Empty(result.BillPayList);
    }

    // Test that GetBillPaysAsync returns bill pay view model with bills for customer with bills
    [Fact]
    public async Task GetBillPaysAsync_CustomerWithBills_ReturnsBillPayViewModel()
    {
        // Arrange
        var customerId = 2100;
        var account = _context.Accounts.First(a => a.CustomerId == customerId);
        var payee = _context.Payees.First();

        var billPay = new BillPay
        {
            AccountNumber = account.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 100m,
            ScheduleTimeUtc = DateTime.UtcNow.AddDays(7),
            Period = PeriodType.Monthly,
            Status = StatusType.Pending
        };
        _context.BillPays.Add(billPay);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetBillPaysAsync(customerId);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.BillPayList);
        Assert.Single(result.BillPayList);
        Assert.Equal(billPay.BillPayId, result.BillPayList[0].BillPayId);
    }

    // Test that GetBillPaysAsync returns all bills sorted by schedule time
    [Fact]
    public async Task GetBillPaysAsync_MultipleBills_ReturnsAllBillsSortedByScheduleTime()
    {
        // Arrange
        var customerId = 2100;
        var account = _context.Accounts.First(a => a.CustomerId == customerId);
        var payee = _context.Payees.First();

        var billPay1 = new BillPay
        {
            AccountNumber = account.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 100m,
            ScheduleTimeUtc = DateTime.UtcNow.AddDays(10),
            Period = PeriodType.Monthly,
            Status = StatusType.Pending
        };

        var billPay2 = new BillPay
        {
            AccountNumber = account.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 200m,
            ScheduleTimeUtc = DateTime.UtcNow.AddDays(5), // Earlier date
            Period = PeriodType.OneOff,
            Status = StatusType.Pending
        };

        _context.BillPays.AddRange(billPay1, billPay2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetBillPaysAsync(customerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.BillPayList.Count);
        // Verify sorted by ScheduleTimeUtc (billPay2 should be first)
        Assert.Equal(billPay2.BillPayId, result.BillPayList[0].BillPayId);
        Assert.Equal(billPay1.BillPayId, result.BillPayList[1].BillPayId);
    }

    // Test that GetBillPaysAsync includes payee information in the results
    [Fact]
    public async Task GetBillPaysAsync_IncludesPayeeInformation()
    {
        // Arrange
        var customerId = 2100;
        var account = _context.Accounts.First(a => a.CustomerId == customerId);
        var payee = _context.Payees.First();

        var billPay = new BillPay
        {
            AccountNumber = account.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 100m,
            ScheduleTimeUtc = DateTime.UtcNow.AddDays(7),
            Period = PeriodType.Monthly,
            Status = StatusType.Pending
        };
        _context.BillPays.Add(billPay);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetBillPaysAsync(customerId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.BillPayList);
        Assert.NotNull(result.BillPayList[0].Payee);
        Assert.Equal(payee.PayeeId, result.BillPayList[0].Payee.PayeeId);
    }


    // Test that CheckPayeeId returns true for a valid payee ID
    [Fact]
    public async Task CheckPayeeId_ValidPayeeId_ReturnsTrue()
    {
        // Arrange
        var payee = _context.Payees.First();

        // Act
        var result = await _service.CheckPayeeId(payee.PayeeId);

        // Assert
        Assert.True(result);
    }


    // Test that CreateBillAsync creates a bill pay with pending status for valid data
    [Fact]
    public async Task CreateBillAsync_ValidData_CreatesBillPayWithPendingStatus()
    {
        // Arrange
        var account = _context.Accounts.First();
        var payee = _context.Payees.First();
        var amount = 150.50m;
        var scheduleTime = DateTime.UtcNow.AddDays(7);
        var period = PeriodType.Monthly;

        // Act
        await _service.CreateBillAsync(account.AccountNumber, payee.PayeeId, amount, scheduleTime, period);

        // Assert
        var createdBill = await _context.BillPays
            .FirstOrDefaultAsync(b => b.AccountNumber == account.AccountNumber && b.Amount == amount);
        Assert.NotNull(createdBill);
        Assert.Equal(payee.PayeeId, createdBill.PayeeId);
        Assert.Equal(amount, createdBill.Amount);
        Assert.Equal(scheduleTime, createdBill.ScheduleTimeUtc);
        Assert.Equal(period, createdBill.Period);
        Assert.Equal(StatusType.Pending, createdBill.Status);
    }

    // Test that CreateBillAsync creates a bill with monthly period
    [Fact]
    public async Task CreateBillAsync_MonthlyPeriod_CreatesBillWithMonthlyPeriod()
    {
        // Arrange
        var account = _context.Accounts.First();
        var payee = _context.Payees.First();
        var amount = 200m;
        var scheduleTime = DateTime.UtcNow.AddDays(10);

        // Act
        await _service.CreateBillAsync(account.AccountNumber, payee.PayeeId, amount, scheduleTime, PeriodType.Monthly);

        // Assert
        var createdBill = await _context.BillPays
            .FirstOrDefaultAsync(b => b.AccountNumber == account.AccountNumber && b.Amount == amount);
        Assert.NotNull(createdBill);
        Assert.Equal(PeriodType.Monthly, createdBill.Period);
    }


    // Test that CancelBillPayAsync removes the bill pay for a valid bill pay ID and customer ID
    [Fact]
    public async Task CancelBillPayAsync_ValidBillPayId_RemovesBillPay()
    {
        // Arrange
        var customerId = 2100;
        var account = _context.Accounts.First(a => a.CustomerId == customerId);
        var payee = _context.Payees.First();

        var billPay = new BillPay
        {
            AccountNumber = account.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 100m,
            ScheduleTimeUtc = DateTime.UtcNow.AddDays(7),
            Period = PeriodType.Monthly,
            Status = StatusType.Pending
        };
        _context.BillPays.Add(billPay);
        await _context.SaveChangesAsync();

        var billPayId = billPay.BillPayId;

        // Act
        await _service.CancelBillPayAsync(billPayId, customerId);

        // Assert
        var deletedBill = await _context.BillPays.FindAsync(billPayId);
        Assert.Null(deletedBill);
    }

    // Test that CancelBillPayAsync does not remove bill if customer ID does not match
    [Fact]
    public async Task CancelBillPayAsync_DifferentCustomerId_DoesNotRemoveBill()
    {
        // Arrange
        var customerId = 2100;
        var otherCustomerId = 2200;
        var account = _context.Accounts.First(a => a.CustomerId == customerId);
        var payee = _context.Payees.First();

        var billPay = new BillPay
        {
            AccountNumber = account.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 100m,
            ScheduleTimeUtc = DateTime.UtcNow.AddDays(7),
            Period = PeriodType.Monthly,
            Status = StatusType.Pending
        };
        _context.BillPays.Add(billPay);
        await _context.SaveChangesAsync();

        var billPayId = billPay.BillPayId;

        // Act - Try to cancel with different customer ID
        await _service.CancelBillPayAsync(billPayId, otherCustomerId);

        // Assert - Bill should still exist
        var existingBill = await _context.BillPays.FindAsync(billPayId);
        Assert.NotNull(existingBill);
    }


    // Test that RetryBillPayAsync updates failed bill status to pending
    [Fact]
    public async Task RetryBillPayAsync_FailedBill_UpdatesStatusToPending()
    {
        // Arrange
        var customerId = 2100;
        var account = _context.Accounts.First(a => a.CustomerId == customerId);
        var payee = _context.Payees.First();

        var billPay = new BillPay
        {
            AccountNumber = account.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 100m,
            ScheduleTimeUtc = DateTime.UtcNow.AddDays(7),
            Period = PeriodType.Monthly,
            Status = StatusType.Failed
        };
        _context.BillPays.Add(billPay);
        await _context.SaveChangesAsync();

        var billPayId = billPay.BillPayId;

        // Act
        await _service.RetryBillPayAsync(billPayId, customerId);

        // Assert
        var updatedBill = await _context.BillPays.FindAsync(billPayId);
        Assert.NotNull(updatedBill);
        Assert.Equal(StatusType.Pending, updatedBill.Status);
    }

    // Test that RetryBillPayAsync updates blocked bill status to pending
    [Fact]
    public async Task RetryBillPayAsync_BlockedBill_UpdatesStatusToPending()
    {
        // Arrange
        var customerId = 2100;
        var account = _context.Accounts.First(a => a.CustomerId == customerId);
        var payee = _context.Payees.First();

        var billPay = new BillPay
        {
            AccountNumber = account.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 100m,
            ScheduleTimeUtc = DateTime.UtcNow.AddDays(7),
            Period = PeriodType.Monthly,
            Status = StatusType.Blocked
        };
        _context.BillPays.Add(billPay);
        await _context.SaveChangesAsync();

        var billPayId = billPay.BillPayId;

        // Act
        await _service.RetryBillPayAsync(billPayId, customerId);

        // Assert
        var updatedBill = await _context.BillPays.FindAsync(billPayId);
        Assert.NotNull(updatedBill);
        Assert.Equal(StatusType.Pending, updatedBill.Status);
    }

    // Test that RetryBillPayAsync does not update status if customer ID does not match
    [Fact]
    public async Task RetryBillPayAsync_DifferentCustomerId_DoesNotUpdateStatus()
    {
        // Arrange
        var customerId = 2100;
        var otherCustomerId = 2200;
        var account = _context.Accounts.First(a => a.CustomerId == customerId);
        var payee = _context.Payees.First();

        var billPay = new BillPay
        {
            AccountNumber = account.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 100m,
            ScheduleTimeUtc = DateTime.UtcNow.AddDays(7),
            Period = PeriodType.Monthly,
            Status = StatusType.Failed
        };
        _context.BillPays.Add(billPay);
        await _context.SaveChangesAsync();

        var billPayId = billPay.BillPayId;

        // Act - Try to retry with different customer ID
        await _service.RetryBillPayAsync(billPayId, otherCustomerId);

        // Assert - Status should remain Failed
        var existingBill = await _context.BillPays.FindAsync(billPayId);
        Assert.NotNull(existingBill);
        Assert.Equal(StatusType.Failed, existingBill.Status);
    }


    // Test that PayBillsAsync processes pending bill due now and marks it completed
    [Fact]
    public async Task PayBillsAsync_PendingBillDueNow_ProcessesBillAndMarksCompleted()
    {
        // Arrange
        var account = _context.Accounts.First();
        var initialBalance = 1000m;
        account.Balance = initialBalance; // Ensure sufficient balance
        var payee = _context.Payees.First();

        var billPay = new BillPay
        {
            AccountNumber = account.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 50m,
            ScheduleTimeUtc = DateTime.UtcNow.AddMinutes(-1), // Due now (in the past)
            Period = PeriodType.OneOff,
            Status = StatusType.Pending
        };
        _context.BillPays.Add(billPay);
        await _context.SaveChangesAsync();

        var billPayId = billPay.BillPayId;
        var initialTransactionCount = await _context.Transactions.CountAsync();

        // Act
        await _service.PayBillsAsync(CancellationToken.None);

        // Assert
        var processedBill = await _context.BillPays.FindAsync(billPayId);
        Assert.NotNull(processedBill);
        Assert.Equal(StatusType.Completed, processedBill.Status);

        // Check balance deduction
        var updatedAccount = await _context.Accounts.FindAsync(account.AccountNumber);
        Assert.NotNull(updatedAccount);
        Assert.True(updatedAccount.Balance < initialBalance);

        // Check transaction creation
        var finalTransactionCount = await _context.Transactions.CountAsync();
        Assert.True(finalTransactionCount > initialTransactionCount);

        var transaction = await _context.Transactions
            .Where(t => t.AccountNumber == account.AccountNumber)
            .OrderByDescending(t => t.TransactionTimeUtc)
            .FirstOrDefaultAsync();
        Assert.NotNull(transaction);
        Assert.Equal(TransactionType.BillPay, transaction.TransactionType);
    }

    // Test that PayBillsAsync creates a new bill for next month for monthly bills
    [Fact]
    public async Task PayBillsAsync_MonthlyBill_CreatesNewBillForNextMonth()
    {
        // Arrange
        var account = _context.Accounts.First();
        account.Balance = 1000m;
        var payee = _context.Payees.First();

        var scheduleTime = DateTime.UtcNow.AddMinutes(-1);
        var billPay = new BillPay
        {
            AccountNumber = account.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 75m,
            ScheduleTimeUtc = scheduleTime,
            Period = PeriodType.Monthly,
            Status = StatusType.Pending
        };
        _context.BillPays.Add(billPay);
        await _context.SaveChangesAsync();

        // Act
        await _service.PayBillsAsync(CancellationToken.None);

        // Assert
        var bills = await _context.BillPays
            .Where(b => b.AccountNumber == account.AccountNumber && b.PayeeId == payee.PayeeId)
            .ToListAsync();

        Assert.Equal(2, bills.Count); // Original + new monthly bill

        var completedBill = bills.First(b => b.Status == StatusType.Completed);
        var newBill = bills.First(b => b.Status == StatusType.Pending);

        Assert.Equal(scheduleTime.AddMonths(1).Date, newBill.ScheduleTimeUtc.Date);
        Assert.Equal(PeriodType.Monthly, newBill.Period);
    }

    // Test that PayBillsAsync does not create a new bill for one-off bills
    [Fact]
    public async Task PayBillsAsync_OneOffBill_DoesNotCreateNewBill()
    {
        // Arrange
        var account = _context.Accounts.First();
        account.Balance = 1000m;
        var payee = _context.Payees.First();

        var billPay = new BillPay
        {
            AccountNumber = account.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 60m,
            ScheduleTimeUtc = DateTime.UtcNow.AddMinutes(-1),
            Period = PeriodType.OneOff,
            Status = StatusType.Pending
        };
        _context.BillPays.Add(billPay);
        await _context.SaveChangesAsync();

        // Act
        await _service.PayBillsAsync(CancellationToken.None);

        // Assert
        var bills = await _context.BillPays
            .Where(b => b.AccountNumber == account.AccountNumber && b.PayeeId == payee.PayeeId)
            .ToListAsync();

        Assert.Single(bills); // Only the original bill, no new one created
        Assert.Equal(StatusType.Completed, bills[0].Status);
    }

    // Test that PayBillsAsync marks bill as failed when there are insufficient funds
    [Fact]
    public async Task PayBillsAsync_InsufficientFunds_MarksAsFailed()
    {
        // Arrange
        var account = _context.Accounts.First(a => a.AccountType == AccountType.Savings);
        account.Balance = 10m; // Insufficient balance
        var payee = _context.Payees.First();

        var billPay = new BillPay
        {
            AccountNumber = account.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 500m, // More than available
            ScheduleTimeUtc = DateTime.UtcNow.AddMinutes(-1),
            Period = PeriodType.OneOff,
            Status = StatusType.Pending
        };
        _context.BillPays.Add(billPay);
        await _context.SaveChangesAsync();

        var billPayId = billPay.BillPayId;

        // Act
        await _service.PayBillsAsync(CancellationToken.None);

        // Assert
        var processedBill = await _context.BillPays.FindAsync(billPayId);
        Assert.NotNull(processedBill);
        Assert.Equal(StatusType.Failed, processedBill.Status);
    }

    // Test that PayBillsAsync processes all due pending bills
    [Fact]
    public async Task PayBillsAsync_MultiplePendingBills_ProcessesAllDueBills()
    {
        // Arrange
        var account = _context.Accounts.First();
        account.Balance = 2000m;
        var payee = _context.Payees.First();

        var billPay1 = new BillPay
        {
            AccountNumber = account.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 50m,
            ScheduleTimeUtc = DateTime.UtcNow.AddMinutes(-10),
            Period = PeriodType.OneOff,
            Status = StatusType.Pending
        };

        var billPay2 = new BillPay
        {
            AccountNumber = account.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 75m,
            ScheduleTimeUtc = DateTime.UtcNow.AddMinutes(-5),
            Period = PeriodType.OneOff,
            Status = StatusType.Pending
        };

        _context.BillPays.AddRange(billPay1, billPay2);
        await _context.SaveChangesAsync();

        // Act
        await _service.PayBillsAsync(CancellationToken.None);

        // Assert
        var processedBills = await _context.BillPays
            .Where(b => b.BillPayId == billPay1.BillPayId || b.BillPayId == billPay2.BillPayId)
            .ToListAsync();

        Assert.All(processedBills, bill => Assert.Equal(StatusType.Completed, bill.Status));
    }


    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}