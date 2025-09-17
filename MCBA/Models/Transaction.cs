using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCBA.Models;

public enum TransactionType
{
    Deposit = 1,
    Withdraw = 2,
    Transfer = 3,
    ServiceCharge = 4,
    BillPay = 5
}

public class Transaction
{
    public int TransactionId { get; set; }

    public TransactionType TransactionType { get; set; }

    [ForeignKey(nameof(Account))] public int AccountNumber { get; set; }
    public virtual Account Account { get; set; }

    [ForeignKey(nameof(DestinationAccount))]
    public int? DestinationAccountNumber { get; set; }

    public virtual Account DestinationAccount { get; set; }

    [Column(TypeName = "money")]
    [DataType(DataType.Currency)]
    public decimal Amount { get; set; }

    [StringLength(30)] public string? Comment { get; set; }

    public DateTime TransactionTimeUtc { get; set; }
}