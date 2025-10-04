using Microsoft.AspNetCore.Mvc;

namespace AdminPortal.Controllers;

public class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}