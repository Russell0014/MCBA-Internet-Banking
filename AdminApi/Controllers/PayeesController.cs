using Microsoft.AspNetCore.Mvc;
using AdminApi.Data;   
using AdminApi.Models;    
using Microsoft.EntityFrameworkCore; 
using System.Collections.Generic; 
using System.Threading.Tasks; 

[ApiController]
[Route("api/[controller]")]
// controller class for payees
public class PayeesController : ControllerBase
{
    private readonly DatabaseContext _context;

    public PayeesController(DatabaseContext context)
    {
        _context = context;
    }

    // GET: api/payees
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Payee>>> GetPayees()
    {
        return await _context.Payees.ToListAsync();
    }

    // GET: api payees by postcode
    [HttpGet("filter")]
    public async Task<ActionResult<IEnumerable<Payee>>> GetPayeesByPostcode([FromQuery] string postcode)
    {
        var payees = await _context.Payees
            .Where(p => p.Postcode.Contains(postcode))
            .ToListAsync();
        return Ok(payees);
    }
}
