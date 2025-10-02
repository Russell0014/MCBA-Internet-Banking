using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MCBA.Models;

namespace MCBA.ViewModel;

public class CreateBillPayViewModel
{

    public string ScheduleTimeFormatted => ScheduleTimeUtc.ToLocalTime().ToString("yyyy-MM-ddTHH:mm");

    [Required(ErrorMessage = "Please select an account.")]
    public int AccountNumber { get; set; }

    public SelectList? Accounts { get; set; }

    [Required(ErrorMessage = "Payee number is required.")]
    [Display(Name = "Payee Number")]
    public int PayeeId { get; set; }

    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive and greater than zero.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Schedule time is required.")]
    [DataType(DataType.DateTime, ErrorMessage = "Please enter a valid date and time.")]
    [Display(Name = "Schedule Time")] // Changed: Removed "UTC" for user clarity
    public DateTime ScheduleTimeUtc { get; set; }

    [Required(ErrorMessage = "Period is required.")]
    [Display(Name = "Payment Period")]
    public PeriodType Period { get; set; }
}