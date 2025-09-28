using MCBA.Data;
using MCBA.Models;

namespace MCBA.Services;

public class ProfileService
{
    private readonly DatabaseContext _context;

    public ProfileService(DatabaseContext context)
    {
        _context = context;
    }

    // Get profile info from customer
    public Customer? GetCustomer(int customerId)
    {
        return _context.Customers.Find(customerId);
    }

    // Update general customer info
    public bool UpdateCustomerInfo(Customer updatedCustomer)
    {
        var customer = _context.Customers.Find(updatedCustomer.CustomerID);
        if (customer == null) return false;

        customer.Name = updatedCustomer.Name;
        customer.TFN = updatedCustomer.TFN;
        customer.Address = updatedCustomer.Address;
        customer.City = updatedCustomer.City;
        customer.State = updatedCustomer.State;
        customer.Postcode = updatedCustomer.Postcode;
        customer.Mobile = updatedCustomer.Mobile;

        _context.SaveChanges();
        return true;
    }

    // Update password only
    public bool ChangePassword(int customerId, string newPasswordHash)
    {
        var customer = _context.Customers.Find(customerId);
        if (customer == null) return false;

        customer.PasswordHash = newPasswordHash; 
        _context.SaveChanges();
        return true;
    }
}
