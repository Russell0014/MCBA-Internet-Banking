// using MCBA.Data;
// using MCBA.Models;

// namespace MCBA.Services;

// public class TransferTransaction : ITransaction
// {
//     private const int MaxFreeTransfer = 2;

//     public int TransactionID { get; set; }
//     public TransactionType TransactionType { get; set; } = TransactionType.Transfer;
//     public required Account Account { get; set; }
//     public Account? DestinationAccount { get; set; } = null;
//     public decimal Amount { get; set; }
//     public string? Comment { get; set; }
//     public DateTime TransactionTimeUtc { get; set; } = DateTime.UtcNow;
//     private decimal _totalDeduction;
    

//     private readonly DatabaseContext _context;

//     public TransferTransaction(DatabaseContext context)
//     {
//         _context = context;
//     }

//     public string? FailureReason { get; private set; } // 

//     // Validate the transaction
//     public bool Validate()
//     {
//         if (DestAccount == null)
//         {
//             FailureReason = "Destination account is required for transfer.";
//             return false;
//         }
        
//         return true;
//     }

//     public bool Execute()
//     {
//         if (!Validate()) return false;
//         // Record transaction in DB
//         _context.Transactions.Add(new Transaction
//         {
//             Account = Account,
//             DestinationAccount = DestAccount,
//             Amount = Amount,
//             Comment = Comment,
//             TransactionTimeUtc = TransactionTimeUtc,
//             TransactionType = TransactionType
//         });

//         _context.SaveChanges();
//         return true;
//     }

// }
