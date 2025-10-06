using AdminPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPortal.Controllers;

[AllowAnonymous]
[Route("/")]
public class LoginController : Controller
{
    public IActionResult Login()
    {
        // Check if admin is already logged in
        var isAdmin = HttpContext.Session.GetString("IsAdmin");
        if (!string.IsNullOrEmpty(isAdmin))
        {
            return RedirectToAction("Index", "Payee"); 
        }

        return View();
    }

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        //Hardcoded admin login
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || username != "admin" || password != "admin")
        {
            ModelState.AddModelError("LoginFailed", "Login failed, please try again.");
            return View(new Login { UserName = username });
        }

        // Set admin session flag
        HttpContext.Session.SetString("IsAdmin", "true");

        return RedirectToAction("Index", "Payee");
    }

    [Route("LogoutNow")]
    public IActionResult Logout()
    {
        // Logout customer.
        HttpContext.Session.Clear();

        return RedirectToAction("Login", "Login");
    }
}