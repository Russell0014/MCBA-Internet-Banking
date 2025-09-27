using MCBA.Models;

namespace MCBA.Services;

public class DepositTransaction : ITransaction
{
    public int TransactionID { get; set; }
    public TransactionType TransactionType { get; set; } = TransactionType.Deposit;
    public Account Account { get; set; }
    public Account? DestinationAccount { get; set; } = null;
    public decimal Amount { get; set; }
    public string? Comment { get; set; }
    public DateTime TransactionTimeUtc { get; set; } = DateTime.UtcNow;
    public decimal Fee { get; private set; } = 0m;
    public string? FailureReason { get; private set; }
    public bool Validate()
    {

        if (Amount <= 0)
        {
            FailureReason = "Deposit amount must be greater than zero.";
            return false;
        }
        return true;
    }
    public void ExecuteTransaction()
    {
        Account.Balance += Amount;
    }
}