using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AdminApi.Models;
using Xunit;

namespace AdminApi.Tests.Models;

public class AccountTests
{
    private IEnumerable<ValidationResult> Validate(object model)
    {
        var ctx = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, ctx, results, true);
        return results;
    }

    [Fact]
    public void ValidAccount_PassesValidation()
    {
        var a = new Account { AccountNumber = 1234, AccountType = AccountType.Checking, Balance = 100m };
        var results = Validate(a);
        Assert.Empty(results);
    }

    [Fact]
    public void InvalidAccountNumber_FailsValidation()
    {
        var a = new Account { AccountNumber = 12 };
        var results = Validate(a).ToList();
        Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("4 digits"));
    }
}
