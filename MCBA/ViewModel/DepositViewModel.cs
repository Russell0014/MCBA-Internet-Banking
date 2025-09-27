using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MCBA.Models;

public class DepositViewModel
{
    [Required(ErrorMessage = "Please select an account.")]
    public int AccountNumber { get; set; }
    public SelectList? Accounts { get; set; }
    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive and greater than zero.")]
    public decimal Amount { get; set; }
    [StringLength(30, ErrorMessage = "Comment cannot exceed 30 characters.")]
    public string? Comment { get; set; } // optional comment
}