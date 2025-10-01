using MCBA.Data;
using MCBA.Models;
using MCBA.Services;
using MCBA.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace MCBA.Controllers;

public class BillPayController : Controller
{
    private readonly BillPayService _service;

    public BillPayController(DatabaseContext context)
    {
        _service = new BillPayService(context);
    }


    // Gets the customer ID from the session
    private int CustomerId => HttpContext.Session.GetInt32(nameof(Customer.CustomerId))!.Value;

    // GET
    public async Task<IActionResult> Index()
    {
        var viewModel = await _service.GetBillPaysAsync(CustomerId);

        return View(viewModel);
    }

    public async Task<IActionResult> Create()
    {
        int customerId = (int)HttpContext.Session.GetInt32(nameof(Customer.CustomerId))!;

        var accounts = await _service.GetAccountsAsync(customerId);

        var viewModel = new CreateBillPayViewModel
        {
            Accounts = new SelectList(accounts, "AccountNumber", "AccountNumber"),
            ScheduleTimeUtc = DateTime.UtcNow.AddDays(1), // default to tomorrow
            Period = PeriodType.OneOff
        };

        return View(viewModel);
    }
}