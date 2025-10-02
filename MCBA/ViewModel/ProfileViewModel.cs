using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
namespace MCBA.ViewModel;

public class ProfileViewModel
{
    public int CustomerId { get; set; } 
    // Customer info
    [Required] public string Name { get; set; }

    // Annotations for business rules for server side validation

    [RegularExpression(@"^\w{3} \w{3} \w{3}$", ErrorMessage = "Must be of the format XXX XXX XXX.")]
    public string? TFN { get; set; }

    [Required] public string Address { get; set; }
    public string? City { get; set; }

    [RegularExpression(@"^(NSW|VIC|QLD|SA|WA|TAS|ACT|NT)$", ErrorMessage = "Must be a valid 2- or 3-letter Australian state code.")]
    public string? State { get; set; }

    [RegularExpression(@"^\d{4}$", ErrorMessage = "Must be exactly 4 digits.")]
    public string? PostCode { get; set; }

    [RegularExpression(@"^04\d{2} \d{3} \d{3}$", ErrorMessage = "Must be of the format 04XX XXX XXX.")]
    public string? Mobile { get; set; }

}
