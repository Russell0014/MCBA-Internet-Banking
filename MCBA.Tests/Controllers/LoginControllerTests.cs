using MCBA.Controllers;
using MCBA.Data;
using MCBA.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MCBA.Tests.Controllers;

public class LoginControllerTests : IDisposable
{
    private readonly DatabaseContext _context;

    public LoginControllerTests()
    {
        _context = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>().
            UseSqlite($"Data Source=file:{Guid.NewGuid()}?mode=memory&cache=shared").Options);

        // The EnsureCreated method creates the schema based on the current context model.
        _context.Database.EnsureCreated();

        // Create a service provider that can resolve DatabaseContext
        var services = new ServiceCollection();
        services.AddScoped(_ => _context);
        var serviceProvider = services.BuildServiceProvider();

        SeedData.Initialize(serviceProvider);

        // Note: Logins are seeded with customers
    }

    private LoginController CreateController()
    {
        var controller = new LoginController(_context);

        // Setup session
        var httpContext = new DefaultHttpContext();
        var session = new MockHttpSession();
        httpContext.Session = session;

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    [Fact]
    public void Login_Get_NotLoggedIn_ReturnsView()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Login();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Login_Get_LoggedIn_RedirectsToCustomer()
    {
        // Arrange
        var controller = CreateController();
        var session = controller.HttpContext.Session as MockHttpSession;
        session!.SetInt32("CustomerId", 2100);

        // Act
        var result = controller.Login();

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Customer", redirectResult.ControllerName);
    }

    [Fact]
    public async Task Login_Post_ValidLogin_SetsSessionAndRedirects()
    {
        // Arrange
        var controller = CreateController();
        var loginId = "12345678";
        var password = "abc123";

        // Act
        var result = await controller.Login(loginId, password);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Customer", redirectResult.ControllerName);

        // Verify session
        var session = controller.HttpContext.Session as MockHttpSession;
        Assert.NotNull(session!.GetInt32("CustomerId"));
    }

    [Fact]
    public async Task Login_Post_InvalidLogin_ReturnsViewWithError()
    {
        // Arrange
        var controller = CreateController();
        var loginId = "invalid";
        var password = "wrong";

        // Act
        var result = await controller.Login(loginId, password);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey("LoginFailed"));
        var model = Assert.IsType<Login>(viewResult.Model);
        Assert.Equal(loginId, model.LoginId);
    }

    [Fact]
    public async Task Login_Post_EmptyPassword_ReturnsViewWithError()
    {
        // Arrange
        var controller = CreateController();
        var loginId = "12345678";
        var password = "";

        // Act
        var result = await controller.Login(loginId, password);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        var model = Assert.IsType<Login>(viewResult.Model);
        Assert.Equal(loginId, model.LoginId);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}