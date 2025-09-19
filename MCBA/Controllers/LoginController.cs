using MCBA.Data;
using MCBA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleHashing.Net;


// Bonus Material: Implement global authorisation check.
//[AllowAnonymous]
namespace MCBA.Controllers;

[AllowAnonymous]
[Route("/Login")]
public class LoginController : Controller
{
    private static readonly ISimpleHash SSimpleHash = new SimpleHash();

    private readonly DatabaseContext _context;

    public LoginController(DatabaseContext context)
    {
        _context = context;
    }

    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string loginId, string password)
    {
        var login = await _context.Logins.FindAsync(loginId);
        if(login == null || string.IsNullOrEmpty(password) || !SSimpleHash.Verify(password, login.PasswordHash))
        { 
            ModelState.AddModelError("LoginFailed", "Login failed, please try again.");
            return View(new Login { LoginId = loginId });
        }

        // Login customer.
        HttpContext.Session.SetInt32(nameof(Customer.CustomerId), login.CustomerId);
        HttpContext.Session.SetString(nameof(Customer.Name), login.Customer.Name);

        return RedirectToAction("Index", "Customer");
    }

    [Route("LogoutNow")]
    public IActionResult Logout()
    {
        // Logout customer.
        HttpContext.Session.Clear();

        return RedirectToAction("Index", "Home");
    }
}