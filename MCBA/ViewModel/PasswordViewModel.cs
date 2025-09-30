using System.ComponentModel.DataAnnotations;

namespace MCBA.ViewModel;

public class PasswordViewModel // view model for dealing with the password changes
{
    public int CustomerId { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; }  // user current password

    [Required]
    [DataType(DataType.Password)]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    public string NewPassword { get; set; } // new password

    [Required]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } // confirm new password
}
