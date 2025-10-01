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
        var accountsList = await _service.GetAccountsAsync(CustomerId);

        var accounts = accountsList?
            .Where(a => a.CustomerId == CustomerId)
            .Select(a => new
            {
                Value = a.AccountNumber,
                Text = $"{a.AccountNumber} {a.AccountType} ({a.Balance:C})"
            })
            .ToList();

        var viewModel = new CreateBillPayViewModel
        {
            Accounts = new SelectList(accounts, "Value", "Text"),
            ScheduleTimeUtc = DateTime.UtcNow.AddDays(1), // default to tomorrow
            Period = PeriodType.OneOff
        };

        return View(viewModel);
    }

    // POST: Handle form submission
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBillPayViewModel model)
    {
        int customerId = (int)HttpContext.Session.GetInt32(nameof(Customer.CustomerId))!;

        if (!ModelState.IsValid)
        {
            // so the dropdown doesnt dissapear if rthe validation fails
            var accountsList = await _service.GetAccountsAsync(customerId);
            model.Accounts = new SelectList(
                accountsList?.Select(a => new { Value = a.AccountNumber, Text = $"{a.AccountNumber} {a.AccountType} ({a.Balance:C})" }),
                "Value",
                "Text"
            );
            return View(model);
        }

        // Create the bill using the service
        await _service.CreateBillAsync(
            model.AccountNumber,
            model.PayeeId,
            model.Amount,
            model.ScheduleTimeUtc.ToUniversalTime(),
            model.Period
        );

        TempData["SuccessMessage"] = "Scheduled payment created successfully!";
        return RedirectToAction("Index");
    }


    public async Task<IActionResult> Cancel(int id)
    {
        await _service.CancelBillPayAsync(billPayId: id, customerId: CustomerId);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Retry(int id)
    {
        await _service.RetryBillPayAsync(billPayId: id, customerId: CustomerId);
        return RedirectToAction("Index");
    }
}