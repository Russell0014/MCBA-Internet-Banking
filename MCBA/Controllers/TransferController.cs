using Microsoft.AspNetCore.Mvc;
using MCBA.Models;
using MCBA.Services;
using MCBA.Data;
using MCBA.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleHashing.Net;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MCBA.Controllers;

public class TransferController : Controller
{
    private readonly DatabaseContext _context;

    public TransferController(DatabaseContext context)
    {
        _context = context;
    } 
    
    // GET: Transfer Transaction
    public IActionResult Index(int accountNumber){
        int customerId = (int)HttpContext.Session.GetInt32(nameof(Customer.CustomerId))!; // gets the customer ID from the session
        // Fetch all accounts for this customer
        var accounts = _context.Accounts
            .Where(a => a.CustomerId == customerId)
            .Select(a => new
            {
                Value = a.AccountNumber,
                Text = $"{a.AccountNumber} {a.AccountType} ({a.Balance:C})"
            })
            .ToList();
        // Create the ViewModel with the SelectList
        var model = new TransferViewModel
        {
            Accounts = new SelectList(accounts, "Value", "Text")
        };

        return View(model);
    }

    [HttpPost, ActionName("Index")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(TransferViewModel model){
        if (ModelState.IsValid)
        {
            var account = await _context.Accounts.FindAsync(model.AccountNumber); // finds the account based on the account number in the model
            var destAccount = await _context.Accounts.FindAsync(model.DestAccountNumber); // finds the destination account based on the dest account number in the model
            if (account == null)
            {
                return NotFound();
            }

            var transaction = TransactionFactory.CreateTransaction( // creates a withdraw transaction using the factory
                TransactionType.Transfer, account, model.Amount, model.DestAccountNumber, model.Comment);  // uses the account, amount, dest account, and comment from the model

            // Assign the tracked destination account
            transaction.DestinationAccount = destAccount;
            var transactionService = new TransactionService(_context);
            var success = transactionService.Execute(transaction);

            if (success)
            {
                TempData["SuccessMessage"] = "Transfer successful!";
            } else {
                TempData["ErrorMessage"] = transaction.FailureReason ?? "An error occurred.";
            }

            return RedirectToAction("Index", new { accountNumber = model.AccountNumber });
        }
        ModelState.AddModelError("", "An Error occurred. Please try again.");
        return RedirectToAction("Index", new { accountNumber = model.AccountNumber });
    }

}
