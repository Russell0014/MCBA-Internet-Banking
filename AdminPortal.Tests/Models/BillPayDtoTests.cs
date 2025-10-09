using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AdminPortal.Models;
using Xunit;

namespace AdminPortal.Tests.Models;

public class BillPayDtoTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var results = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, results, true);
        return results;
    }

    [Fact]
    public void ValidBillPay_ShouldPassValidation()
    {
        var b = new BillPayDto
        {
            BillPayId = 1,
            AccountNumber = 123456,
            PayeeId = 10,
            Amount = 25.50m,
            ScheduleTimeUtc = System.DateTime.UtcNow.AddDays(1),
            Period = BillPayDto.PeriodType.OneOff,
            Status = BillPayDto.StatusType.Pending
        };

        var results = ValidateModel(b);
        Assert.Empty(results);
    }

    [Fact]
    public void AmountZeroOrNegative_ShouldFailValidation()
    {
        var b = new BillPayDto
        {
            BillPayId = 2,
            AccountNumber = 1,
            PayeeId = 1,
            Amount = 0m,
            ScheduleTimeUtc = System.DateTime.UtcNow,
            Period = BillPayDto.PeriodType.Monthly,
            Status = BillPayDto.StatusType.Pending
        };

        var results = ValidateModel(b);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(BillPayDto.Amount)));
    }

    [Theory]
    [InlineData(BillPayDto.PeriodType.OneOff)]
    [InlineData(BillPayDto.PeriodType.Monthly)]
    public void PeriodEnum_HasExpectedValues(BillPayDto.PeriodType period)
    {
        // Ensure enum serialisation/values remain usable
        var b = new BillPayDto { Period = period };
        Assert.True(System.Enum.IsDefined(typeof(BillPayDto.PeriodType), b.Period));
    }

    [Theory]
    [InlineData(BillPayDto.StatusType.Completed)]
    [InlineData(BillPayDto.StatusType.Pending)]
    [InlineData(BillPayDto.StatusType.Failed)]
    [InlineData(BillPayDto.StatusType.Blocked)]
    public void StatusEnum_HasExpectedValues(BillPayDto.StatusType status)
    {
        var b = new BillPayDto { Status = status };
        Assert.True(System.Enum.IsDefined(typeof(BillPayDto.StatusType), b.Status));
    }
}
