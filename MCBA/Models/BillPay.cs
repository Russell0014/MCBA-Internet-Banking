using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCBA.Models;

public enum PeriodType
{
    [Display(Name = "One Off")]
    OneOff = 1,
    Monthly = 2
}

public enum StatusType
{
    Completed = 1,
    Pending = 2,
    Failed = 3
}

public class BillPay
{
    public int BillPayId { get; set; }

    [ForeignKey(nameof(Account))] public int AccountNumber { get; set; }
    public virtual Account Account { get; set; }

    [ForeignKey(nameof(Payee))] public int PayeeId { get; set; }
    public virtual Payee Payee { get; set; }

    [Column(TypeName = "money")]
    [DataType(DataType.Currency)]
    [Range(0.01, double.MaxValue, ErrorMessage = "Must be a positive value.")]
    public decimal Amount { get; set; }

    public DateTime ScheduleTimeUtc { get; set; }

    public PeriodType Period { get; set; }
    
    public StatusType Status { get; set; }
}