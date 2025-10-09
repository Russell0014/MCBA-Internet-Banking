using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AdminApi.Models;
using Xunit;

namespace AdminApi.Tests.Models;

public class BillPayTests
{
    private IEnumerable<ValidationResult> Validate(object model)
    {
        var ctx = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, ctx, results, true);
        return results;
    }

    [Fact]
    public void ValidBillPay_PassesValidation()
    {
        var b = new BillPay
        {
            BillPayId = 1,
            AccountNumber = 1234,
            PayeeId = 1,
            Amount = 10.50m,
            ScheduleTimeUtc = System.DateTime.UtcNow,
            Period = PeriodType.OneOff,
            Status = StatusType.Pending
        };

        var results = Validate(b);
        Assert.Empty(results);
    }

    [Fact]
    public void NegativeAmount_FailsValidation()
    {
        var b = new BillPay { Amount = -5m };
        var results = Validate(b).ToList();
        Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("positive"));
    }
}
