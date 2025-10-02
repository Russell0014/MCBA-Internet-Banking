using Microsoft.AspNetCore.Mvc;
using MCBA.Services;
using MCBA.ViewModel;
using MCBA.Models;

namespace MCBA.Controllers;

public class PasswordController : Controller
{
    private readonly PasswordService _passwordService;

    public PasswordController(PasswordService passwordService)
    {
        _passwordService = passwordService;
    } 

    // GET: Password
    public IActionResult Index()
    {
        int customerId = (int)HttpContext.Session.GetInt32(nameof(Customer.CustomerId))!;
        return View(new PasswordViewModel { CustomerId = customerId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(PasswordViewModel model)
    {
        int customerId = (int)HttpContext.Session.GetInt32(nameof(Customer.CustomerId))!;

        if (!ModelState.IsValid)
            return View(model);

        var login = _passwordService.GetLoginByCustomerId(customerId);
        if (login == null)
            return NotFound();

        // Verify current password
        if (!_passwordService.VerifyPassword(login, model.CurrentPassword))
        {
            ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
            return View(model);
        }

                // Prevent new password being same as old
        if (_passwordService.VerifyPassword(login, model.NewPassword))
        {
            ModelState.AddModelError("NewPassword", "New password cannot be the same as the current password.");
            return View(model);
        }

        // Update with new password
        _passwordService.UpdatePassword(login, model.NewPassword);

        TempData["SuccessMessage"] = "Password updated successfully!";
        return View(model);
    }
}
