using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCBA.Models;

public enum PeriodType
{
    OneOff = 1,
    Monthly = 2
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
    public decimal Amount { get; set; }

    public DateTime ScheduleTimeUtc { get; set; }

    public PeriodType Period { get; set; }
}