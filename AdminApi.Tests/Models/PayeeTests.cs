using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AdminApi.Models;
using Xunit;

namespace AdminApi.Tests.Models;

public class PayeeTests
{
    private IEnumerable<ValidationResult> Validate(object model)
    {
        var ctx = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, ctx, results, true);
        return results;
    }

    [Fact]
    public void ValidPayee_PassesValidation()
    {
        var p = new Payee
        {
            Name = "Bob",
            Address = "1 Test St",
            City = "Melbourne",
            State = "VIC",
            Postcode = "3000",
            Phone = "(03) 9123 4567"
        };

        var results = Validate(p);
        Assert.Empty(results);
    }

    [Fact]
    public void InvalidState_FailsValidation()
    {
        var p = new Payee { State = "XX" };
        var results = Validate(p).ToList();
        Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("state", System.StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void InvalidPostcode_FailsValidation()
    {
        var p = new Payee { Postcode = "ABC" };
        var results = Validate(p).ToList();
        Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("4 digits"));
    }

    [Fact]
    public void InvalidPhone_FailsValidation()
    {
        var p = new Payee { Phone = "12345" };
        var results = Validate(p).ToList();
        Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("format"));
    }
}
