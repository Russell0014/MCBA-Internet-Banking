using MCBA.Services;
using MCBA.Data;

namespace MCBA.Models;

// Factory class for creating transaction objects based on type
public static class TransactionFactory
{
    public static ITransaction CreateTransaction(TransactionType type, Account account, decimal amount,
        string? comment = null)
    {
        // // Transfer transactions require a destination account number
        if (type == TransactionType.Transfer)
        {
            throw new ArgumentException("Transfer transactions require a destination account.");
        }

        // Calls overloaded method with destinationAccountNumber = 0
        return CreateTransaction(type, account, amount, 0, comment);
    }

    public static ITransaction CreateTransaction(TransactionType type, Account account, decimal amount,
        int destinationAccountNumber, string? comment = null)
    {
        // Uses switch expression to instantiate the correct transaction type
        return type switch
        {
            TransactionType.Withdraw => new WithdrawTransaction
            {
                Account = account,
                Amount = amount,
                Comment = comment
            },
            TransactionType.Deposit => new DepositTransaction
            {
                Account = account,
                Amount = amount,
                Comment = comment
            },
            TransactionType.Transfer => new TransferTransaction
            {
                Account = account,
                Amount = amount,
                Comment = comment,
                // For transfer, set the destination account
                DestinationAccount = new Account { AccountNumber = destinationAccountNumber }
            },
            TransactionType.BillPay => new BillPayTransaction
            {
                Account = account,
                Amount = amount,
                Comment = comment
            },
            _ => throw new ArgumentException($"Invalid transaction type: {type}")
        };
    }
}