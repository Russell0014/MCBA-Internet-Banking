using AdminApi.Models.Repository;
using AdminApi.Data;
using Microsoft.EntityFrameworkCore;

namespace AdminApi.Models.DataManager
// data manager for billpay
{
    public class BillPayManager : IBillPayRepository
    {
        private readonly DatabaseContext _context;

        public BillPayManager(DatabaseContext context)
        {
            _context = context;
        }

        // get all billpay

        public async Task<IEnumerable<BillPay>> GetAllAsync()
        {
            return await _context.BillPays.ToListAsync();
        }


        // block/ unblock bill

        public async Task UpdateBillStatusAsync(int billPayId, StatusType status)
        {
            var bill = await _context.BillPays.FindAsync(billPayId);
            if (bill == null) throw new KeyNotFoundException("Bill not found"); // if bill id not valid

            if (status == StatusType.Blocked && bill.Status != StatusType.Pending) 
                throw new InvalidOperationException("Only pending bills can be blocked."); // can only block bending bill

            if (status == StatusType.Pending && bill.Status != StatusType.Blocked)
                throw new InvalidOperationException("Only blocked bills can be unblocked."); // can only unblock blocked bill

            bill.Status = status;
            _context.BillPays.Update(bill);
            await _context.SaveChangesAsync();
        }
    }
}
