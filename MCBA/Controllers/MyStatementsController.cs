using MCBA.Data;
using MCBA.Models;
using MCBA.Services;
using Microsoft.AspNetCore.Mvc;

namespace MCBA.Controllers;

public class MyStatementsController : Controller
{
    private readonly MyStatementsService _service;

    public MyStatementsController(DatabaseContext context)
    {
        _service = new MyStatementsService(context);
    }


    // Gets the customer ID from the session
    private int CustomerId => HttpContext.Session.GetInt32(nameof(Customer.CustomerId))!.Value;

    // GET
    public async Task<IActionResult> Index()
    {
        var customer = await _service.GetCustomerAsync(CustomerId);
        return View(customer);
    }

    // Displays the details view for a specific account's transactions with pagination
    public async Task<IActionResult> Details(int accountNumber, int page = 1)
    {
        var viewModel = await _service.GetPagedTransactionsAsync(CustomerId, accountNumber, page);
        if (viewModel == null)
        {
            return NotFound("Account not found or you don't have access to it.");
        }

        return View(viewModel);
    }
}