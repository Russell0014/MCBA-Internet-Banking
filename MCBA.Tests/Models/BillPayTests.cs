using MCBA.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace MCBA.Tests.Models;
// Tests for BillPay model
public class BillPayTests
{
    // test a bill pay can be created with valid data
    [Fact]
    public void BillPay_CanBeCreated_WithValidData()
    {
        // Arrange & Act
        var billPay = new BillPay
        {
            BillPayId = 1,
            AccountNumber = 1234,
            PayeeId = 1,
            Amount = 100.00m,
            ScheduleTimeUtc = DateTime.UtcNow,
            Period = PeriodType.OneOff,
            Status = StatusType.Pending
        };

        // Assert
        Assert.Equal(1, billPay.BillPayId);
        Assert.Equal(1234, billPay.AccountNumber);
        Assert.Equal(1, billPay.PayeeId);
        Assert.Equal(100.00m, billPay.Amount);
        Assert.Equal(PeriodType.OneOff, billPay.Period);
        Assert.Equal(StatusType.Pending, billPay.Status);
    }

// test ammount accepts valid positive values
    [Theory]
    [InlineData(0.01)]
    [InlineData(1.00)]
    [InlineData(1000.00)]
    public void Amount_AcceptsValidPositiveValues(decimal amount)
    {
        // Arrange
        var billPay = new BillPay
        {
            BillPayId = 1,
            AccountNumber = 1234,
            PayeeId = 1,
            Amount = amount,
            ScheduleTimeUtc = DateTime.UtcNow,
            Period = PeriodType.OneOff,
            Status = StatusType.Pending
        };

        // Act
        var validationResults = ValidateModel(billPay);

        // Assert
        Assert.Empty(validationResults);
    }

// test ammount accepts maximum decimal value
    [Fact]
    public void Amount_AcceptsMaximumDecimalValue()
    {
        // Arrange
        var billPay = new BillPay
        {
            BillPayId = 1,
            AccountNumber = 1234,
            PayeeId = 1,
            Amount = decimal.MaxValue,
            ScheduleTimeUtc = DateTime.UtcNow,
            Period = PeriodType.OneOff,
            Status = StatusType.Pending
        };

        // Act
        var validationResults = ValidateModel(billPay);

        // Assert
        Assert.Empty(validationResults);
    }

// test ammount rejects invalid zero or negative values
    [Theory]
    [InlineData(0.00)]
    [InlineData(-1.00)]
    [InlineData(-100.00)]
    public void Amount_RejectsInvalidValues(decimal amount)
    {
        // Arrange
        var billPay = new BillPay
        {
            BillPayId = 1,
            AccountNumber = 1234,
            PayeeId = 1,
            Amount = amount,
            ScheduleTimeUtc = DateTime.UtcNow,
            Period = PeriodType.OneOff,
            Status = StatusType.Pending
        };

        // Act
        var validationResults = ValidateModel(billPay);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage.Contains("Must be a positive value"));
    }

// test period accepts valid enum values
    [Theory]
    [InlineData(PeriodType.OneOff)]
    [InlineData(PeriodType.Monthly)]
    public void Period_AcceptsValidEnumValues(PeriodType period)
    {
        // Arrange
        var billPay = new BillPay
        {
            BillPayId = 1,
            AccountNumber = 1234,
            PayeeId = 1,
            Amount = 100.00m,
            ScheduleTimeUtc = DateTime.UtcNow,
            Period = period,
            Status = StatusType.Pending
        };

        // Act
        var validationResults = ValidateModel(billPay);

        // Assert
        Assert.Empty(validationResults);
    }

// test status accepts valid enum values
    [Theory]
    [InlineData(StatusType.Completed)]
    [InlineData(StatusType.Pending)]
    [InlineData(StatusType.Failed)]
    [InlineData(StatusType.Blocked)]
    public void Status_AcceptsValidEnumValues(StatusType status)
    {
        // Arrange
        var billPay = new BillPay
        {
            BillPayId = 1,
            AccountNumber = 1234,
            PayeeId = 1,
            Amount = 100.00m,
            ScheduleTimeUtc = DateTime.UtcNow,
            Period = PeriodType.OneOff,
            Status = status
        };

        // Act
        var validationResults = ValidateModel(billPay);

        // Assert
        Assert.Empty(validationResults);
    }

// test period type accepts only defined enum values
    [Fact]
    public void PeriodType_HasCorrectEnumValues()
    {
        // Assert
        Assert.Equal(1, (int)PeriodType.OneOff);
        Assert.Equal(2, (int)PeriodType.Monthly);
    }

// test status type accepts only defined enum values
    [Fact]
    public void StatusType_HasCorrectEnumValues()
    {
        // Assert
        Assert.Equal(1, (int)StatusType.Completed);
        Assert.Equal(2, (int)StatusType.Pending);
        Assert.Equal(3, (int)StatusType.Failed);
        Assert.Equal(4, (int)StatusType.Blocked);
    }

// test period type has correct display names
    [Fact]
    public void PeriodType_HasCorrectDisplayNames()
    {
        // Assert
        var oneOffDisplay = typeof(PeriodType).GetField(PeriodType.OneOff.ToString())!
            .GetCustomAttributes(typeof(DisplayAttribute), false)
            .Cast<DisplayAttribute>()
            .FirstOrDefault()?.Name;
        Assert.Equal("One Off", oneOffDisplay);

        // Monthly has no display attribute, so it should be null or default
        var monthlyDisplay = typeof(PeriodType).GetField(PeriodType.Monthly.ToString())!
            .GetCustomAttributes(typeof(DisplayAttribute), false)
            .Cast<DisplayAttribute>()
            .FirstOrDefault()?.Name;
        Assert.Null(monthlyDisplay);
    }

// test navigation properties can be set and retrieved
    [Fact]
    public void NavigationProperties_CanBeSet()
    {
        // Arrange
        var billPay = new BillPay
        {
            BillPayId = 1,
            AccountNumber = 1234,
            PayeeId = 1,
            Amount = 100.00m,
            ScheduleTimeUtc = DateTime.UtcNow,
            Period = PeriodType.OneOff,
            Status = StatusType.Pending
        };

        var account = new Account { AccountNumber = 1234 };
        var payee = new Payee { PayeeId = 1 };

        // Act
        billPay.Account = account;
        billPay.Payee = payee;

        // Assert
        Assert.Equal(account, billPay.Account);
        Assert.Equal(payee, billPay.Payee);
    }

    [Fact]
    public void NavigationProperties_AreInitiallyNull()
    {
        // Arrange & Act
        var billPay = new BillPay
        {
            BillPayId = 1,
            AccountNumber = 1234,
            PayeeId = 1,
            Amount = 100.00m,
            ScheduleTimeUtc = DateTime.UtcNow,
            Period = PeriodType.OneOff,
            Status = StatusType.Pending
        };

        // Assert
        Assert.Null(billPay.Account);
        Assert.Null(billPay.Payee);
    }

// test ScheduleTimeUtc can be set to past and future dates
    [Fact]
    public void ScheduleTimeUtc_CanBeSetToPastAndFuture()
    {
        // Arrange
        var pastTime = DateTime.UtcNow.AddDays(-1);
        var futureTime = DateTime.UtcNow.AddDays(1);

        // Act & Assert - Past time
        var billPayPast = new BillPay
        {
            BillPayId = 1,
            AccountNumber = 1234,
            PayeeId = 1,
            Amount = 100.00m,
            ScheduleTimeUtc = pastTime,
            Period = PeriodType.OneOff,
            Status = StatusType.Pending
        };
        Assert.Equal(pastTime, billPayPast.ScheduleTimeUtc);

        // Act & Assert - Future time
        var billPayFuture = new BillPay
        {
            BillPayId = 2,
            AccountNumber = 1234,
            PayeeId = 1,
            Amount = 100.00m,
            ScheduleTimeUtc = futureTime,
            Period = PeriodType.OneOff,
            Status = StatusType.Pending
        };
        Assert.Equal(futureTime, billPayFuture.ScheduleTimeUtc);
    }

    private static List<ValidationResult> ValidateModel(object model)
    {
        ArgumentNullException.ThrowIfNull(model);
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model!);
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        return validationResults;
    }
}
