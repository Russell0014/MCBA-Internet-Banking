using Microsoft.AspNetCore.Mvc;
using AdminApi.Data;   
using AdminApi.Models;    
using Microsoft.EntityFrameworkCore; 
using System.Collections.Generic; 
using System.Threading.Tasks; 
using AdminApi.Models.Repository;

[ApiController]
[Route("api/[controller]")]
// controller class for billpay
public class BillPayController : ControllerBase
{
    private readonly IBillPayRepository _repo;

    public BillPayController(IBillPayRepository repo)
    {
        _repo = repo;
    }
    // change the billpay status to blocked

    [HttpPut("{id}/block")]
    public async Task<IActionResult> BlockBill(int id)
    {
        try
        {
            await _repo.UpdateBillStatusAsync(id, StatusType.Blocked);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // change the billpay status back to pending

    [HttpPut("{id}/unblock")]
    public async Task<IActionResult> UnblockBill(int id)
    {
        try
        {
            await _repo.UpdateBillStatusAsync(id, StatusType.Pending);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET: api/billpay
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BillPay>>> GetBillPay()
    {
        var billpay = await _repo.GetAllAsync();
        return Ok(billpay);
    }



  
}



