using AdminApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminApi.Models.Repository
{
    public interface IPayeeRepository
    {
        Task<List<Payee>> GetAllAsync();
        Task<Payee?> GetByIdAsync(int id);
        Task<List<Payee>> GetByPostcodeAsync(string postcode);
        Task UpdateAsync(Payee payee);
    }
}
