using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminApi.Models;

public class Login
{
    [Column(TypeName = "char")]
    [StringLength(8)]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "Must be exactly 8 digits.")]
    
    [Display(Name = "Login ID")]
    public string LoginId { get; set; }

    public int CustomerId { get; set; }
    public virtual Customer Customer { get; set; }

    [Column(TypeName = "char")]
    [StringLength(94)]
    public string PasswordHash { get; set; }
}