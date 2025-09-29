using System.Security.Claims;
using MCBA.Data;
using MCBA.Models;
using MCBA.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace MCBA.Controllers;

public class MyStatementsController : Controller
{
    private readonly DatabaseContext _context;

    public MyStatementsController(DatabaseContext context)
    {
        _context = context;
    }
    
    private int CustomerId => HttpContext.Session.GetInt32(nameof(Customer.CustomerId))!.Value; // gets the customer ID from the session

    // GET
    public async Task<IActionResult> Index()
    {
        // Create and populate the view
        var customer = await _context.Customers.FindAsync(CustomerId);
        return View(customer);
    }

    public async Task<IActionResult> Details(int accountNumber, int page = 1)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber && 
                                      a.CustomerId == CustomerId);
    
        if (account == null)
        {
            return NotFound("Account not found or you don't have access to it.");
        }
        
        const int pageSize = 4;
        var pagedTransactions = _context.Transactions
            .Where(t => t.AccountNumber == accountNumber)
            .OrderByDescending(t => t.TransactionTimeUtc).ToPagedList(page, pageSize);

        var viewModel = new MyStatementsViewModel
        {
            Account = account,
            Transactions = pagedTransactions
        };
        
        return View(viewModel);
    }
}