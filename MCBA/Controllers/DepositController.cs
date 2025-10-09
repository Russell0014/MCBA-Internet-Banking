using MCBA.Data;
using MCBA.Models;
using MCBA.Services;
using MCBA.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MCBA.Controllers;

public class DepositController : Controller
{
    private readonly DatabaseContext _context;

    public DepositController(DatabaseContext context)
    {
        _context = context;
    }

    // GET
    public async Task<IActionResult> Index(int accountNumber)
    {
        var customerId = (int)HttpContext.Session.GetInt32(nameof(Customer.CustomerId))!; // gets the customer ID from the session
        // Fetch all accounts for this customer
        var accounts = _context.Accounts
            .Where(a => a.CustomerId == customerId)
            .Select(a => new
            {
                Value = a.AccountNumber,
                Text = $"{a.AccountNumber} {a.AccountType} ({a.Balance:C})"
            })
            .ToList();
        
        // Create and populate the ViewModel
        var viewModel = new DepositViewModel
        {
            Accounts = new SelectList(accounts, "Value", "Text")
        };

        return View(viewModel);
    }

    // POST 
    [HttpPost, ActionName("Index")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(DepositViewModel model)
    {
        if (ModelState.IsValid)
        {
            var account = await _context.Accounts.FindAsync(model.AccountNumber); // finds the account based on the account number in the model
            if (account == null)
            {
                TempData["ErrorMessage"] = "Account not found.";
                return RedirectToAction("Index");
            }

            var transaction = TransactionFactory.CreateTransaction( // creates a deposit transaction using the factory
                TransactionType.Deposit, account, model.Amount, model.Comment); // uses the account, amount, and comment from the model

            var transactionService = new TransactionService(_context);
            var success = transactionService.Execute(transaction);

            if (success)
            {
                TempData["SuccessMessage"] = "Deposit successful!";
            }
            else
            {
                TempData["ErrorMessage"] = transaction.FailureReason ?? "An error occurred.";
            }

            // Redirect with accountNumber to pre-select it
            return RedirectToAction("Index", new { accountNumber = model.AccountNumber });
        }

        // On validation failure, return to view with errors (don't redirect)
        TempData["ErrorMessage"] = "Please correct the errors below.";
        return View(model);
    }
}