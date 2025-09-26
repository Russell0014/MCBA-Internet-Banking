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
            var feeApplies = TransactionRules.ShouldApplyWithdrawFee(withdrawTransaction.Account.AccountNumber, _context);
            withdrawTransaction.SetFee(feeApplies ? TransactionRules.AtmWithdrawFee : 0m); // set fee
        }

        if (!transaction.Validate()) return false;

        // Deduct total from account
        decimal deduction = transaction.Amount;

        if (transaction is WithdrawTransaction withdrawTx) // if withdrawal, include fee
        {
            deduction = withdrawTx.GetTotalDeduction(); // amount + fee
        }

        transaction.Account.Balance -= deduction; // deduct from account balance

        // persist transaction
        _context.Transactions.Add(new Transaction
        {
            Account = transaction.Account,
            Amount = transaction.Amount,
            Comment = transaction.Comment,
            TransactionType = transaction.TransactionType,
            TransactionTimeUtc = DateTime.UtcNow
        });

        _context.SaveChanges();
        return true;
    }
}
