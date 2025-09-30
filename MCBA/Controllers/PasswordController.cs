using SimpleHashing.Net;

using Microsoft.AspNetCore.Mvc;
using MCBA.Models;
using MCBA.Services;
using MCBA.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleHashing.Net;
using Microsoft.AspNetCore.Mvc.Rendering;
using MCBA.ViewModel;

namespace MCBA.Controllers;

public class PasswordController : Controller
{
    private readonly DatabaseContext _context;

    public PasswordController(DatabaseContext context)
    {
        _context = context;
    } 

    // GET: Password
    public IActionResult Index(){
        int customerId = (int)HttpContext.Session.GetInt32(nameof(Customer.CustomerId))!; // gets the customer ID from the session
        var customer = _context.Login.Find(customerId); // finds the login info based on the customer ID
        if (customer == null)
        {
            return NotFound();
        }
        var model = new PasswordViewModel 
        {
            CustomerId = customer.CustomerId,
            Password = customer.PasswordHash

        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PasswordViewModel model){

        if (ModelState.IsValid)
        {
            var customer = await _context.Login.FindAsync(model.CustomerId); // finds the login based on the customer ID in the model
            if (customer == null)
            {
                return NotFound();
            }

            // Update the password of the customer
            customer.PasswordHash = model.PasswordHash;

            _context.Update(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

}