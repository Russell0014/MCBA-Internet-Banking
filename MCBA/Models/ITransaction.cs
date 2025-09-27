namespace MCBA.Models;

public interface ITransaction
{
    int TransactionID { get; set; }
    TransactionType TransactionType { get; set; }
    Account Account { get; set; }
    Account? DestinationAccount { get; set; }
    decimal Amount { get; set; }
    string? Comment { get; set; }
    DateTime TransactionTimeUtc { get; set; }
    string? FailureReason { get; }

    bool Validate();
}