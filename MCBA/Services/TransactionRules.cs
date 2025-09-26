using MCBA.Data;
using System.Data.SqlClient;
using MCBA.Models;


public static class TransactionRules
{
    private const int MaxFreeTransfers = 2;
    public const decimal AtmWithdrawFee = 0.01m;

    // Determines if the ATM withdrawal fee should be applied based on the number of prior withdrawals/transfers

    public static bool ShouldApplyWithdrawFee(int accountNumber, DatabaseContext context)
    {
        // Count withdrawals and transfers for this account
        var count = context.Transactions
            .Where(t => t.Account.AccountNumber == accountNumber 
                        && (t.TransactionType == TransactionType.Withdraw || t.TransactionType == TransactionType.Transfer))
            .Count();

        return count >= MaxFreeTransfers;
    }

    public static decimal GetAtmWithdrawFee(int accountNumber, DatabaseContext context)
    {
        return ShouldApplyWithdrawFee(accountNumber, context) ? AtmWithdrawFee : 0;
    }


    // Get minimum balance based on account type
    public static decimal GetMinBalance(Account account) =>
    account.AccountType switch
    {
        AccountType.Checking => -500m,
        AccountType.Savings => 0m,
        _ => 0m
    };
}
