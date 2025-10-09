using MCBA.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace MCBA.Tests.Models;

public class CustomerTests
{
    // test a customer can be created with valid data
    [Fact]
    public void Customer_CanBeCreated_WithValidData()
    {
        // Arrange & Act
        var customer = new Customer
        {
            CustomerId = 1234,
            Name = "John Doe",
            TFN = "123 456 789",
            Address = "123 Main Street",
            City = "Melbourne",
            State = "VIC",
            PostCode = "3000",
            Mobile = "0412 345 678"
        };

        // Assert
        Assert.Equal(1234, customer.CustomerId);
        Assert.Equal("John Doe", customer.Name);
        Assert.Equal("123 456 789", customer.TFN);
        Assert.Equal("123 Main Street", customer.Address);
        Assert.Equal("Melbourne", customer.City);
        Assert.Equal("VIC", customer.State);
        Assert.Equal("3000", customer.PostCode);
        Assert.Equal("0412 345 678", customer.Mobile);
    }

// test a customer can be created with minimum required data
    [Fact]
    public void Customer_CanBeCreated_WithMinimumRequiredData()
    {
        // Arrange & Act
        var customer = new Customer
        {
            CustomerId = 1234,
            Name = "John Doe"
        };

        // Assert
        Assert.Equal(1234, customer.CustomerId);
        Assert.Equal("John Doe", customer.Name);
        Assert.Null(customer.TFN);
        Assert.Null(customer.Address);
        Assert.Null(customer.City);
        Assert.Null(customer.State);
        Assert.Null(customer.PostCode);
        Assert.Null(customer.Mobile);
    }

    // Helper method to validate a model
    [Theory]
    [InlineData(1234)]
    [InlineData(9999)]
    public void CustomerId_AcceptsValidFourDigitNumbers(int customerId)
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = customerId,
            Name = "John Doe"
        };

        // Act
        var validationResults = ValidateModel(customer);

        // Assert
        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData(123)]     // 3 digits
    [InlineData(12345)]   // 5 digits
    [InlineData(0)]       // 0
    public void CustomerId_RejectsInvalidNumbers(int customerId)
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = customerId,
            Name = "John Doe"
        };

        // Act
        var validationResults = ValidateModel(customer);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage.Contains("Must be exactly 4 digits"));
    }

    [Theory]
    [InlineData("John Doe")]
    [InlineData(null)]  // Null is allowed since no Required attribute
    [InlineData("")]    // Empty string is allowed since no Required attribute
    public void Name_AcceptsValidStrings(string name)
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = 1234,
            Name = name
        };

        // Act
        var validationResults = ValidateModel(customer);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void Name_RejectsTooLongString()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = 1234,
            Name = "A very long name that exceeds the fifty character limit and should fail validation"
        };

        // Act
        var validationResults = ValidateModel(customer);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage.Contains("maximum length"));
    }

    [Theory]
    [InlineData("123 456 789")]
    [InlineData(null)]
    public void TFN_AcceptsValidFormats(string tfn)
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = 1234,
            Name = "John Doe",
            TFN = tfn
        };

        // Act
        var validationResults = ValidateModel(customer);

        // Assert
        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData("123456789")]     // No spaces
    [InlineData("123 456 78")]    // Wrong format
    [InlineData("abc def ghi")]   // Non-digits
    public void TFN_RejectsInvalidFormats(string tfn)
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = 1234,
            Name = "John Doe",
            TFN = tfn
        };

        // Act
        var validationResults = ValidateModel(customer);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage.Contains("Must be of the format"));
    }

    [Theory]
    [InlineData("NSW")]
    [InlineData("VIC")]
    [InlineData("QLD")]
    [InlineData("nsw")]  // lowercase
    [InlineData(null)]
    public void State_AcceptsValidAustralianStates(string state)
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = 1234,
            Name = "John Doe",
            State = state
        };

        // Act
        var validationResults = ValidateModel(customer);

        // Assert
        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData("XX")]    // Invalid state
    [InlineData("NY")]    // US state
    [InlineData("123")]   // Numbers
    public void State_RejectsInvalidValues(string state)
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = 1234,
            Name = "John Doe",
            State = state
        };

        // Act
        var validationResults = ValidateModel(customer);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage.Contains("valid 2 or 3 letter Australian state code"));
    }

    [Theory]
    [InlineData("3000")]
    [InlineData(null)]
    public void PostCode_AcceptsValidFourDigitNumbers(string postCode)
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = 1234,
            Name = "John Doe",
            PostCode = postCode
        };

        // Act
        var validationResults = ValidateModel(customer);

        // Assert
        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData("123")]     // 3 digits
    [InlineData("12345")]   // 5 digits
    [InlineData("abcd")]    // Non-digits
    public void PostCode_RejectsInvalidFormats(string postCode)
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = 1234,
            Name = "John Doe",
            PostCode = postCode
        };

        // Act
        var validationResults = ValidateModel(customer);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage.Contains("Must be exactly 4 digits"));
    }

    [Theory]
    [InlineData("0412 345 678")]
    [InlineData(null)]
    public void Mobile_AcceptsValidFormats(string mobile)
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = 1234,
            Name = "John Doe",
            Mobile = mobile
        };

        // Act
        var validationResults = ValidateModel(customer);

        // Assert
        Assert.Empty(validationResults);
    }

    [Theory]
    [InlineData("0412345678")]    // No spaces
    [InlineData("0512 345 678")]  // Wrong prefix
    [InlineData("abc def ghi")]   // Non-digits
    public void Mobile_RejectsInvalidFormats(string mobile)
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = 1234,
            Name = "John Doe",
            Mobile = mobile
        };

        // Act
        var validationResults = ValidateModel(customer);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage.Contains("Must be of the format"));
    }

    [Fact]
    public void Accounts_CanBeNullOrEmpty()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = 1234,
            Name = "John Doe",
            Accounts = null
        };

        // Assert
        Assert.Null(customer.Accounts);

        // Test with empty list
        customer.Accounts = new List<Account>();
        Assert.NotNull(customer.Accounts);
        Assert.Empty(customer.Accounts);
    }

    [Fact]
    public void Accounts_CanContainAccounts()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { AccountNumber = 1234, AccountType = AccountType.Checking, CustomerId = 1234, Balance = 1000.00m },
            new Account { AccountNumber = 5678, AccountType = AccountType.Savings, CustomerId = 1234, Balance = 500.00m }
        };

        var customer = new Customer
        {
            CustomerId = 1234,
            Name = "John Doe",
            Accounts = accounts
        };

        // Assert
        Assert.NotNull(customer.Accounts);
        Assert.Equal(2, customer.Accounts.Count);
        Assert.Equal(1234, customer.Accounts[0].AccountNumber);
        Assert.Equal(5678, customer.Accounts[1].AccountNumber);
    }

    [Fact]
    public void Customer_ValidatesMultipleInvalidFields()
    {
        // Arrange - Multiple invalid fields
        var customer = new Customer
        {
            CustomerId = 123,  // Invalid: 3 digits
            Name = "A very long name that exceeds the fifty character limit and should fail validation",  // Invalid: too long
            TFN = "123456789",  // Invalid: wrong format
            State = "NY",       // Invalid: not Australian state
            PostCode = "123",   // Invalid: 3 digits
            Mobile = "0412345678"  // Invalid: wrong format
        };

        // Act
        var validationResults = ValidateModel(customer);

        // Assert
        Assert.NotEmpty(validationResults);
        // Should have multiple validation errors
        Assert.True(validationResults.Count >= 5);
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
