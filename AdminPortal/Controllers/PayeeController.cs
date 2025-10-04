using Microsoft.AspNetCore.Mvc;

namespace AdminPortal.Controllers;

public class PayeeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}