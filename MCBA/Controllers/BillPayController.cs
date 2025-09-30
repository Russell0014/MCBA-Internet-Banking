using MCBA.Data;
using Microsoft.AspNetCore.Mvc;

namespace MCBA.Controllers;

public class BillPayController : Controller
{
    private readonly DatabaseContext _context;

    public BillPayController(DatabaseContext context)
    {
        _context = context;
    }

    // GET
    public ActionResult Index()
    {
        return View();
    }
}