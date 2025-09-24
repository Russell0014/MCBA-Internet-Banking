using Microsoft.AspNetCore.Mvc.Rendering;
namespace MCBA.Models;

// ViewModel for Withdraw
public class WithdrawViewModel {
    public int AccountNumber { get; set; }
    public SelectList? Accounts { get; set; }
    public decimal Amount { get; set; }
    public string? Comment { get; set; } // optional comment
    

    // in the viewmodel we have account number, a list of accounts, amount, and comment. These are what is seen on the website and captures from the user.

    
}


