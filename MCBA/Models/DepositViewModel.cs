using Microsoft.AspNetCore.Mvc.Rendering;

namespace MCBA.Models;

public class DepositViewModel
{
    public int AccountNumber { get; set; }
    public SelectList? Accounts { get; set; }
    public decimal Amount { get; set; }
    public string? Comment { get; set; } // optional comment
}