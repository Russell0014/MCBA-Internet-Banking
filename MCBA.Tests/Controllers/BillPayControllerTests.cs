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

// Tests for BillPayController
public class BillPayControllerTests : IDisposable
{
    private readonly DatabaseContext _context;

    public BillPayControllerTests()
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

    private BillPayController CreateController(int customerId = 2100)
    {
        var controller = new BillPayController(_context);

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

    // test the GET Index action returns the correct view with the correct model
    [Fact]
    public async Task Index_Get_ReturnsViewWithBillPayViewModel()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BillPayViewModel>(viewResult.Model);
        Assert.NotNull(model);
    }

    // test the GET Index action returns bill pays for the customer's accounts
    [Fact]
    public async Task Index_Get_DisplaysBillPaysForCustomerAccounts()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        // Add a test bill pay
        var account = _context.Accounts.First(a => a.CustomerId == customerId);
        var payee = _context.Payees.First();
        var billPay = new BillPay
        {
            AccountNumber = account.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 100m,
            ScheduleTimeUtc = DateTime.UtcNow.AddDays(7),
            Period = PeriodType.Monthly,
            Status = StatusType.Pending
        };
        _context.BillPays.Add(billPay);
        await _context.SaveChangesAsync();

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BillPayViewModel>(viewResult.Model);
        Assert.NotNull(model.BillPayList);
        Assert.Contains(model.BillPayList, b => b.BillPayId == billPay.BillPayId);
    }

    // test the GET Create action returns the correct view with the correct model
    [Fact]
    public async Task Create_Get_ReturnsViewWithCreateBillPayViewModel()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        // Act
        var result = await controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<CreateBillPayViewModel>(viewResult.Model);
        Assert.NotNull(model);
        Assert.NotNull(model.Accounts);
        Assert.Equal(PeriodType.OneOff, model.Period);
    }

// test the POST Create action with valid model creates a new bill pay and redirects to Index
    [Fact]
    public async Task Create_Post_ValidBill_CreatesNewBillPayAndRedirects()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var account = _context.Accounts.First(a => a.CustomerId == customerId);
        var payee = _context.Payees.First();

        var model = new CreateBillPayViewModel
        {
            AccountNumber = account.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 150.50m,
            ScheduleTimeUtc = DateTime.UtcNow.AddDays(10),
            Period = PeriodType.Monthly
        };

        // Act
        var result = await controller.Create(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);

        // Verify bill was created
        var createdBill = await _context.BillPays
            .FirstOrDefaultAsync(b => b.AccountNumber == account.AccountNumber && b.Amount == 150.50m);
        Assert.NotNull(createdBill);
        Assert.Equal(payee.PayeeId, createdBill.PayeeId);
        Assert.Equal(PeriodType.Monthly, createdBill.Period);
        Assert.Equal(StatusType.Pending, createdBill.Status);
    }

    // test the POST Create action with invalid model state returns the view with the model
    [Fact]
    public async Task Create_Post_InvalidModelState_ReturnsViewWithModel()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);
        controller.ModelState.AddModelError("Amount", "Amount is required");

        var model = new CreateBillPayViewModel
        {
            AccountNumber = 4100,
            PayeeId = 1,
            Period = PeriodType.Monthly
        };

        // Act
        var result = await controller.Create(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
        Assert.False(controller.ModelState.IsValid);
    }

    // test the POST Create action with invalid payee id returns TempData error and the view with the model
    [Fact]
    public async Task Create_Post_InvalidPayeeId_ReturnsTempDataErrorAndView()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var account = _context.Accounts.First(a => a.CustomerId == customerId);

        var model = new CreateBillPayViewModel
        {
            AccountNumber = account.AccountNumber,
            PayeeId = 99999, // Non-existent payee
            Amount = 100m,
            ScheduleTimeUtc = DateTime.UtcNow.AddDays(7),
            Period = PeriodType.Monthly
        };

        // Act
        var result = await controller.Create(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
        Assert.Equal("Invalid Payee Number", controller.TempData["ErrorMessage"]);

        // Verify no bill was created
        var billCount = await _context.BillPays.CountAsync(b => b.PayeeId == 99999);
        Assert.Equal(0, billCount);
    }

// test the POST Create action with schedule time in the past returns TempData error and the view with the model
    [Fact]
    public async Task Create_Post_ScheduleTimeInPast_ReturnsTempDataErrorAndView()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var account = _context.Accounts.First(a => a.CustomerId == customerId);
        var payee = _context.Payees.First();

        var model = new CreateBillPayViewModel
        {
            AccountNumber = account.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 100m,
            ScheduleTimeUtc = DateTime.UtcNow.AddDays(-1), // Past date
            Period = PeriodType.Monthly
        };

        // Act
        var result = await controller.Create(model);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
        Assert.Equal("Please enter a schedule time in the future.", controller.TempData["ErrorMessage"]);
    }

    // test the Cancel action with valid bill pay id removes the bill pay and redirects to Index
    [Fact]
    public async Task Cancel_ValidBillPayId_RemovesBillPayAndRedirects()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var account = _context.Accounts.First(a => a.CustomerId == customerId);
        var payee = _context.Payees.First();
        var billPay = new BillPay
        {
            AccountNumber = account.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 100m,
            ScheduleTimeUtc = DateTime.UtcNow.AddDays(7),
            Period = PeriodType.Monthly,
            Status = StatusType.Pending
        };
        _context.BillPays.Add(billPay);
        await _context.SaveChangesAsync();

        var billPayId = billPay.BillPayId;

        // Act
        var result = await controller.Cancel(billPayId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);

        // Verify bill was removed
        var deletedBill = await _context.BillPays.FindAsync(billPayId);
        Assert.Null(deletedBill);
    }

    // test the Cancel action with bill pay id that does not belong to the customer does not remove the bill pay
    [Fact]
    public async Task Cancel_BillPayFromDifferentCustomer_DoesNotRemoveBill()
    {
        // Arrange
        var customerId = 2100;
        var otherCustomerId = 2200;
        var controller = CreateController(customerId);

        // Create bill for different customer
        var otherAccount = _context.Accounts.First(a => a.CustomerId == otherCustomerId);
        var payee = _context.Payees.First();
        var billPay = new BillPay
        {
            AccountNumber = otherAccount.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 100m,
            ScheduleTimeUtc = DateTime.UtcNow.AddDays(7),
            Period = PeriodType.Monthly,
            Status = StatusType.Pending
        };
        _context.BillPays.Add(billPay);
        await _context.SaveChangesAsync();

        var billPayId = billPay.BillPayId;

        // Act
        var result = await controller.Cancel(billPayId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);

        // Verify bill was NOT removed (different customer)
        var existingBill = await _context.BillPays.FindAsync(billPayId);
        Assert.NotNull(existingBill);
    }

    // test the Retry action with valid bill pay id updates status to Pending and redirects to Index
    [Fact]
    public async Task Retry_ValidBillPayId_UpdatesStatusToPendingAndRedirects()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var account = _context.Accounts.First(a => a.CustomerId == customerId);
        var payee = _context.Payees.First();
        var billPay = new BillPay
        {
            AccountNumber = account.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 100m,
            ScheduleTimeUtc = DateTime.UtcNow.AddDays(7),
            Period = PeriodType.Monthly,
            Status = StatusType.Failed // Failed status
        };
        _context.BillPays.Add(billPay);
        await _context.SaveChangesAsync();

        var billPayId = billPay.BillPayId;

        // Act
        var result = await controller.Retry(billPayId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);

        // Verify status was updated to Pending
        var updatedBill = await _context.BillPays.FindAsync(billPayId);
        Assert.NotNull(updatedBill);
        Assert.Equal(StatusType.Pending, updatedBill.Status);
    }

    // test the Retry action with bill pay id that does not belong to the customer does not update the status
    [Fact]
    public async Task Retry_BillPayFromDifferentCustomer_DoesNotUpdateStatus()
    {
        // Arrange
        var customerId = 2100;
        var otherCustomerId = 2200;
        var controller = CreateController(customerId);

        // Create bill for different customer
        var otherAccount = _context.Accounts.First(a => a.CustomerId == otherCustomerId);
        var payee = _context.Payees.First();
        var billPay = new BillPay
        {
            AccountNumber = otherAccount.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 100m,
            ScheduleTimeUtc = DateTime.UtcNow.AddDays(7),
            Period = PeriodType.Monthly,
            Status = StatusType.Failed
        };
        _context.BillPays.Add(billPay);
        await _context.SaveChangesAsync();

        var billPayId = billPay.BillPayId;

        // Act
        var result = await controller.Retry(billPayId);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectResult.ActionName);

        // Verify status was NOT updated (different customer)
        var existingBill = await _context.BillPays.FindAsync(billPayId);
        Assert.NotNull(existingBill);
        Assert.Equal(StatusType.Failed, existingBill.Status);
    }

    // test the Create POST action with zero amount fails model validation
    [Fact]
    public async Task Create_Post_ZeroAmount_ModelValidationShouldFail()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var account = _context.Accounts.First(a => a.CustomerId == customerId);
        var payee = _context.Payees.First();

        var model = new CreateBillPayViewModel
        {
            AccountNumber = account.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = 0m, // Zero amount
            ScheduleTimeUtc = DateTime.UtcNow.AddDays(7),
            Period = PeriodType.Monthly
        };

        // Manually validate the model
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(model);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
            model, validationContext, validationResults, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Amount"));
    }

    // test the Create POST action with negative amount fails model validation
    [Fact]
    public async Task Create_Post_NegativeAmount_ModelValidationShouldFail()
    {
        // Arrange
        var customerId = 2100;
        var controller = CreateController(customerId);

        var account = _context.Accounts.First(a => a.CustomerId == customerId);
        var payee = _context.Payees.First();

        var model = new CreateBillPayViewModel
        {
            AccountNumber = account.AccountNumber,
            PayeeId = payee.PayeeId,
            Amount = -50m, // Negative amount
            ScheduleTimeUtc = DateTime.UtcNow.AddDays(7),
            Period = PeriodType.Monthly
        };

        //  validate the model
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(model);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
            model, validationContext, validationResults, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, v => v.MemberNames.Contains("Amount"));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
