using AdminApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminApi.Models.Repository
{
    public interface IBillPayRepository
    {
        Task<IEnumerable<BillPay>> GetAllAsync(); // get all bills
        Task UpdateBillStatusAsync(int billPayId, StatusType status); // update bill status
    }
}
