using MCBA.Data;
using MCBA.Models;

namespace MCBA.Services;

public class TransactionService
{
    private readonly DatabaseContext _context;

    public TransactionService(DatabaseContext context)
    {
        _context = context;
    }

    public bool Execute(ITransaction transaction)
    {
        // Calculate fee if it's a withdrawal
        if (transaction is WithdrawTransaction withdrawTransaction)
        {
            withdrawTransaction.CalculateFee(_context);
        }
        else if (transaction is TransferTransaction transferTransaction)
        {
            transferTransaction.CalculateFee(_context);
        }

        if (!transaction.Validate()) return false;

        // Execute the transaction 
        transaction.ExecuteTransaction();

        // persist transaction
        _context.Transactions.Add(new Transaction
        {
            Account = transaction.Account,
            DestinationAccountNumber = transaction.DestinationAccount?.AccountNumber,
            Amount = transaction.Amount,
            Comment = transaction.Comment,
            TransactionType = transaction.TransactionType,
            TransactionTimeUtc = DateTime.UtcNow
        });

        if (transaction is TransferTransaction transfer)
        {
            // persist destination transaction
            _context.Transactions.Add(new Transaction
            {
                Account = transfer.DestinationAccount,
                Amount = transfer.Amount,
                Comment = transfer.Comment,
                TransactionType = transaction.TransactionType,
                TransactionTimeUtc = DateTime.UtcNow
            });
        }

        // If there's a fee, add a separate ServiceCharge transaction
        if (transaction.Fee > 0)
        {
            _context.Transactions.Add(new Transaction
            {
                Account = transaction.Account,
                Amount = transaction.Fee,
                Comment = transaction.TransactionType == TransactionType.Withdraw ? "Withdraw Fee" : "Transfer Fee",
                TransactionType = TransactionType.ServiceCharge,
                TransactionTimeUtc = DateTime.UtcNow
            });
        }

        _context.SaveChanges();
        return true;
    }
}