using Microsoft.AspNetCore.Mvc;
using MCBA.Models;
using MCBA.Services;
using MCBA.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleHashing.Net;



namespace MCBA.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly DatabaseContext _context;

        public TransactionsController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: Withdraw Transaction
        public IActionResult Withdraw(int accountNumber){
            var model = new WithdrawViewModel { AccountNumber = accountNumber }; // sets the account number to the one passed in the URL
            return View(model);
        }

        [HttpPost, ActionName("Withdraw")]
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
                    _context, TransactionType.Withdraw, account, model.Amount, model.Comment);  // uses the account, amount, and comment from the model

                var success = transaction.Execute();
                if (success)
                {
                    _context.SaveChanges();
                    TempData["Message"] = "Withdrawal successful!";
                    return RedirectToAction("Details", "Accounts", new { accountNumber = model.AccountNumber });
                }
                ModelState.AddModelError("", "Withdrawal failed.");
                return View(model);

            }
            return View(model);
        }

    }
}