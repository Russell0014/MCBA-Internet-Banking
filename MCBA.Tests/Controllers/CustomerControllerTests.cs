using MCBA.Controllers;
using MCBA.Data;
using MCBA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace MCBA.Tests.Controllers;

public class CustomerControllerTests : IDisposable
{
    private readonly DatabaseContext _context;

    public CustomerControllerTests()
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

    private CustomerController CreateController(int customerId = 2100)
    {
        var controller = new CustomerController(_context);

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
    public async Task Index_ReturnsViewWithCustomer_WhenCustomerExists()
    {
        // Arrange
        var customerId = 9999;
        var customer = new Customer { CustomerId = customerId, Name = "Test Customer" };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var controller = CreateController(customerId);

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Customer>(viewResult.Model);
        Assert.Equal(customerId, model.CustomerId);
    }

    [Fact]
    public async Task Index_ReturnsViewWithNull_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = 9998;
        var controller = CreateController(customerId);

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.Model);
    }

    [Fact]
    public async Task Index_ThrowsException_WhenSessionCustomerIdIsNull()
    {
        // Arrange
        var controller = new CustomerController(_context);

        // Setup session with null CustomerId
        var httpContext = new DefaultHttpContext();
        var session = new MockHttpSession();
        // Do not set CustomerId, so GetInt32 returns null
        httpContext.Session = session;

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.Index());
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}