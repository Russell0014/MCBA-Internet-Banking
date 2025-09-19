using MCBA.Data;
using MCBA.Filters;
using MCBA.Models;
using Microsoft.AspNetCore.Mvc;

namespace MCBA.Controllers;

public class CustomerController : Controller
{
    private readonly DatabaseContext _context;

    // ReSharper disable once PossibleInvalidOperationException
    private int CustomerId => HttpContext.Session.GetInt32(nameof(Customer.CustomerId))!.Value;

    public CustomerController(DatabaseContext context)
    {
        _context = context;
    }

    // Can add authorize attribute to actions.
    //[AuthorizeCustomer]
    public async Task<IActionResult> Index()
    {
        var customer = await _context.Customers.FindAsync(CustomerId);
        return View(customer);
    }

}