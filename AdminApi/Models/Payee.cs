using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models;

public class Payee
{
    public int PayeeId { get; set; }

    [StringLength(50)] public string Name { get; set; }

    [StringLength(50)] public string Address { get; set; }

    [StringLength(40)] public string City { get; set; }

    [StringLength(3)]
    [RegularExpression(@"(?i)^(ACT|NSW|NT|QLD|SA|TAS|VIC|WA)$",
        ErrorMessage = "Must be a valid 2 or 3 letter Australian state code.")]
    public string State { get; set; }

    [StringLength(4)]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "Must be exactly 4 digits.")]
    public string Postcode { get; set; }

    [StringLength(14)]
    [RegularExpression(@"^\(0\d\) \d{4} \d{4}$",
        ErrorMessage = "Must be of the format: (0X) XXXX XXXX")]
    public string Phone { get; set; }
}