using AdminApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminApi.Data;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<BillPay> BillPays { get; set; }
    public DbSet<Payee> Payees { get; set; }
}