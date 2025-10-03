using AdminApi.Models.Repository;
using AdminApi.Data;
using Microsoft.EntityFrameworkCore;

namespace AdminApi.Models.DataManager
{
    public class PayeeManager : IPayeeRepository
    {
        private readonly DatabaseContext _context;

        public PayeeManager(DatabaseContext context)
        {
            _context = context;
        }

        // get all payees

        public async Task<List<Payee>> GetAllAsync()
        {
            return await _context.Payees.ToListAsync();
        }

        // get by id

        public async Task<Payee?> GetByIdAsync(int id)
        {
            return await _context.Payees.FindAsync(id);
        }

        // get payees by postcode

        public async Task<List<Payee>> GetByPostcodeAsync(string postcode)
        {
            return await _context.Payees
                .Where(p => p.Postcode == postcode)
                .ToListAsync();
        }

        // save changes

        public async Task UpdateAsync(Payee payee)
        {
            _context.Payees.Update(payee);
            await _context.SaveChangesAsync();
        }
    }
}
