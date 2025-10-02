using MCBA.Data;
using MCBA.Models;
using SimpleHashing.Net;

namespace MCBA.Services;

public class PasswordService // service for changing password
{
    private readonly DatabaseContext _context;
    private readonly ISimpleHash _hasher;

    public PasswordService(DatabaseContext context, ISimpleHash hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    public Login? GetLoginByCustomerId(int customerId) // get login from the customer id of current session
    {
        return _context.Logins.FirstOrDefault(l => l.CustomerId == customerId);
    }

    public bool VerifyPassword(Login login, string password) // verify the hashed password with the one entered 
    {
        return _hasher.Verify(password, login.PasswordHash);
    }

    public void UpdatePassword(Login login, string newPassword) // update new password in db
    {
        login.PasswordHash = _hasher.Compute(newPassword);
        _context.Update(login);
        _context.SaveChanges();
    }
}
