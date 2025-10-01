using MCBA.Data;
using MCBA.Models;
using MCBA.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace MCBA.Services;

public class BillPayService
{
    private readonly DatabaseContext _context;

    // Initialises a new instance of the BillPay service
    public BillPayService(DatabaseContext context)
    {
        _context = context;
    }

    // Retrieves a customer by id
    public async Task<Customer?> GetCustomerAsync(int customerId)
    {
        return await _context.Customers.FindAsync(customerId);
    }

    // Retrieves an account by account number and customer id
    public async Task<List<Account>?> GetAccountsAsync(int customerId)
    {
        return await _context.Accounts.Where(a => a.CustomerId == customerId).ToListAsync();
    }

    // Retrieves BillPays for all the customer's accounts
    public async Task<BillPayViewModel?> GetBillPaysAsync(int customerId)
    {
        var accounts = await GetAccountsAsync(customerId);
        if (accounts == null || accounts.Count == 0) return null;

        var accountNumbers = accounts.Select(a => a.AccountNumber).ToList();

        var bills = await _context.BillPays
            .Where(b => accountNumbers.Contains(b.AccountNumber))
            .OrderBy(b => b.ScheduleTimeUtc)
            .ToListAsync();

        return new BillPayViewModel
        {
            BillPayList = bills
        };
    }

    // create new bill
    public async Task<BillPay> CreateBillAsync(int accountNumber, int payeeId, decimal amount, DateTime scheduleTimeUtc, PeriodType period)
    {
        var bill = new BillPay
        { 
            AccountNumber = accountNumber,
            PayeeId = payeeId,
            Amount = amount,
            ScheduleTimeUtc = scheduleTimeUtc,
            Period = period,
            Status = StatusType.InProgress // status starts as in progress
        };

        _context.BillPays.Add(bill); // add bill to table
        await _context.SaveChangesAsync();
        return bill;
    }




}