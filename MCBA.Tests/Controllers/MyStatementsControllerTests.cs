using MCBA.Controllers;
using MCBA.Data;
using MCBA.Models;
using MCBA.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MCBA.Tests.Controllers;

public class MyStatementsControllerTests : IDisposable
{
    private readonly DatabaseContext _context;

    public MyStatementsControllerTests()
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

    private MyStatementsController CreateController(int customerId = 2100)
    {
        var controller = new MyStatementsController(_context);

        // Setup session
        var httpContext = new DefaultHttpContext();
        var session = new MockHttpSession();
        session.SetInt32(nameof(Customer.CustomerId), customerId);
        httpContext.Session = session;

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    [Fact]
    public async Task Details_ReturnsViewWithPagedTransactions_WhenAccountExistsAndBelongsToCustomer()
    {
        // Arrange
        var customerId = 2100; // Using seeded customer ID
        var accountNumber = 4100; // Account that belongs to customer 2100
        var controller = CreateController(customerId);

        // Act
        var result = await controller.Details(accountNumber);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MyStatementsViewModel>(viewResult.Model);
        Assert.Equal(accountNumber, model.Account.AccountNumber);
        Assert.NotNull(model.Transactions);
        Assert.True(model.Transactions.PageNumber == 1);
    }

    [Fact]
    public async Task Details_ReturnsViewWithPagedTransactions_OnSecondPage()
    {
        // Arrange
        var customerId = 2100;
        var accountNumber = 4100;
        var page = 2;
        var controller = CreateController(customerId);

        // Act
        var result = await controller.Details(accountNumber, page);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MyStatementsViewModel>(viewResult.Model);
        Assert.Equal(accountNumber, model.Account.AccountNumber);
        Assert.Equal(page, model.Transactions.PageNumber);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenAccountDoesNotExist()
    {
        // Arrange
        var customerId = 2100;
        var accountNumber = 9999; // Non-existent account number
        var controller = CreateController(customerId);

        // Act
        var result = await controller.Details(accountNumber);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Account not found or you don't have access to it.", notFoundResult.Value);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenAccountBelongsToDifferentCustomer()
    {
        // Arrange
        var customerId = 2100;
        var accountNumber = 4200; // Account that belongs to customer 2200, not 2100
        var controller = CreateController(customerId);

        // Act
        var result = await controller.Details(accountNumber);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Account not found or you don't have access to it.", notFoundResult.Value);
    }

    [Fact]
    public async Task Details_ReturnsNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = 9999; // Non-existent customer
        var accountNumber = 4100;
        var controller = CreateController(customerId);

        // Act
        var result = await controller.Details(accountNumber);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Account not found or you don't have access to it.", notFoundResult.Value);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
