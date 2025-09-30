using MCBA.Data;
using MCBA.Models;
using MCBA.Services;
using MCBA.ViewModel;
using Microsoft.AspNetCore.Mvc;

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

    public IActionResult Create()
    {
        var viewModel = new CreateBillPayViewModel
        {

        };
        return View(viewModel);
    }
}