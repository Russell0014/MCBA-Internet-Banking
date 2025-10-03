using Microsoft.AspNetCore.Mvc;
using MCBA.Data;   
using MCBA.Models;    
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
}
