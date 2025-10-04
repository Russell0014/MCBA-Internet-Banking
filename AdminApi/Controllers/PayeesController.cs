using Microsoft.AspNetCore.Mvc;
using AdminApi.Data;   
using AdminApi.Models;    
using Microsoft.EntityFrameworkCore; 
using System.Collections.Generic; 
using System.Threading.Tasks; 
using AdminApi.Models.Repository;

[ApiController]
[Route("api/[controller]")]
// controller class for payees
public class PayeesController : ControllerBase
{
    private readonly IPayeeRepository _repo;

    public PayeesController(IPayeeRepository repo)
    {
        _repo = repo;
    }

    // GET: api/payees
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Payee>>> GetPayees()
    {
        var payees = await _repo.GetAllAsync();
        return Ok(payees);
    }

    // GET: api payees by postcode
    [HttpGet("filter")]
    public async Task<ActionResult<IEnumerable<Payee>>> GetPayeesByPostcode([FromQuery] string postcode)
    {
        var payees = await _repo.GetByPostcodeAsync(postcode);
        return Ok(payees);
    }
}
