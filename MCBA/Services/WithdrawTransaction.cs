using MCBA.Data;
using MCBA.Models;

namespace MCBA.Services;

public class WithdrawTransaction : ITransaction
{
    private const decimal AtmWithdrawFee = 0.01m;
    private const int MaxFreeTransfer = 2;

    public int TransactionID { get; set; }
    public TransactionType TransactionType { get; set; } = TransactionType.Withdraw;
    public required Account Account { get; set; }
    public Account? DestinationAccount { get; set; } = null;
    public decimal Amount { get; set; }
    public string? Comment { get; set; }
    public DateTime TransactionTimeUtc { get; set; } = DateTime.UtcNow;

    private readonly DatabaseContext _context;

    public WithdrawTransaction(DatabaseContext context)
    {
        _context = context;
    }


    public bool Validate()
    {
        // amount must be positive and less than balance
        if (Amount <= 0) return false;
        if (Account.Balance < Amount) return false;
        return true;
    }

    public bool Execute()
    {
        if (!Validate()) return false;

        // Apply ATM fee if necessary ]\
        var feeApplies = false;
        if (feeApplies)
        {
            Account.Balance -= AtmWithdrawFee;
        }

        // Withdraw the amount
        Account.Balance -= Amount;

        // Record transaction in DB
        _context.Transactions.Add(new Transaction
        {
            Account = Account,
            Amount = Amount,
            Comment = Comment,
            TransactionTimeUtc = TransactionTimeUtc,
            TransactionType = TransactionType
        });

        _context.SaveChanges();
        return true;
    }
}
