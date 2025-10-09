using MCBA.Data;
using MCBA.Models;
using MCBA.Services;
using MCBA.ViewModel;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MCBA.Tests.Services;

public class MyStatementsServiceTests : IDisposable
{
    private readonly DatabaseContext _context;
    private readonly MyStatementsService _service;

    public MyStatementsServiceTests()
    {
        _context = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>().
            UseSqlite($"Data Source=file:{Guid.NewGuid()}?mode=memory&cache=shared").Options);

        _context.Database.EnsureCreated();

        _service = new MyStatementsService(_context);
    }

    // test that GetCustomerAsync returns a customer when given an existing customerId

    [Fact]
    public async Task GetCustomerAsync_ExistingCustomer_ReturnsCustomer()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = 2100,
            Name = "John Doe"
        };
        _context.Customers.Add(customer);
        _context.SaveChanges();

        // Act
        var result = await _service.GetCustomerAsync(2100);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2100, result.CustomerId);
        Assert.Equal("John Doe", result.Name);
    }

    [Fact]
    public async Task GetCustomerAsync_NonExistingCustomer_ReturnsNull()
    {
        // Act
        var result = await _service.GetCustomerAsync(9999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAccountAsync_ExistingAccount_ReturnsAccount()
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

        // Act
        var result = await _service.GetAccountAsync(1234, 2100);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1234, result.AccountNumber);
        Assert.Equal(AccountType.Checking, result.AccountType);
        Assert.Equal(2100, result.CustomerId);
        Assert.Equal(1000.00m, result.Balance);
    }

    [Fact]
    public async Task GetAccountAsync_WrongCustomerId_ReturnsNull()
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

        // Act
        var result = await _service.GetAccountAsync(1234, 9999); // Wrong customer ID

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAccountAsync_NonExistingAccount_ReturnsNull()
    {
        // Act
        var result = await _service.GetAccountAsync(9999, 2100);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPagedTransactionsAsync_ValidAccount_ReturnsPagedTransactions()
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

        // Add multiple transactions
        var transactions = new List<Transaction>();
        for (int i = 1; i <= 10; i++)
        {
            transactions.Add(new Transaction
            {
                TransactionId = i,
                AccountNumber = 1234,
                TransactionType = TransactionType.Deposit,
                Amount = 100.00m * i,
                TransactionTimeUtc = DateTime.UtcNow.AddMinutes(-i),
                Comment = $"Transaction {i}"
            });
        }
        _context.Transactions.AddRange(transactions);
        _context.SaveChanges();

        // Act
        var result = await _service.GetPagedTransactionsAsync(2100, 1234, 1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(account, result.Account);
        Assert.NotNull(result.Transactions);
        Assert.Equal(4, result.Transactions.Count); // Page size is 4
        Assert.Equal(10, result.Transactions.TotalItemCount);
        Assert.Equal(3, result.Transactions.PageCount); // 10 items / 4 per page = 3 pages

        // Check ordering (most recent first)
        var firstTransaction = result.Transactions.First();
        var lastTransaction = result.Transactions.Last();
        Assert.True(firstTransaction.TransactionTimeUtc > lastTransaction.TransactionTimeUtc);
    }

    [Fact]
    public async Task GetPagedTransactionsAsync_SecondPage_ReturnsCorrectPage()
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

        // Add multiple transactions
        var transactions = new List<Transaction>();
        for (int i = 1; i <= 10; i++)
        {
            transactions.Add(new Transaction
            {
                TransactionId = i,
                AccountNumber = 1234,
                TransactionType = TransactionType.Deposit,
                Amount = 100.00m * i,
                TransactionTimeUtc = DateTime.UtcNow.AddMinutes(-i),
                Comment = $"Transaction {i}"
            });
        }
        _context.Transactions.AddRange(transactions);
        _context.SaveChanges();

        // Act
        var result = await _service.GetPagedTransactionsAsync(2100, 1234, 2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(account, result.Account);
        Assert.NotNull(result.Transactions);
        Assert.Equal(4, result.Transactions.Count); // Page size is 4
        Assert.Equal(2, result.Transactions.PageNumber);
    }

    [Fact]
    public async Task GetPagedTransactionsAsync_WrongCustomerId_ReturnsNull()
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

        // Act
        var result = await _service.GetPagedTransactionsAsync(9999, 1234, 1); // Wrong customer ID

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPagedTransactionsAsync_NonExistingAccount_ReturnsNull()
    {
        // Act
        var result = await _service.GetPagedTransactionsAsync(2100, 9999, 1);

        // Assert
        Assert.Null(result);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}