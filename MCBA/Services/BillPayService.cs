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
            .Include(b => b.Payee)
            .Where(b => accountNumbers.Contains(b.AccountNumber))
            .OrderBy(b => b.ScheduleTimeUtc)
            .ToListAsync();

        return new BillPayViewModel
        {
            BillPayList = bills
        };
    }

    // create new bill
    public async Task<BillPay> CreateBillAsync(int accountNumber, int payeeId, decimal amount, DateTime scheduleTimeUtc,
        PeriodType period)
    {
        var bill = new BillPay
        {
            AccountNumber = accountNumber,
            PayeeId = payeeId,
            Amount = amount,
            ScheduleTimeUtc = scheduleTimeUtc,
            Period = period,
            Status = StatusType.Pending // status starts as in progress
        };

        _context.BillPays.Add(bill); // add bill to table
        await _context.SaveChangesAsync();
        return bill;
    }

    public async Task CancelBillPayAsync(int billPayId, int customerId)
    {
        // Finds the bill using BillPay id and customer id
        var bill = await _context.BillPays
            .Where(b => b.BillPayId == billPayId && b.Account.CustomerId == customerId)
            .SingleOrDefaultAsync();

        if (bill != null)
        {
            _context.BillPays.Remove(bill);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RetryBillPayAsync(int billPayId, int customerId)
    {
        // Finds the bill using BillPay id and customer id
        var bill = await _context.BillPays
            .Where(b => b.BillPayId == billPayId && b.Account.CustomerId == customerId)
            .SingleOrDefaultAsync();

        if (bill != null)
        {
            bill.Status = StatusType.Pending;
            await _context.SaveChangesAsync();
        }
    }

    public async Task PayBillsAsync(CancellationToken cancellationToken)
    {
        var bills = await _context.BillPays
            .Include(b => b.Account).Include(b => b.Payee) // Load Account and Payee for each bill
            .Where(b => b.Status == StatusType.Pending &&
                        b.ScheduleTimeUtc <= DateTime.UtcNow) // Process due or past bills
            .ToListAsync(cancellationToken);

        foreach (var bill in bills)
        {
            // Create and execute the transaction
            var transaction = TransactionFactory.CreateTransaction(TransactionType.BillPay, account: bill.Account,
                amount: bill.Amount, comment: $"BillPay to {bill.Payee.Name}");
            var transactionService = new TransactionService(_context);
            var success = transactionService.Execute(transaction);

            if (success)
            {
                bill.Status = StatusType.Completed;
                // Creates a new bill for the next month 
                if (bill.Period == PeriodType.Monthly)
                {
                    var newBill = new BillPay
                    {
                        AccountNumber = bill.AccountNumber,
                        PayeeId = bill.PayeeId,
                        Amount = bill.Amount,
                        ScheduleTimeUtc = bill.ScheduleTimeUtc.AddMonths(1), // Next month
                        Period = bill.Period,
                        Status = StatusType.Pending // New bill starts pending
                    };
                    _context.BillPays.Add(newBill); // Add the new bill as a separate bill
                }
            }
            else
            {
                bill.Status = StatusType.Failed;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}