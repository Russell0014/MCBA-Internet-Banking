using MCBA.Data;
using MCBA.Models;
using MCBA.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MCBA.Tests.Services;

public class TransactionServiceTests : IDisposable
{
    private readonly DatabaseContext _context;
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        _context = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>().
            UseSqlite($"Data Source=file:{Guid.NewGuid()}?mode=memory&cache=shared").Options);

        _context.Database.EnsureCreated();

        _service = new TransactionService(_context);
    }

    [Fact]
    public void Execute_DepositTransaction_Succeeds()
    {
        // Arrange
        var customer = new Customer { CustomerId = 2100, Name = "Test Customer" };
        _context.Customers.Add(customer);

        var account = new Account
        {
            AccountNumber = 1234,
            AccountType = AccountType.Checking,
            CustomerId = 2100,
            Balance = 1000.00m
        };
        _context.Accounts.Add(account);
        _context.SaveChanges();

        var depositTransaction = new DepositTransaction
        {
            Account = account,
            Amount = 500.00m,
            Comment = "Test deposit"
        };

        // Act
        var result = _service.Execute(depositTransaction);

        // Assert
        Assert.True(result);
        Assert.Equal(1500.00m, account.Balance);

        var savedTransaction = _context.Transactions.FirstOrDefault(t => t.AccountNumber == 1234);
        Assert.NotNull(savedTransaction);
        Assert.Equal(TransactionType.Deposit, savedTransaction.TransactionType);
        Assert.Equal(500.00m, savedTransaction.Amount);
        Assert.Equal("Test deposit", savedTransaction.Comment);
    }

    [Fact]
    public void Execute_WithdrawTransaction_Succeeds()
    {
        // Arrange
        var customer = new Customer { CustomerId = 2100, Name = "Test Customer" };
        _context.Customers.Add(customer);

        var account = new Account
        {
            AccountNumber = 1234,
            AccountType = AccountType.Checking,
            CustomerId = 2100,
            Balance = 1000.00m
        };
        _context.Accounts.Add(account);
        _context.SaveChanges();

        var withdrawTransaction = new WithdrawTransaction
        {
            Account = account,
            Amount = 200.00m,
            Comment = "Test withdrawal"
        };

        // Act
        var result = _service.Execute(withdrawTransaction);

        // Assert
        Assert.True(result);
        Assert.Equal(800.00m, account.Balance); // 1000 - 200

        var transactions = _context.Transactions.Where(t => t.AccountNumber == 1234).ToList();
        Assert.Single(transactions); // Only withdrawal, no fee for first transaction

        var withdrawal = transactions.First();
        Assert.Equal(TransactionType.Withdraw, withdrawal.TransactionType);
        Assert.Equal(200.00m, withdrawal.Amount);
        Assert.Equal("Test withdrawal", withdrawal.Comment);
    }

    [Fact]
    public void Execute_TransferTransaction_Succeeds()
    {
        // Arrange
        var customer = new Customer { CustomerId = 2100, Name = "Test Customer" };
        _context.Customers.Add(customer);

        var sourceAccount = new Account
        {
            AccountNumber = 1234,
            AccountType = AccountType.Checking,
            CustomerId = 2100,
            Balance = 1000.00m
        };
        var destinationAccount = new Account
        {
            AccountNumber = 5678,
            AccountType = AccountType.Savings,
            CustomerId = 2100,
            Balance = 500.00m
        };
        _context.Accounts.AddRange(sourceAccount, destinationAccount);
        _context.SaveChanges();

        var transferTransaction = new TransferTransaction
        {
            Account = sourceAccount,
            DestinationAccount = destinationAccount,
            Amount = 300.00m,
            Comment = "Test transfer"
        };

        // Act
        var result = _service.Execute(transferTransaction);

        // Assert
        Assert.True(result);
        Assert.Equal(700.00m, sourceAccount.Balance); // 1000 - 300
        Assert.Equal(800.00m, destinationAccount.Balance); // 500 + 300

        var sourceTransactions = _context.Transactions.Where(t => t.AccountNumber == 1234).ToList();
        var destTransactions = _context.Transactions.Where(t => t.AccountNumber == 5678).ToList();

        Assert.Single(sourceTransactions); // Only transfer, no fee for first transaction
        Assert.Single(destTransactions); // Transfer only

        var transfer = sourceTransactions.First();
        Assert.Equal(TransactionType.Transfer, transfer.TransactionType);
        Assert.Equal(300.00m, transfer.Amount);
        Assert.Equal("Test transfer", transfer.Comment);
    }

    [Fact]
    public void Execute_InvalidTransaction_Fails()
    {
        // Arrange
        var customer = new Customer { CustomerId = 2100, Name = "Test Customer" };
        _context.Customers.Add(customer);

        var account = new Account
        {
            AccountNumber = 1234,
            AccountType = AccountType.Checking,
            CustomerId = 2100,
            Balance = 100.00m
        };
        _context.Accounts.Add(account);
        _context.SaveChanges();

        var withdrawTransaction = new WithdrawTransaction
        {
            Account = account,
            Amount = 1000.00m, // More than balance
            Comment = "Invalid withdrawal"
        };

        // Act
        var result = _service.Execute(withdrawTransaction);

        // Assert
        Assert.False(result);
        Assert.Equal(100.00m, account.Balance); // Balance unchanged

        var transactions = _context.Transactions.Where(t => t.AccountNumber == 1234).ToList();
        Assert.Empty(transactions); // No transactions saved
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}