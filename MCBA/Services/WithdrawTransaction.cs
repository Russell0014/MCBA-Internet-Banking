using MCBA.Data;
using MCBA.Models;

namespace MCBA.Services;

public class WithdrawTransaction : ITransaction
{
    public const decimal AtmWithdrawFee = 0.01m;
    private const int MaxFreeTransfer = 2;

    public int TransactionID { get; set; }
    public TransactionType TransactionType { get; set; } = TransactionType.Withdraw;
    public required Account Account { get; set; }
    public Account? DestinationAccount { get; set; } = null;
    public decimal Amount { get; set; }
    public string? Comment { get; set; }
    public DateTime TransactionTimeUtc { get; set; } = DateTime.UtcNow;
    private decimal _totalDeduction;
    public decimal Fee { get; private set; } = 0m; // fee injected externally

    public string? FailureReason { get; private set; } // 

    public void SetFee(decimal fee)
    {
        Fee = fee;
    }


    public bool Validate()
    {
    
        var minBalance = TransactionRules.GetMinBalance(Account); // get min balance based on account type

        _totalDeduction = Amount + Fee;
        
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
    public decimal GetTotalDeduction() => _totalDeduction;

}
