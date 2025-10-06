using System.ComponentModel.DataAnnotations;

namespace AdminPortal.Models;

public class BillPayDto
{
    public enum PeriodType
    {
        [Display(Name = "One Off")] OneOff = 1,
        Monthly = 2
    }

    public enum StatusType
    {
        Completed = 1,
        Pending = 2,
        Failed = 3,
        Blocked = 4
    }

    public int BillPayId { get; set; }

    public int AccountNumber { get; set; }

    public int PayeeId { get; set; }

    [DataType(DataType.Currency)]
    [Range(0.01, double.MaxValue, ErrorMessage = "Must be a positive value.")]
    public decimal Amount { get; set; }

    [Display(Name = "Schedule Time"), DataType(DataType.DateTime)]
    public DateTime ScheduleTimeUtc { get; set; }

    public PeriodType Period { get; set; }

    public StatusType Status { get; set; }
}