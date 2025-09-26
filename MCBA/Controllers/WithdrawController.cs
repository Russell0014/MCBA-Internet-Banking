using Microsoft.AspNetCore.Mvc;
using MCBA.Models;
using MCBA.Services;
using MCBA.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleHashing.Net;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MCBA.Controllers;

public class WithdrawController : Controller
{
    private readonly DatabaseContext _context;

    public WithdrawController(DatabaseContext context)
    {
        _context = context;
    } 
    
    // GET: Withdraw Transaction
    public IActionResult Withdraw(int accountNumber){
        int customerId = (int)HttpContext.Session.GetInt32(nameof(Customer.CustomerId))!; // gets the customer ID from the session
        // Fetch all accounts for this customer
        var accounts = _context.Accounts
            .Where(a => a.CustomerId == customerId)
            .ToList();
        // Create the ViewModel with the SelectList
        var model = new WithdrawViewModel
        {
            Accounts = new SelectList(accounts, "AccountNumber", "AccountType") 
        };

        return View(model);
    }

    [HttpPost, ActionName("withdraw")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(WithdrawViewModel model){
        if (ModelState.IsValid)
        {
            var account = await _context.Accounts.FindAsync(model.AccountNumber); // finds the account based on the account number in the model
            if (account == null)
            {
                return NotFound();
            }

            var transaction = TransactionFactory.CreateTransaction( // creates a withdraw transaction using the factory
                TransactionType.Withdraw, account, model.Amount, model.Comment);  // uses the account, amount, and comment from the model

            var transactionService = new TransactionService(_context);
            var success = transactionService.Execute(transaction);

            if (success)
            {
                TempData["SuccessMessage"] = "Withdrawal successful!";
                return RedirectToAction("Withdraw", new { accountNumber = model.AccountNumber });
            } else {
                TempData["ErrorMessage"] = transaction.FailureReason ?? "An error occurred.";
                return RedirectToAction("Withdraw", new { accountNumber = model.AccountNumber });

            }
        }
        ModelState.AddModelError("", "An Error occurred. Please try again.");
        return RedirectToAction("Withdraw", new { accountNumber = model.AccountNumber });
    }

}
