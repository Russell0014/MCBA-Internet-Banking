<<<<<<< Updated upstream
// using Microsoft.AspNetCore.Mvc.Rendering;
// namespace MCBA.Models;

// // ViewModel for Transfer
// public class TransferViewModel {
//     public int AccountNumber { get; set; }
//     public int DestAccountNumber { get; set; }
//     public SelectList? Accounts { get; set; }
//     public decimal Amount { get; set; }
//     public string? Comment { get; set; } // optional comment
=======
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
namespace MCBA.ViewModel;

// ViewModel for Transfer
public class TransferViewModel {

    [Required(ErrorMessage = "Please select an account.")]
    public int AccountNumber { get; set; }

    public int DestAccountNumber { get; set; }

    public SelectList? Accounts { get; set; }
    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive and greater than zero.")]
    public decimal Amount { get; set; }
    [StringLength(30, ErrorMessage = "Comment cannot exceed 30 characters.")]
    public string? Comment { get; set; } // optional comment
>>>>>>> Stashed changes
    

//     // in the viewmodel we have account number, a list of accounts, dest account, amount, and comment. These are what is seen on the website and captures from the user.

    
// }


