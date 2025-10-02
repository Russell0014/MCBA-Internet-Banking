using MCBA.Models;

namespace MCBA.Services;

public class BillPayTransaction : ITransaction
{
    public int TransactionID { get; set; }
    public TransactionType TransactionType { get; set; } = TransactionType.BillPay;
    public Account Account { get; set; }
    public Account? DestinationAccount { get; set; }
    public decimal Amount { get; set; }
    public string? Comment { get; set; }
    public DateTime TransactionTimeUtc { get; set; }
    public decimal Fee { get; } = 0m;
    public string? FailureReason { get; }


    public bool Validate()
    {
        var minBalance = TransactionRules.GetMinBalance(Account); // get min balance based on account type

        // amount must be positive and less than balance
        if (Amount <= 0)
        {
            return false;
        }

        if (Account.Balance - Amount < minBalance)
        {
            return false;
        }

        return true;
    }

    public void ExecuteTransaction()
    {
        Account.Balance -= Amount;
    }
}