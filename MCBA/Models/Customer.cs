using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCBA.Models;

public class Customer
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "Must be exactly 4 digits.")]
    public int CustomerId { get; set; }

    [StringLength(50)] public string Name { get; set; }

    [StringLength(11)]
    [RegularExpression(@"^\d{3} \d{3} \d{3}$", ErrorMessage = "Must be of the format: XXX XXX XXX")]
    public string? TFN { get; set; }

    [StringLength(50)] public string? Address { get; set; }

    [StringLength(40)] public string? City { get; set; }

    [StringLength(3)]
    [RegularExpression(@"(?i)^(ACT|NSW|NT|QLD|SA|TAS|VIC|WA)$",
        ErrorMessage = "Must be a valid 2 or 3 letter Australian state code.")]
    public string? State { get; set; }

    [StringLength(4)]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "Must be exactly 4 digits.")]
    public string? PostCode { get; set; }

    [StringLength(12)]
    [RegularExpression(@"^04\d{2} \d{3} \d{3}$", ErrorMessage = "Must be of the format: 04XX XXX XXX")]
    public string? Mobile { get; set; }

    public virtual List<Account>? Accounts { get; set; }
}