using MCBA.Controllers;
using MCBA.Data;
using MCBA.Models;
using MCBA.Services;
using MCBA.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SimpleHashing.Net;
using Xunit;

namespace MCBA.Tests.Controllers;

public class PasswordControllerTests : IDisposable
{
    private readonly DatabaseContext _context;
    private readonly PasswordService _passwordService;
    private readonly ISimpleHash _hasher;

    public PasswordControllerTests()
    {
        _context = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite($"Data Source=file:{Guid.NewGuid()}?mode=memory&cache=shared").Options);

        _context.Database.EnsureCreated();

        // Create a service provider that can resolve DatabaseContext
        var services = new ServiceCollection();
        services.AddScoped(_ => _context);
        var serviceProvider = services.BuildServiceProvider();

        SeedData.Initialize(serviceProvider);

        _hasher = new SimpleHash();
        _passwordService = new PasswordService(_context, _hasher);
    }

    private void SeedLogin(int customerId, string loginId, string password)
    {
        // Ensure customer exists
        var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == customerId);
        if (customer == null)
        {
            customer = new Customer
            {
                CustomerId = customerId,
                Name = $"Test Customer {customerId}",
                Address = "Test Address",
                City = "Test City",
                State = "VIC",
                PostCode = "3000",
                Accounts = new List<Account>()
            };
            _context.Customers.Add(customer);
            _context.SaveChanges();
        }

        var login = _context.Logins.FirstOrDefault(l => l.CustomerId == customerId);
        if (login == null)
        {
            login = new Login
            {
                LoginId = loginId,
                CustomerId = customerId,
                PasswordHash = _hasher.Compute(password)
            };
            _context.Logins.Add(login);
        }
        else
        {
            login.PasswordHash = _hasher.Compute(password);
            _context.Logins.Update(login);
        }
        _context.SaveChanges();
    }

    private PasswordController CreateController(int customerId = 2100)
    {
        var controller = new PasswordController(_passwordService);

        // Setup session
        var httpContext = new DefaultHttpContext();
        var session = new MockHttpSession();
        session.SetInt32(nameof(Customer.CustomerId), customerId);
        httpContext.Session = session;

        // Setup TempData with a mock provider
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        controller.TempData = tempData;

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    [Fact]
    public void Index_Get_ReturnsViewWithPasswordViewModel()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        // Act
        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PasswordViewModel>(viewResult.Model);
        Assert.Equal(customerId, model.CustomerId);
    }

    [Fact]
    public void Index_Post_InvalidModelState_ReturnsViewWithModel()
    {
        // Arrange
        var controller = CreateController();
        controller.ModelState.AddModelError("NewPassword", "Required");
        var model = new PasswordViewModel { CustomerId = 2100 };

        // Act
        var result = controller.Index(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
    }

    [Fact]
    public void Index_Post_LoginNotFound_ReturnsNotFound()
    {
        // Arrange
        var customerId = 2101; // Use a different customer ID that doesn't have a login
        var model = new PasswordViewModel
        {
            CustomerId = customerId,
            CurrentPassword = "oldpass",
            NewPassword = "newpass123",
            ConfirmPassword = "newpass123"
        };

        var controller = CreateController(customerId);

        // Act
        var result = controller.Index(model);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Index_Post_IncorrectCurrentPassword_ReturnsViewWithError()
    {
        // Arrange
        var customerId = 2100;
        var loginId = "12345678"; // Unique loginId
        SeedLogin(customerId, loginId, "correctpass");

        var model = new PasswordViewModel
        {
            CustomerId = customerId,
            CurrentPassword = "wrongpass",
            NewPassword = "newpass123",
            ConfirmPassword = "newpass123"
        };

        var controller = CreateController(customerId);

        // Act
        var result = controller.Index(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains("CurrentPassword", controller.ModelState.Keys);
        Assert.Equal("Current password is incorrect.", controller.ModelState["CurrentPassword"].Errors[0].ErrorMessage);
    }

    [Fact]
    public void Index_Post_NewPasswordSameAsCurrent_ReturnsViewWithError()
    {
        // Arrange
        var customerId = 2102;
        var loginId = "87654321"; // Unique loginId
        var password = "samepass";
        SeedLogin(customerId, loginId, password);

        var model = new PasswordViewModel
        {
            CustomerId = customerId,
            CurrentPassword = password,
            NewPassword = password,
            ConfirmPassword = password
        };

        var controller = CreateController(customerId);

        // Act
        var result = controller.Index(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains("NewPassword", controller.ModelState.Keys);
        Assert.Equal("New password cannot be the same as the current password.", controller.ModelState["NewPassword"].Errors[0].ErrorMessage);
    }

    [Fact]
    public void Index_Post_ValidPasswordChange_UpdatesPasswordAndReturnsViewWithSuccess()
    {
        // Arrange
        var customerId = 2103; // Use a different customer ID
        var oldPassword = "oldpass";
        var newPassword = "newpass123";

        // Create customer and login
        SeedLogin(customerId, "testlogin", oldPassword);

        var model = new PasswordViewModel
        {
            CustomerId = customerId,
            CurrentPassword = oldPassword,
            NewPassword = newPassword,
            ConfirmPassword = newPassword
        };

        var controller = CreateController(customerId);

        // Act
        var result = controller.Index(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        Assert.True(controller.ModelState.IsValid);
        Assert.Equal("Password updated successfully!", controller.TempData["SuccessMessage"]);
    }

    [Fact]
    public void Index_Post_ModelValidationErrors_ReturnsViewWithModel()
    {
        // Arrange
        var customerId = 2100;
        var model = new PasswordViewModel
        {
            CustomerId = customerId,
            CurrentPassword = "oldpass",
            NewPassword = "new", // Too short
            ConfirmPassword = "different" // Doesn't match
        };

        var controller = CreateController(customerId);
        controller.ModelState.AddModelError("NewPassword", "Password must be at least 6 characters.");
        controller.ModelState.AddModelError("ConfirmPassword", "The new password and confirmation password do not match.");

        // Act
        var result = controller.Index(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        Assert.False(controller.ModelState.IsValid);
    }
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
