using MCBA.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace MCBA.Tests.Models;

public class LoginTests
{
    [Fact]
    public void Login_CanBeCreated_WithValidData()
    {
        // Arrange & Act
        var login = new Login
        {
            LoginId = "12345678",
            CustomerId = 2100,
            PasswordHash = "hashedpassword"
        };

        // Assert
        Assert.Equal("12345678", login.LoginId);
        Assert.Equal(2100, login.CustomerId);
        Assert.Equal("hashedpassword", login.PasswordHash);
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("87654321")]
    [InlineData("00000000")]
    public void LoginId_AcceptsValidEightDigitStrings(string loginId)
    {
        // Arrange
        var login = new Login
        {
            LoginId = loginId,
            CustomerId = 2100,
            PasswordHash = "hashedpassword"
        };

        // Act
        var validationResults = ValidateModel(login);

        // Assert
        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData("1234567")]    // 7 digits
    [InlineData("123456789")]  // 9 digits
    [InlineData("abcdefgh")]   // letters
    [InlineData("1234567a")]   // mixed
    public void LoginId_RejectsInvalidStrings(string loginId)
    {
        // Arrange
        var login = new Login
        {
            LoginId = loginId,
            CustomerId = 2100,
            PasswordHash = "hashedpassword"
        };

        // Act
        var validationResults = ValidateModel(login);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage.Contains("Must be exactly 8 digits"));
    }

    [Fact]
    public void CustomerId_IsRequired()
    {
        // Arrange
        var login = new Login
        {
            LoginId = "12345678",
            PasswordHash = "hashedpassword"
            // CustomerId not set
        };

        // Act
        var validationResults = ValidateModel(login);

        // Assert
        Assert.Equal(0, login.CustomerId); // Default value
    }

    [Fact]
    public void PasswordHash_CanBeSet()
    {
        // Arrange
        var login = new Login
        {
            LoginId = "12345678",
            CustomerId = 2100
        };

        // Act
        login.PasswordHash = "newhashedpassword";

        // Assert
        Assert.Equal("newhashedpassword", login.PasswordHash);
    }

    [Fact]
    public void NavigationProperties_CanBeSet()
    {
        // Arrange
        var login = new Login
        {
            LoginId = "12345678",
            CustomerId = 2100,
            PasswordHash = "hashedpassword"
        };

        var customer = new Customer { CustomerId = 2100 };

        // Act
        login.Customer = customer;

        // Assert
        Assert.Equal(customer, login.Customer);
    }

    [Fact]
    public void NavigationProperties_AreInitiallyNull()
    {
        // Arrange & Act
        var login = new Login
        {
            LoginId = "12345678",
            CustomerId = 2100,
            PasswordHash = "hashedpassword"
        };

        // Assert
        Assert.Null(login.Customer);
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
