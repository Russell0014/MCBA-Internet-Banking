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
            await PopulateAccounts(model, customerId);
            return View(model);
        }

        // Check payee existence
        if (!await _service.CheckPayeeId(model.PayeeId))
        {
            TempData["ErrorMessage"] = "Invalid Payee Number";
            await PopulateAccounts(model, customerId);
            return View(model);
        }

        // Check schedule time is in the future
        if (!model.ScheduleTimeUtc.HasValue || model.ScheduleTimeUtc.Value <= DateTime.UtcNow)
        {
            TempData["ErrorMessage"] = "Please enter a schedule time in the future.";
            await PopulateAccounts(model, customerId);
            return View(model);
        }


        // Create the bill using the service
        await _service.CreateBillAsync(
            model.AccountNumber,
            model.PayeeId,
            model.Amount,
            model.ScheduleTimeUtc.Value.ToUniversalTime(),
            model.Period
        );

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

    // Helper method to populate accounts in the view model
    private async Task PopulateAccounts(CreateBillPayViewModel model, int customerId)
    {
        var accountsList = await _service.GetAccountsAsync(customerId);
        model.Accounts = new SelectList(
            accountsList?.Select(a => new
            { Value = a.AccountNumber, Text = $"{a.AccountNumber} {a.AccountType} ({a.Balance:C})" }),
            "Value",
            "Text"
        );
    }
}