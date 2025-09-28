using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
namespace MCBA.ViewModel;

public class ProfileViewModel
{
    // Customer info
    [Required] public string Name { get; set; }

    // Annotations for business rules for server side validation

    [RegularExpression(@"^\w{3} \w{3} \w{3}$", ErrorMessage = "Must be of the format XXX XXX XXX.")]
    [Required] public string TFN { get; set; }

    [Required] public string Address { get; set; }
    [Required] public string City { get; set; }

    [RegularExpression(@"^(NSW|VIC|QLD|SA|WA|TAS|ACT|NT)$", ErrorMessage = "Must be a valid 2- or 3-letter Australian state code.")]
    [Required] public string State { get; set; }

    [RegularExpression(@"^\d{4}$", ErrorMessage = "Must be exactly 4 digits.")]
    [Required] public string Postcode { get; set; }

    [RegularExpression(@"^04\d{2} \d{3} \d{3}$", ErrorMessage = "Must be of the format 04XX XXX XXX.")]
    [Required] public string Mobile { get; set; }

    public string? CurrentPassword { get; set; }

    public string? NewPassword { get; set; }

    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string? ConfirmPassword { get; set; }
}
