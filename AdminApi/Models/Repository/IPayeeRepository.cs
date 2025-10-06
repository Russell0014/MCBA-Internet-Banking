using AdminApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminApi.Models.Repository
{
    public interface IPayeeRepository
    {
        Task<IEnumerable<Payee>> GetAllAsync();
        Task<IEnumerable<Payee>> GetByPostcodeAsync(string postcode);
        Task<Payee?> GetByIdAsync(int id);
        Task UpdateAsync(Payee payee);
    }
}
