using Microsoft.AspNetCore.Mvc;
using MCBA.Models;
using MCBA.Services;
using MCBA.Data;
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
        var model = new WithdrawViewModel
        {
            Accounts = new SelectList(accounts, "Value", "Text")
        };

        return View(model);
    }

    [HttpPost, ActionName("Index")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(WithdrawViewModel model){
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
            } else {
                TempData["ErrorMessage"] = transaction.FailureReason ?? "An error occurred.";
            }

            return RedirectToAction("Index", new { accountNumber = model.AccountNumber });
        }
        ModelState.AddModelError("", "An Error occurred. Please try again.");
        return RedirectToAction("Index", new { accountNumber = model.AccountNumber });
    }

}
