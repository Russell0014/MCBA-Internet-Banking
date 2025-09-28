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
        var customer = _context.Customers.Find(updatedCustomer.CustomerId);
        if (customer == null) return false;

        customer.Name = updatedCustomer.Name;
        customer.TFN = updatedCustomer.TFN;
        customer.Address = updatedCustomer.Address;
        customer.City = updatedCustomer.City;
        customer.State = updatedCustomer.State;
        customer.PostCode = updatedCustomer.PostCode;
        customer.Mobile = updatedCustomer.Mobile;

        _context.SaveChanges();
        return true;
    }

}
