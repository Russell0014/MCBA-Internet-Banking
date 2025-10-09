using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AdminPortal.Models;
using Xunit;

namespace AdminPortal.Tests.Models;

public class PayeeDtoTests
{
    private static IList<ValidationResult> ValidateModel(object model)
    {
        var results = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, results, true);
        return results;
    }

    [Fact]
    public void ValidPayee_ShouldPassValidation()
    {
        var p = new PayeeDto
        {
            PayeeId = 1,
            Name = "Bob",
            Address = "1 Short St",
            City = "Melb",
            State = "VIC",
            Postcode = "3000",
            Phone = "(03) 1234 5678"
        };

        var results = ValidateModel(p);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData("XYZ")]
    [InlineData("Victoria")]
    public void InvalidState_ShouldFailValidation(string state)
    {
        var p = new PayeeDto
        {
            PayeeId = 2,
            Name = "Alice",
            Address = "2 Some Rd",
            City = "Sydney",
            State = state,
            Postcode = "2000",
            Phone = "(02) 1234 5678"
        };

        var results = ValidateModel(p);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(PayeeDto.State)));
    }

    [Theory]
    [InlineData("300")]   // too short
    [InlineData("abcd")]  // non-digits
    [InlineData("12345")] // too long
    public void InvalidPostcode_ShouldFailValidation(string postcode)
    {
        var p = new PayeeDto
        {
            PayeeId = 3,
            Name = "Sam",
            Address = "3 Lane",
            City = "Hobart",
            State = "TAS",
            Postcode = postcode,
            Phone = "(03) 1234 5678"
        };

        var results = ValidateModel(p);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(PayeeDto.Postcode)));
    }

    [Theory]
    [InlineData("03 1234 5678")] // missing parentheses
    [InlineData("(03)12345678")] // missing spaces
    [InlineData("(03) 123 45678")] // wrong grouping
    public void InvalidPhone_ShouldFailValidation(string phone)
    {
        var p = new PayeeDto
        {
            PayeeId = 4,
            Name = "Kelly",
            Address = "4 Ave",
            City = "Adelaide",
            State = "SA",
            Postcode = "5000",
            Phone = phone
        };

        var results = ValidateModel(p);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(PayeeDto.Phone)));
    }

    [Fact]
    public void NameTooLong_ShouldFailStringLengthValidation()
    {
        var longName = new string('x', 51);
        var p = new PayeeDto
        {
            PayeeId = 5,
            Name = longName,
            Address = "Address",
            City = "City",
            State = "NSW",
            Postcode = "2000",
            Phone = "(02) 1234 5678"
        };

        var results = ValidateModel(p);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(PayeeDto.Name)));
    }
}
