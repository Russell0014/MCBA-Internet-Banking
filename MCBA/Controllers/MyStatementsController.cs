using System.Security.Claims;
using MCBA.Data;
using MCBA.Models;
using MCBA.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace MCBA.Controllers;

public class MyStatementsController : Controller
{
    private readonly DatabaseContext _context;

    public MyStatementsController(DatabaseContext context)
    {
        _context = context;
    }

    // GET
    public async Task<IActionResult> Index(int accountNumber)
    {
        var customerId =
            (int)HttpContext.Session.GetInt32(nameof(Customer.CustomerId))!; // gets the customer ID from the session

        // Create and populate the ViewModel
        var viewModel = new MyStatementsViewModel
        {
        };

        return View(viewModel);
    }

    // POST 
    [HttpPost, ActionName("Index")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(MyStatementsViewModel model)
    {
        if (ModelState.IsValid)
        {
            return RedirectToAction("Index");
        }

        return View(model);
    }
}