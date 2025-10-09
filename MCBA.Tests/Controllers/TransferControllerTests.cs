using MCBA.Controllers;
using MCBA.Data;
using MCBA.Models;
using MCBA.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace MCBA.Tests.Controllers;

// Tests for TransferController
public class TransferControllerTests : IDisposable
{
    private readonly DatabaseContext _context;

    public TransferControllerTests()
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

    private TransferController CreateController(int customerId = 2100)
    {
        var controller = new TransferController(_context);

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

    // test that index returns view with TransferViewModel
    [Fact]
    public void Index_Get_ReturnsViewWithTransferViewModel()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        // Act
        var result = controller.Index(4100); // Using existing account number from seed data

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<TransferViewModel>(viewResult.Model);
        Assert.NotNull(model.Accounts);
    }

    // test that post index with valid transfer updates balances and redirects
    [Fact]
    public async Task Index_Post_ValidTransfer_SuccessfullyTransfersFunds()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var sourceAccount = _context.Accounts.First(a => a.CustomerId == customerId);
        var destAccount = _context.Accounts.First(a => a.AccountNumber != sourceAccount.AccountNumber);

        var initialSourceBalance = sourceAccount.Balance;
        var initialDestBalance = destAccount.Balance;
        var transferAmount = 100m;

        var model = new TransferViewModel
        {
            AccountNumber = sourceAccount.AccountNumber,
            DestAccountNumber = destAccount.AccountNumber.ToString(),
            Amount = transferAmount,
            Comment = "Test transfer"
        };

        // Act
        var result = await controller.Index(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Transfer successful!", controller.TempData["SuccessMessage"]);

        // Verify balances updated correctly
        _context.Entry(sourceAccount).Reload();
        _context.Entry(destAccount).Reload();

        // Source should decrease by amount + fee (if applicable)
        Assert.True(sourceAccount.Balance < initialSourceBalance);

        // Destination should increase by exact transfer amount
        Assert.Equal(initialDestBalance + transferAmount, destAccount.Balance);
    }

    // test that post index with insufficient funds returns error
    [Fact]
    public async Task Index_Post_InsufficientFunds_ReturnsError()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var sourceAccount = _context.Accounts.First(a => a.CustomerId == customerId);
        var destAccount = _context.Accounts.First(a => a.AccountNumber != sourceAccount.AccountNumber);

        // Try to transfer more than available balance
        var transferAmount = sourceAccount.Balance + 10000m;

        var model = new TransferViewModel
        {
            AccountNumber = sourceAccount.AccountNumber,
            DestAccountNumber = destAccount.AccountNumber.ToString(),
            Amount = transferAmount,
            Comment = "Test transfer"
        };

        // Act
        var result = await controller.Index(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.NotNull(controller.TempData["ErrorMessage"]);
        Assert.Contains("Insufficient funds", controller.TempData["ErrorMessage"]?.ToString());
    }

    // test that post index with same source and destination account returns error
    [Fact]
    public async Task Index_Post_TransferToSameAccount_ReturnsError()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var sourceAccount = _context.Accounts.First(a => a.CustomerId == customerId);

        var model = new TransferViewModel
        {
            AccountNumber = sourceAccount.AccountNumber,
            DestAccountNumber = sourceAccount.AccountNumber.ToString(), // Same account
            Amount = 50m,
            Comment = "Test transfer"
        };

        // Act
        var result = await controller.Index(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.NotNull(controller.TempData["ErrorMessage"]);
        Assert.Contains("Cannot transfer to the same account", controller.TempData["ErrorMessage"]?.ToString());
    }

    // test that post index with invalid destination account returns error
    [Fact]
    public async Task Index_Post_InvalidDestinationAccount_ReturnsError()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var sourceAccount = _context.Accounts.First(a => a.CustomerId == customerId);

        var model = new TransferViewModel
        {
            AccountNumber = sourceAccount.AccountNumber,
            DestAccountNumber = "9999", // Non-existent account
            Amount = 50m,
            Comment = "Test transfer"
        };

        // Act
        var result = await controller.Index(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.NotNull(controller.TempData["ErrorMessage"]);
        Assert.Contains("Invalid destination account number.", controller.TempData["ErrorMessage"]?.ToString());
    }

    // test that post index with invalid amount returns error
    [Fact]
    public async Task Index_Post_InvalidAmount_ReturnsError()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var sourceAccount = _context.Accounts.First(a => a.CustomerId == customerId);
        var destAccount = _context.Accounts.First(a => a.AccountNumber != sourceAccount.AccountNumber);

        // Test zero amount
        var zeroModel = new TransferViewModel
        {
            AccountNumber = sourceAccount.AccountNumber,
            DestAccountNumber = destAccount.AccountNumber.ToString(),
            Amount = 0m,
            Comment = "Test transfer"
        };

        // Act
        var result = await controller.Index(zeroModel);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.NotNull(controller.TempData["ErrorMessage"]);
        Assert.Contains("Amount must be greater than zero", controller.TempData["ErrorMessage"]?.ToString());

        // Test negative amount
        var negativeModel = new TransferViewModel
        {
            AccountNumber = sourceAccount.AccountNumber,
            DestAccountNumber = destAccount.AccountNumber.ToString(),
            Amount = -50m,
            Comment = "Test transfer"
        };

        // Act
        result = await controller.Index(negativeModel);

        // Assert
        redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.NotNull(controller.TempData["ErrorMessage"]);
        Assert.Contains("Amount must be greater than zero", controller.TempData["ErrorMessage"]?.ToString());
    }

    // test that post index applies fee after 2 free transfers
    [Fact]
    public async Task Index_Post_AppliesFeeAfterFreeTransfers()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var sourceAccount = _context.Accounts.First(a => a.CustomerId == customerId);
        var destAccount = _context.Accounts.First(a => a.AccountNumber != sourceAccount.AccountNumber);

        // Perform 2 free transfers first
        for (int i = 0; i < 2; i++)
        {
            var freeModel = new TransferViewModel
            {
                AccountNumber = sourceAccount.AccountNumber,
                DestAccountNumber = destAccount.AccountNumber.ToString(),
                Amount = 10m,
                Comment = $"Free transfer {i + 1}"
            };
            await controller.Index(freeModel);
            _context.Entry(sourceAccount).Reload();
        }

        var balanceBeforeFeeTransfer = sourceAccount.Balance;
        var transferAmount = 10m;

        // Third transfer should have a fee
        var model = new TransferViewModel
        {
            AccountNumber = sourceAccount.AccountNumber,
            DestAccountNumber = destAccount.AccountNumber.ToString(),
            Amount = transferAmount,
            Comment = "Transfer with fee"
        };

        // Act
        var result = await controller.Index(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Transfer successful!", controller.TempData["SuccessMessage"]);

        _context.Entry(sourceAccount).Reload();

        // Balance should decrease by amount + fee (0.05)
        var expectedBalance = balanceBeforeFeeTransfer - transferAmount - TransactionRules.AtmTransferFee;
        Assert.Equal(expectedBalance, sourceAccount.Balance);

        // Verify service charge transaction was created
        var serviceChargeTransaction = _context.Transactions
            .FirstOrDefault(t => t.Account.AccountNumber == sourceAccount.AccountNumber
                                && t.TransactionType == TransactionType.ServiceCharge
                                && t.Comment == "Transfer Fee");

        Assert.NotNull(serviceChargeTransaction);
        Assert.Equal(TransactionRules.AtmTransferFee, serviceChargeTransaction.Amount);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
