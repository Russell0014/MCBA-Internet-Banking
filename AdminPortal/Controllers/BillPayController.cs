using Microsoft.AspNetCore.Mvc;

namespace AdminPortal.Controllers;

public class BillPayController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}