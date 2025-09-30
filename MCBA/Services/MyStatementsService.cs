using MCBA.Data;
using MCBA.Models;
using MCBA.ViewModel;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace MCBA.Services;

// Service class for handling statements-related operations.
public class MyStatementsService
{
    private readonly DatabaseContext _context;

    // Initialises a new instance of the MyStatementsService
    public MyStatementsService(DatabaseContext context)
    {
        _context = context;
    }

    // Retrieves a customer by id
    public async Task<Customer?> GetCustomerAsync(int customerId)
    {
        return await _context.Customers.FindAsync(customerId);
    }

    // Retrieves an account by account number and customer id
    public async Task<Account?> GetAccountAsync(int accountNumber, int customerId)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber && a.CustomerId == customerId);
    }

    // Retrieves paged transactions for a specific account
    public async Task<MyStatementsViewModel?> GetPagedTransactionsAsync(int customerId, int accountNumber, int page = 1)
    {
        const int pageSize = 4;
        var account =
            await GetAccountAsync(accountNumber, customerId);
        if (account == null) return null;

        var pagedTransactions = _context.Transactions
            .Where(t => t.AccountNumber == accountNumber)
            .OrderByDescending(t => t.TransactionTimeUtc)
            .ToPagedList(page, pageSize);

        return new MyStatementsViewModel
        {
            Account = account,
            Transactions = pagedTransactions
        };
    }
}