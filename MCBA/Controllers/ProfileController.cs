using Microsoft.AspNetCore.Mvc;
using MCBA.Models;
using MCBA.Data;
using MCBA.ViewModel;

namespace MCBA.Controllers;

public class ProfileController : Controller
{
    private readonly DatabaseContext _context;

    public ProfileController(DatabaseContext context)
    {
        _context = context;
    } 

    // GET: Profile
    public IActionResult Index(){
        int customerId = (int)HttpContext.Session.GetInt32(nameof(Customer.CustomerId))!; // gets the customer ID from the session
        var customer = _context.Customers.Find(customerId); // finds the customer based on the customer ID
        if (customer == null)
        {
            return NotFound();
        }
        var model = new ProfileViewModel
        {
            CustomerId = customer.CustomerId,
            Name = customer.Name,
            Address = customer.Address,
            City = customer.City,
            State = customer.State,
            PostCode = customer.PostCode,
            Mobile = customer.Mobile
        };

        return View(model);
    }

        // GET: Profile/Edit
    public async Task<IActionResult> Edit()
    {
        int customerId = (int)HttpContext.Session.GetInt32(nameof(Customer.CustomerId))!;
        var customer = await _context.Customers.FindAsync(customerId);

        if (customer == null)
            return NotFound();

        var model = new ProfileViewModel
        {
            CustomerId = customer.CustomerId,
            Name = customer.Name,
            Address = customer.Address,
            City = customer.City,
            State = customer.State,
            PostCode = customer.PostCode,
            Mobile = customer.Mobile
        };

        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileViewModel model){

        if (ModelState.IsValid)
        {
            var customer = await _context.Customers.FindAsync(model.CustomerId); // finds the customer based on the customer ID in the model
            if (customer == null)
            {
                return NotFound();
            }

            // Update customer properties
            customer.Name = model.Name;
            customer.Address = model.Address;
            customer.City = model.City;
            customer.State = model.State;
            customer.PostCode = model.PostCode;
            customer.Mobile = model.Mobile;

            _context.Update(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

}