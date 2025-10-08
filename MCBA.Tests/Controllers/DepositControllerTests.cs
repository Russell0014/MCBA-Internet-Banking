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
// Tests for DepositController

public class DepositControllerTests : IDisposable
{
    private readonly DatabaseContext _context;

    public DepositControllerTests()
    {
        _context = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite($"Data Source=file:{Guid.NewGuid()}?mode=memory&cache=shared").Options);

        // The EnsureCreated method creates the schema based on the current context model.
        _context.Database.EnsureCreated();

        // Create a service provider that can resolve DatabaseContext
        var services = new ServiceCollection();
        services.AddScoped(_ => _context);
        var serviceProvider = services.BuildServiceProvider();

        SeedData.Initialize(serviceProvider);
    }

    private DepositController CreateController(int customerId = 2100)
    {
        var controller = new DepositController(_context);

        // Setup session
        var httpContext = new DefaultHttpContext();
        var session = new MockHttpSession();
        session.SetInt32(nameof(Customer.CustomerId), customerId);
        httpContext.Session = session;

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Setup TempData
        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        return controller;
    }

   // test that index returns view with DepositViewModel
    [Fact]
    public async Task Index_Get_ReturnsViewWithDepositViewModel()
    {
        // Arrange
        var controller = CreateController();
        var accountNumber = 4100;

        // Act
        var result = await controller.Index(accountNumber);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DepositViewModel>(viewResult.Model);
        Assert.NotNull(model.Accounts);
    }

    // test that index post with valid model creates transaction and redirects
    [Fact]
    public async Task Index_Post_ValidModel_CreatesTransactionAndRedirects()
    {
        // Arrange
        var controller = CreateController();
        var accountNumber = 4100;
        var initialBalance = (await _context.Accounts.FindAsync(accountNumber))!.Balance;
        var depositAmount = 100m;
        var model = new DepositViewModel
        {
            AccountNumber = accountNumber,
            Amount = depositAmount
        };

        // Act
        var result = await controller.Index(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Null(redirectResult.ControllerName);

        // Verify transaction was created
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t =>
                t.AccountNumber == accountNumber && t.Amount == depositAmount &&
                t.TransactionType == TransactionType.Deposit);
        Assert.NotNull(transaction);

        // Verify balance updated
        var account = await _context.Accounts.FindAsync(accountNumber);
        Assert.Equal(initialBalance + depositAmount, account!.Balance);

        // Verify success message
        Assert.NotNull(controller.TempData["SuccessMessage"]);
    }

    // test that index post with invalid account number returns view with error
    [Fact]
    public async Task Index_Post_InvalidAccountNumber_ReturnsViewWithError()
    {
        // Arrange
        var controller = CreateController();
        var model = new DepositViewModel
        {
            AccountNumber = 9999,
            Amount = 100m
        };

        // Act
        var result = await controller.Index(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Null(redirectResult.ControllerName);
        Assert.Equal("Account not found.", controller.TempData["ErrorMessage"]);
    }

    // test that index post with non-positive amount returns view with error
    [Fact]
    public async Task Index_Post_AmountNotPositive_ReturnsViewWithError()
    {
        // Arrange
        var controller = CreateController();
        var model = new DepositViewModel
        {
            AccountNumber = 4100,
            Amount = 0m
        };

        // Act
        var result = await controller.Index(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Null(redirectResult.ControllerName);
        Assert.Equal("Deposit amount must be greater than zero.", controller.TempData["ErrorMessage"]);
    }

    // test that index post with amount greater than balance returns view with error
    [Fact]
    public async Task Index_Post_InsufficientFunds_ReturnsViewWithError()
    {
        // Arrange
        var controller = CreateController();
        var accountNumber = 4100;
        var account = await _context.Accounts.FindAsync(accountNumber);
        var largeAmount = account!.Balance + 100m; // Amount greater than balance
        var model = new DepositViewModel
        {
            AccountNumber = accountNumber,
            Amount = largeAmount
        };

        // Act
        var result = await controller.Index(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Null(redirectResult.ControllerName);
        Assert.Null(controller.TempData["ErrorMessage"]);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}