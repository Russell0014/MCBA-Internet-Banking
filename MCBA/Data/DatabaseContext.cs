using Microsoft.EntityFrameworkCore;
using MCBA.Models;

namespace MCBA.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options) { }

        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Login> Logins { get; set; } = null!;
        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<BillPay> BillPays { get; set; } = null!;
        public DbSet<Payee> Payees { get; set; } = null!;
        
    }
}
