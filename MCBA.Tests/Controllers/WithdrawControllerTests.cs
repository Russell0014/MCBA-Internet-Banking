using MCBA.Controllers;
using MCBA.Data;
using MCBA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MCBA.Tests.Controllers;

// Tests for WithdrawController
public class WithdrawControllerTests : IDisposable
{
    private readonly DatabaseContext _context;

    public WithdrawControllerTests()
    {
        _context = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite($"Data Source=file:{Guid.NewGuid()}?mode=memory&cache=shared").Options);

        _context.Database.EnsureCreated();

        // Create a service provider that can resolve DatabaseContext
        var services = new ServiceCollection();
        services.AddScoped(_ => _context);
        var serviceProvider = services.BuildServiceProvider();

        SeedData.Initialize(serviceProvider);
    }

    private WithdrawController CreateController(int customerId = 2100)
    {
        var controller = new WithdrawController(_context);

        // Setup session
        var httpContext = new DefaultHttpContext();
        var session = new MockHttpSession();
        session.SetInt32(nameof(Customer.CustomerId), customerId);
        httpContext.Session = session;

        // Setup TempData
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        controller.TempData = tempData;

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

   // Test that Index GET returns view with WithdrawViewModel
    [Fact]
    public void Index_Get_ReturnsViewWithWithdrawViewModel()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        // Act
        var result = controller.Index(4100); // Using existing account number from seed data

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<WithdrawViewModel>(viewResult.Model);
        Assert.NotNull(model.Accounts);
    }

// Test that Index POST with valid withdrawal successfully withdraws funds
    [Fact]
    public async Task Index_Post_ValidWithdrawal_SuccessfullyWithdrawsFunds()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var account = _context.Accounts.First(a => a.CustomerId == customerId);
        var initialBalance = account.Balance;
        var withdrawAmount = 100m;

        var model = new WithdrawViewModel
        {
            AccountNumber = account.AccountNumber,
            Amount = withdrawAmount,
            Comment = "Test withdrawal"
        };

        // Act
        var result = await controller.Index(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Withdrawal successful!", controller.TempData["SuccessMessage"]);

        // Verify balance updated correctly
        _context.Entry(account).Reload();

        // Balance should decrease by amount + fee (if applicable)
        Assert.True(account.Balance < initialBalance);
        Assert.True(account.Balance <= initialBalance - withdrawAmount);
    }

// Test that Index POST with insufficient funds returns error
    [Fact]
    public async Task Index_Post_InsufficientFunds_ReturnsError()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var account = _context.Accounts.First(a => a.CustomerId == customerId);

        // Try to withdraw more than available balance (considering min balance rules)
        var withdrawAmount = account.Balance + 10000m;

        var model = new WithdrawViewModel
        {
            AccountNumber = account.AccountNumber,
            Amount = withdrawAmount,
            Comment = "Test withdrawal"
        };

        // Act
        var result = await controller.Index(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.NotNull(controller.TempData["ErrorMessage"]);
        Assert.Contains("Insufficient funds", controller.TempData["ErrorMessage"]?.ToString());
    }

// Test that Index POST with invalid amount (zero or negative) returns error
    [Fact]
    public async Task Index_Post_InvalidAmount_ReturnsError()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var account = _context.Accounts.First(a => a.CustomerId == customerId);

        // Test zero amount
        var zeroModel = new WithdrawViewModel
        {
            AccountNumber = account.AccountNumber,
            Amount = 0m,
            Comment = "Test withdrawal"
        };

        // Act
        var result = await controller.Index(zeroModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.NotNull(controller.TempData["ErrorMessage"]);
        Assert.Contains("Withdrawal amount must be greater than zero", controller.TempData["ErrorMessage"]?.ToString());

        // Test negative amount
        var negativeModel = new WithdrawViewModel
        {
            AccountNumber = account.AccountNumber,
            Amount = -50m,
            Comment = "Test withdrawal"
        };

        // Act
        result = await controller.Index(negativeModel);

        // Assert
        redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.NotNull(controller.TempData["ErrorMessage"]);
        Assert.Contains("Withdrawal amount must be greater than zero", controller.TempData["ErrorMessage"]?.ToString());
    }

// Test that Index POST with non-existent account returns NotFound
    [Fact]
    public async Task Index_Post_NonExistentAccount_ReturnsNotFound()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var model = new WithdrawViewModel
        {
            AccountNumber = 9999, // Non-existent account
            Amount = 50m,
            Comment = "Test withdrawal"
        };

        // Act
        var result = await controller.Index(model);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

// Test that Index POST applies fee after free withdrawals are used up
    [Fact]
    public async Task Index_Post_AppliesFeeAfterFreeWithdrawals()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var account = _context.Accounts.First(a => a.CustomerId == customerId);

        // Perform 2 free withdrawals first
        for (int i = 0; i < 2; i++)
        {
            var freeModel = new WithdrawViewModel
            {
                AccountNumber = account.AccountNumber,
                Amount = 10m,
                Comment = $"Free withdrawal {i + 1}"
            };
            await controller.Index(freeModel);
            _context.Entry(account).Reload();
        }

        var balanceBeforeFeeWithdrawal = account.Balance;
        var withdrawAmount = 10m;

        // Third withdrawal should have a fee
        var model = new WithdrawViewModel
        {
            AccountNumber = account.AccountNumber,
            Amount = withdrawAmount,
            Comment = "Withdrawal with fee"
        };

        // Act
        var result = await controller.Index(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Withdrawal successful!", controller.TempData["SuccessMessage"]);

        _context.Entry(account).Reload();

        // Balance should decrease by amount + fee (0.01)
        var expectedBalance = balanceBeforeFeeWithdrawal - withdrawAmount - TransactionRules.AtmWithdrawFee;
        Assert.Equal(expectedBalance, account.Balance);

        // Verify service charge transaction was created
        var serviceChargeTransaction = _context.Transactions
            .FirstOrDefault(t => t.Account.AccountNumber == account.AccountNumber
                                && t.TransactionType == TransactionType.ServiceCharge
                                && t.Comment == "Withdraw Fee");

        Assert.NotNull(serviceChargeTransaction);
        Assert.Equal(TransactionRules.AtmWithdrawFee, serviceChargeTransaction.Amount);
    }

// Test that Index POST allows overdraft up to limit for checking accounts
    [Fact]
    public async Task Index_Post_CheckingAccount_AllowsOverdraftUpToLimit()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        // Find a checking account (should allow -500 overdraft)
        var checkingAccount = _context.Accounts
            .FirstOrDefault(a => a.CustomerId == customerId && a.AccountType == AccountType.Checking);

        if (checkingAccount != null)
        {
            // Set balance to small amount
            checkingAccount.Balance = 100m;
            _context.SaveChanges();

            // Try to withdraw amount that would put account in overdraft but within limit
            var model = new WithdrawViewModel
            {
                AccountNumber = checkingAccount.AccountNumber,
                Amount = 400m, // Would result in -300 balance (within -500 limit)
                Comment = "Overdraft test"
            };

            // Act
            var result = await controller.Index(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Withdrawal successful!", controller.TempData["SuccessMessage"]);

            _context.Entry(checkingAccount).Reload();
            Assert.True(checkingAccount.Balance < 0); // Should be negative but valid
            Assert.True(checkingAccount.Balance >= -500m); // Should not exceed overdraft limit
        }
    }

// Test that Index POST does not allow overdraft for savings accounts
    [Fact]
    public async Task Index_Post_SavingsAccount_DoesNotAllowNegativeBalance()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        // Find a savings account (should not allow negative balance)
        var savingsAccount = _context.Accounts
            .FirstOrDefault(a => a.CustomerId == customerId && a.AccountType == AccountType.Savings);

        if (savingsAccount != null)
        {
            // Set balance to small amount
            var initialBalance = savingsAccount.Balance;

            // Try to withdraw more than balance
            var model = new WithdrawViewModel
            {
                AccountNumber = savingsAccount.AccountNumber,
                Amount = initialBalance + 10m, // More than available
                Comment = "Test overdraft prevention"
            };

            // Act
            var result = await controller.Index(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.NotNull(controller.TempData["ErrorMessage"]);
            Assert.Contains("Insufficient funds", controller.TempData["ErrorMessage"]?.ToString());

            _context.Entry(savingsAccount).Reload();
            Assert.Equal(initialBalance, savingsAccount.Balance); // Balance should be unchanged
        }
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
