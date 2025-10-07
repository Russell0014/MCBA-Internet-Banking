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

public class ProfileControllerTests : IDisposable
{
    private readonly DatabaseContext _context;

    public ProfileControllerTests()
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

    private ProfileController CreateController(int customerId = 2100)
    {
        var controller = new ProfileController(_context);

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

    [Fact]
    public void Index_Get_ReturnsViewWithProfileViewModel()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        // Act
        var result = controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProfileViewModel>(viewResult.Model);
        Assert.Equal(customerId, model.CustomerId);
        Assert.NotNull(model.Name);
    }

    [Fact]
    public void Index_Get_CustomerNotFound_ReturnsNotFound()
    {
        // Arrange
        var customerId = 9999; // Non-existent customer
        var controller = CreateController(customerId);

        // Act
        var result = controller.Index();

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_ReturnsViewWithProfileViewModel()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        // Act
        var result = await controller.Edit();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ProfileViewModel>(viewResult.Model);
        Assert.Equal(customerId, model.CustomerId);
        Assert.NotNull(model.Name);
    }

    [Fact]
    public async Task Edit_Get_CustomerNotFound_ReturnsNotFound()
    {
        // Arrange
        var customerId = 9999; // Non-existent customer
        var controller = CreateController(customerId);

        // Act
        var result = await controller.Edit();

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_ValidModel_UpdatesCustomerAndRedirects()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var model = new ProfileViewModel
        {
            CustomerId = customerId,
            Name = "Updated Name",
            Address = "123 New Street",
            City = "Melbourne",
            State = "VIC",
            PostCode = "3000",
            Mobile = "0412 345 678"
        };

        // Act
        var result = await controller.Edit(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(ProfileController.Index), redirectResult.ActionName);

        // Verify customer was updated
        var customer = await _context.Customers.FindAsync(customerId);
        Assert.NotNull(customer);
        Assert.Equal("Updated Name", customer.Name);
        Assert.Equal("123 New Street", customer.Address);
        Assert.Equal("Melbourne", customer.City);
        Assert.Equal("VIC", customer.State);
        Assert.Equal("3000", customer.PostCode);
        Assert.Equal("0412 345 678", customer.Mobile);
    }

    [Fact]
    public async Task Edit_Post_InvalidModelState_ReturnsViewWithModel()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);
        controller.ModelState.AddModelError("Name", "Required");

        var model = new ProfileViewModel
        {
            CustomerId = customerId,
            Name = "", // Invalid - required
            Address = "123 Street"
        };

        // Act
        var result = await controller.Edit(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(model, viewResult.Model);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Edit_Post_CustomerNotFound_ReturnsNotFound()
    {
        // Arrange
        var customerId = 9999; // Non-existent customer
        var controller = CreateController(customerId);

        var model = new ProfileViewModel
        {
            CustomerId = customerId,
            Name = "Test Name",
            Address = "123 Street"
        };

        // Act
        var result = await controller.Edit(model);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_ValidatesStateFormat()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var model = new ProfileViewModel
        {
            CustomerId = customerId,
            Name = "Test Name",
            Address = "123 Street",
            State = "INVALID" // Should fail regex validation
        };

        // Manually trigger validation
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(model);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
            model, validationContext, validationResults, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, v => v.MemberNames.Contains("State"));
    }

    [Fact]
    public async Task Edit_Post_ValidatesPostCodeFormat()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var model = new ProfileViewModel
        {
            CustomerId = customerId,
            Name = "Test Name",
            Address = "123 Street",
            PostCode = "12345" // Should be exactly 4 digits
        };

        // Manually trigger validation
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(model);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
            model, validationContext, validationResults, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, v => v.MemberNames.Contains("PostCode"));
    }

    [Fact]
    public async Task Edit_Post_ValidatesMobileFormat()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var model = new ProfileViewModel
        {
            CustomerId = customerId,
            Name = "Test Name",
            Address = "123 Street",
            Mobile = "1234567890" // Should match 04XX XXX XXX format
        };

        // Manually trigger validation
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(model);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
            model, validationContext, validationResults, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Mobile"));
    }

    [Fact]
    public async Task Edit_Post_AcceptsValidAustralianMobile()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var model = new ProfileViewModel
        {
            CustomerId = customerId,
            Name = "Test Name",
            Address = "123 Street",
            Mobile = "0412 345 678" // Valid format
        };

        // Manually trigger validation
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(model);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
            model, validationContext, validationResults, true);

        // Assert - Mobile should be valid (other fields might fail but not Mobile)
        Assert.DoesNotContain(validationResults, v => v.MemberNames.Contains("Mobile"));
    }

    [Fact]
    public async Task Edit_Post_AcceptsAllValidAustralianStates()
    {
        // Arrange
        var validStates = new[] { "NSW", "VIC", "QLD", "SA", "WA", "TAS", "ACT", "NT" };

        foreach (var state in validStates)
        {
            var model = new ProfileViewModel
            {
                CustomerId = 2100,
                Name = "Test Name",
                Address = "123 Street",
                State = state
            };

            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(model);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
                model, validationContext, validationResults, true);

            // Assert - State should be valid
            Assert.DoesNotContain(validationResults, v => v.MemberNames.Contains("State"));
        }
    }

    [Fact]
    public async Task Edit_Post_UpdatesOnlyProvidedFields()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        // Get original customer data
        var originalCustomer = await _context.Customers.FindAsync(customerId);
        var originalMobile = originalCustomer?.Mobile;

        var model = new ProfileViewModel
        {
            CustomerId = customerId,
            Name = "Changed Name Only",
            Address = originalCustomer!.Address,
            City = originalCustomer.City,
            State = originalCustomer.State,
            PostCode = originalCustomer.PostCode,
            Mobile = null // Setting to null
        };

        // Act
        var result = await controller.Edit(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);

        // Verify only name was changed, mobile was set to null
        var updatedCustomer = await _context.Customers.FindAsync(customerId);
        Assert.Equal("Changed Name Only", updatedCustomer!.Name);
        Assert.Null(updatedCustomer.Mobile);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
