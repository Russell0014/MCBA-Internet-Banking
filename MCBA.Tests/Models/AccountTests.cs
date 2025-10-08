using MCBA.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace MCBA.Tests.Models;
// Tests for Account model
public class AccountTests
{
    // test an account can be created with valid data
    [Fact]
    public void Account_CanBeCreated_WithValidData()
    {
        // Arrange & Act
        var account = new Account
        {
            AccountNumber = 1234,
            AccountType = AccountType.Checking,
            CustomerId = 2100,
            Balance = 1000.00m
        };

        // Assert
        Assert.Equal(1234, account.AccountNumber);
        Assert.Equal(AccountType.Checking, account.AccountType);
        Assert.Equal(2100, account.CustomerId);
        Assert.Equal(1000.00m, account.Balance);
    }

// test input account number when its valid
    [Theory]
    [InlineData(1234)]
    [InlineData(9999)]
    public void AccountNumber_AcceptsValidFourDigitNumbers(int accountNumber)
    {
        // Arrange
        var account = new Account
        {
            AccountNumber = accountNumber,
            AccountType = AccountType.Checking,
            CustomerId = 2100,
            Balance = 0.00m
        };

        // Act
        var validationResults = ValidateModel(account);

        // Assert
        Assert.Empty(validationResults);
    }

// test input account number when its invalid (not 4 digits with edge cases)
    [Theory]
    [InlineData(123)]     // 3 digits
    [InlineData(12345)]   // 5 digits
    public void AccountNumber_RejectsInvalidNumbers(int accountNumber)
    {
        // Arrange
        var account = new Account
        {
            AccountNumber = accountNumber,
            AccountType = AccountType.Checking,
            CustomerId = 2100,
            Balance = 0.00m
        };

        // Act
        var validationResults = ValidateModel(account);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage.Contains("Must be exactly 4 digits"));
    }

// test account type when its valid
    [Theory]
    [InlineData(AccountType.Checking)]
    [InlineData(AccountType.Savings)]
    public void AccountType_AcceptsValidEnumValues(AccountType accountType)
    {
        // Arrange
        var account = new Account
        {
            AccountNumber = 1234,
            AccountType = accountType,
            CustomerId = 2100,
            Balance = 0.00m
        };

        // Act
        var validationResults = ValidateModel(account);

        // Assert
        Assert.Empty(validationResults);
    }

// test that balance accepts valid decimal values
    [Fact]
    public void Balance_AcceptsValidDecimalValues()
    {
        // Arrange
        var account = new Account
        {
            AccountNumber = 1234,
            AccountType = AccountType.Checking,
            CustomerId = 2100,
            Balance = 100.50m
        };

        // Act
        var validationResults = ValidateModel(account);

        // Assert
        Assert.Empty(validationResults);
        Assert.Equal(100.50m, account.Balance);
    }

// test that balance rejects negative decimal values
    [Fact]
    public void NavigationProperties_CanBeSet()
    {
        // Arrange
        var account = new Account
        {
            AccountNumber = 1234,
            AccountType = AccountType.Checking,
            CustomerId = 2100,
            Balance = 0.00m
        };

        var customer = new Customer { CustomerId = 2100 };
        var transactions = new List<Transaction>
        {
            new Transaction { TransactionId = 1 },
            new Transaction { TransactionId = 2 }
        };

        // Act
        account.Customer = customer;
        account.Transactions = transactions;

        // Assert
        Assert.Equal(customer, account.Customer);
        Assert.Equal(transactions, account.Transactions);
        Assert.Equal(2, account.Transactions.Count);
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
