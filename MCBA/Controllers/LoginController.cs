using MCBA.Data;
using MCBA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleHashing.Net;


namespace MCBA.Controllers;

[AllowAnonymous]
[Route("/")]
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

        var id = HttpContext.Session.GetInt32("CustomerId");

        // Checks if user is already logged in
        if (id != null)
        {
            return RedirectToAction("Index", "Customer");
        }
        
        // Login customer.
        HttpContext.Session.SetInt32("CustomerId", login.CustomerId);

        return RedirectToAction("Index", "Customer");
    }

    [Route("LogoutNow")]
    public IActionResult Logout()
    {
        // Logout customer.
        HttpContext.Session.Clear();

        return RedirectToAction("Login", "Login");
    }
}