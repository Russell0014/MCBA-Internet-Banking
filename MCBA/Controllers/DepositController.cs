using System.Security.Claims;
using MCBA.Data;
using MCBA.Models;
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
    public async Task<IActionResult> Index()
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
    [HttpPost]
    public async Task<IActionResult> Index(DepositViewModel viewModel)
    {

        return RedirectToAction("Index");
    }
}