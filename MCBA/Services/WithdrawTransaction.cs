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
    private decimal _totalDeduction;
    

    private readonly DatabaseContext _context;

    public WithdrawTransaction(DatabaseContext context)
    {
        _context = context;
    }

    public string? FailureReason { get; private set; } // 


    public bool Validate()
    {
        

        var minBalance = TransactionRules.GetMinBalance(Account); // get min balance based on account type

        // Apply ATM fee if customer exceeded free withdrawal limit
        var fee = TransactionRules.GetAtmWithdrawFee(Account.AccountNumber, _context); // get the fee

        _totalDeduction = Amount + fee;
        
        // amount must be positive and less than balance
        if (Amount <= 0){
            FailureReason = "Withdrawal amount must be greater than zero.";
            return false;
        }

        if (Account.Balance - _totalDeduction < minBalance){
            FailureReason = "Insufficient funds.";
            return false;
        }
        return true;
    }

    public bool Execute()
    {
        if (!Validate()) return false;

        Amount = _totalDeduction;
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
