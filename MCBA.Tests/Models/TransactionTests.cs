using MCBA.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace MCBA.Tests.Models;

public class TransactionTests
{
    // test a transaction can be created with valid data
    [Fact]
    public void Transaction_CanBeCreated_WithValidData()
    {
        // Arrange
        var account = new Account { AccountNumber = 1234 };

        // Act
        var transaction = new Transaction
        {
            TransactionId = 1,
            TransactionType = TransactionType.Deposit,
            AccountNumber = 1234,
            Account = account,
            Amount = 100.00m,
            Comment = "Test deposit",
            TransactionTimeUtc = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(1, transaction.TransactionId);
        Assert.Equal(TransactionType.Deposit, transaction.TransactionType);
        Assert.Equal(1234, transaction.AccountNumber);
        Assert.Equal(account, transaction.Account);
        Assert.Equal(100.00m, transaction.Amount);
        Assert.Equal("Test deposit", transaction.Comment);
        Assert.NotEqual(default(DateTime), transaction.TransactionTimeUtc);
    }

    [Theory]
    [InlineData(TransactionType.Deposit)]
    [InlineData(TransactionType.Withdraw)]
    [InlineData(TransactionType.Transfer)]
    [InlineData(TransactionType.ServiceCharge)]
    [InlineData(TransactionType.BillPay)]
    public void TransactionType_AcceptsValidEnumValues(TransactionType transactionType)
    {
        // Arrange
        var transaction = new Transaction
        {
            TransactionId = 1,
            TransactionType = transactionType,
            AccountNumber = 1234,
            Amount = 100.00m,
            TransactionTimeUtc = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateModel(transaction);

        // Assert
        Assert.Empty(validationResults);
    }

// test transaction type has correct enum values
    [Fact]
    public void TransactionType_HasCorrectEnumValues()
    {
        // Assert
        Assert.Equal(1, (int)TransactionType.Deposit);
        Assert.Equal(2, (int)TransactionType.Withdraw);
        Assert.Equal(3, (int)TransactionType.Transfer);
        Assert.Equal(4, (int)TransactionType.ServiceCharge);
        Assert.Equal(5, (int)TransactionType.BillPay);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1.00)]
    [InlineData(1000.00)]
    [InlineData(999999.99)]
    public void Amount_AcceptsValidPositiveValues(decimal amount)
    {
        // Arrange
        var transaction = new Transaction
        {
            TransactionId = 1,
            TransactionType = TransactionType.Deposit,
            AccountNumber = 1234,
            Amount = amount,
            TransactionTimeUtc = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateModel(transaction);

        // Assert
        Assert.Empty(validationResults);
        Assert.Equal(amount, transaction.Amount);
    }

    [Theory]
    [InlineData(0.00)]
    [InlineData(-1.00)]
    [InlineData(-100.00)]
    public void Amount_RejectsInvalidValues(decimal amount)
    {
        // Arrange
        var transaction = new Transaction
        {
            TransactionId = 1,
            TransactionType = TransactionType.Deposit,
            AccountNumber = 1234,
            Amount = amount,
            TransactionTimeUtc = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateModel(transaction);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.ErrorMessage.Contains("positive value"));
    }

    [Theory]
    [InlineData("Valid comment")]
    [InlineData("Another comment")]
    [InlineData(null)]
    [InlineData("")]
    public void Comment_AcceptsValidStrings(string? comment)
    {
        // Arrange
        var transaction = new Transaction
        {
            TransactionId = 1,
            TransactionType = TransactionType.Deposit,
            AccountNumber = 1234,
            Amount = 100.00m,
            Comment = comment,
            TransactionTimeUtc = DateTime.UtcNow
        };

        // Act
        var validationResults = ValidateModel(transaction);

        // Assert
        Assert.Empty(validationResults);
        Assert.Equal(comment, transaction.Comment);
    }

// test a comment can be null
    [Fact]
    public void Comment_CanBeNull()
    {
        // Arrange
        var transaction = new Transaction
        {
            TransactionId = 1,
            TransactionType = TransactionType.Deposit,
            AccountNumber = 1234,
            Amount = 100.00m,
            TransactionTimeUtc = DateTime.UtcNow
            // Comment not set
        };

        // Assert
        Assert.Null(transaction.Comment);
    }

    [Fact]
    public void NavigationProperties_CanBeSet()
    {
        // Arrange
        var transaction = new Transaction
        {
            TransactionId = 1,
            TransactionType = TransactionType.Transfer,
            AccountNumber = 1234,
            Amount = 100.00m,
            TransactionTimeUtc = DateTime.UtcNow
        };

        var account = new Account { AccountNumber = 1234 };
        var destinationAccount = new Account { AccountNumber = 5678 };

        // Act
        transaction.Account = account;
        transaction.DestinationAccountNumber = 5678;
        transaction.DestinationAccount = destinationAccount;

        // Assert
        Assert.Equal(account, transaction.Account);
        Assert.Equal(5678, transaction.DestinationAccountNumber);
        Assert.Equal(destinationAccount, transaction.DestinationAccount);
    }

    [Fact]
    public void NavigationProperties_AreInitiallyNull()
    {
        // Arrange & Act
        var transaction = new Transaction
        {
            TransactionId = 1,
            TransactionType = TransactionType.Deposit,
            AccountNumber = 1234,
            Amount = 100.00m,
            TransactionTimeUtc = DateTime.UtcNow
        };

        // Assert
        Assert.Null(transaction.Account);
        Assert.Null(transaction.DestinationAccount);
        Assert.Null(transaction.DestinationAccountNumber);
    }

// test destination account number can be null when its a deposit
    [Fact]
    public void DestinationAccountNumber_CanBeNull()
    {
        // Arrange
        var transaction = new Transaction
        {
            TransactionId = 1,
            TransactionType = TransactionType.Deposit,
            AccountNumber = 1234,
            Amount = 100.00m,
            TransactionTimeUtc = DateTime.UtcNow
            // DestinationAccountNumber not set
        };

        // Assert
        Assert.Null(transaction.DestinationAccountNumber);
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
