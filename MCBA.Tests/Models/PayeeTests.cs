using MCBA.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace MCBA.Tests.Models;

public class PayeeTests
{
    // test a payee can be created with valid data
    [Fact]
    public void Payee_CanBeCreated_WithValidData()
    {
        // Arrange & Act
        var payee = new Payee
        {
            PayeeId = 1,
            Name = "Test Payee",
            Address = "123 Test Street",
            City = "Melbourne",
            State = "VIC",
            Postcode = "3000",
            Phone = "(04) 1234 5678"
        };

        // Assert
        Assert.Equal(1, payee.PayeeId);
        Assert.Equal("Test Payee", payee.Name);
        Assert.Equal("123 Test Street", payee.Address);
        Assert.Equal("Melbourne", payee.City);
        Assert.Equal("VIC", payee.State);
        Assert.Equal("3000", payee.Postcode);
        Assert.Equal("(04) 1234 5678", payee.Phone);
    }
    [Theory]
    [InlineData("(04) 1234 5678")]
    public void Phone_AcceptsValidAustralianFormat(string phone)
    {
        // Arrange
        var payee = new Payee
        {
            PayeeId = 1,
            Name = "Test Payee",
            Phone = phone
        };

        // Act
        var validationResults = ValidateModel(payee);

        // Assert
        Assert.Empty(validationResults);
    }

    // test input phone when its invalid (not in valid Australian format with edge cases)
    [Theory]
    [InlineData("0412345678")]
    [InlineData("(04)12345678")]
    public void Phone_RejectsInvalidFormats(string phone)
    {
        // Arrange
        var payee = new Payee
        {
            PayeeId = 1,
            Name = "Test Payee",
            Phone = phone
        };

        // Act
        var validationResults = ValidateModel(payee);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage.Contains("format: (0X) XXXX XXXX"));
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
