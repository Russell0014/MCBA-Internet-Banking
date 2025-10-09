using MCBA.Models;
using MCBA.Services;
using Xunit;

namespace MCBA.Tests.Models;

public class TransactionFactoryTests
{
    // test CreateTransaction method for each transaction type
    [Fact]
    public void CreateTransaction_WithWithdrawType_ReturnsWithdrawTransaction()
    {
        // Arrange
        var account = new Account { AccountNumber = 1234 };
        var amount = 100.00m;
        var comment = "Test withdrawal";

        // Act
        var transaction = TransactionFactory.CreateTransaction(TransactionType.Withdraw, account, amount, comment);

        // Assert
        Assert.IsType<WithdrawTransaction>(transaction);
        Assert.Equal(TransactionType.Withdraw, transaction.TransactionType);
        Assert.Equal(account, transaction.Account);
        Assert.Equal(amount, transaction.Amount);
        Assert.Equal(comment, transaction.Comment);
    }

    // test CreateTransaction method for each transaction type
    [Fact]
    public void CreateTransaction_WithDepositType_ReturnsDepositTransaction()
    {
        // Arrange
        var account = new Account { AccountNumber = 1234 };
        var amount = 200.00m;
        var comment = "Test deposit";

        // Act
        var transaction = TransactionFactory.CreateTransaction(TransactionType.Deposit, account, amount, comment);

        // Assert
        Assert.IsType<DepositTransaction>(transaction);
        Assert.Equal(TransactionType.Deposit, transaction.TransactionType);
        Assert.Equal(account, transaction.Account);
        Assert.Equal(amount, transaction.Amount);
        Assert.Equal(comment, transaction.Comment);
    }

    // test CreateTransaction method for each transaction type
    [Fact]
    public void CreateTransaction_WithTransferType_ReturnsTransferTransaction()
    {
        // Arrange
        var account = new Account { AccountNumber = 1234 };
        var destinationAccountNumber = 5678;
        var amount = 150.00m;
        var comment = "Test transfer";

        // Act
        var transaction = TransactionFactory.CreateTransaction(TransactionType.Transfer, account, amount, destinationAccountNumber, comment);

        // Assert
        Assert.IsType<TransferTransaction>(transaction);
        Assert.Equal(TransactionType.Transfer, transaction.TransactionType);
        Assert.Equal(account, transaction.Account);
        Assert.Equal(amount, transaction.Amount);
        Assert.Equal(comment, transaction.Comment);
        Assert.NotNull(transaction.DestinationAccount);
        Assert.Equal(destinationAccountNumber, transaction.DestinationAccount.AccountNumber);
    }

    // test CreateTransaction method for each transaction type
    [Fact]
    public void CreateTransaction_WithBillPayType_ReturnsBillPayTransaction()
    {
        // Arrange
        var account = new Account { AccountNumber = 1234 };
        var amount = 75.00m;
        var comment = "Test bill pay";

        // Act
        var transaction = TransactionFactory.CreateTransaction(TransactionType.BillPay, account, amount, comment);

        // Assert
        Assert.IsType<BillPayTransaction>(transaction);
        Assert.Equal(TransactionType.BillPay, transaction.TransactionType);
        Assert.Equal(account, transaction.Account);
        Assert.Equal(amount, transaction.Amount);
        Assert.Equal(comment, transaction.Comment);
    }

// test CreateTransaction method throws exception with no destination account for transfer type
    [Fact]
    public void CreateTransaction_WithTransferType_ThrowsException_WhenCalledWithoutDestination()
    {
        // Arrange
        var account = new Account { AccountNumber = 1234 };
        var amount = 100.00m;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            TransactionFactory.CreateTransaction(TransactionType.Transfer, account, amount));

        Assert.Contains("Transfer transactions require a destination account", exception.Message);
    }


// test CreateTransaction method throws exception for invalid type
    [Fact]
    public void CreateTransaction_WithInvalidType_ThrowsException()
    {
        // Arrange
        var account = new Account { AccountNumber = 1234 };
        var amount = 100.00m;
        var invalidType = (TransactionType)999;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            TransactionFactory.CreateTransaction(invalidType, account, amount));

        Assert.Contains("Invalid transaction type", exception.Message);
    }

// test CreateTransaction method with overload that includes destination account number

    [Fact]
    public void CreateTransaction_Overload_WithDestinationAccountNumber_WorksForAllTypes()
    {
        // Arrange
        var account = new Account { AccountNumber = 1234 };
        var amount = 100.00m;
        var destinationAccountNumber = 5678;
        var comment = "Test";

        // Test Withdraw
        var withdrawTransaction = TransactionFactory.CreateTransaction(TransactionType.Withdraw, account, amount, destinationAccountNumber, comment);
        Assert.IsType<WithdrawTransaction>(withdrawTransaction);

        // Test Deposit
        var depositTransaction = TransactionFactory.CreateTransaction(TransactionType.Deposit, account, amount, destinationAccountNumber, comment);
        Assert.IsType<DepositTransaction>(depositTransaction);

        // Test Transfer
        var transferTransaction = TransactionFactory.CreateTransaction(TransactionType.Transfer, account, amount, destinationAccountNumber, comment);
        Assert.IsType<TransferTransaction>(transferTransaction);

        // Test BillPay
        var billPayTransaction = TransactionFactory.CreateTransaction(TransactionType.BillPay, account, amount, destinationAccountNumber, comment);
        Assert.IsType<BillPayTransaction>(billPayTransaction);
    }

    // test CreateTransaction method with null comment
    [Fact]
    public void CreateTransaction_CommentCanBeNull()
    {
        // Arrange
        var account = new Account { AccountNumber = 1234 };
        var amount = 100.00m;

        // Act
        var transaction = TransactionFactory.CreateTransaction(TransactionType.Deposit, account, amount);

        // Assert
        Assert.Null(transaction.Comment);
    }
}
