using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCBA.Models;

public class Login
{
    [Column(TypeName = "char")]
    [StringLength(8)]
    public string LoginId { get; set; }

    public int CustomerId { get; set; }
    public virtual Customer Customer { get; set; }

    [Column(TypeName = "char")]
    [StringLength(94)]
    public string PasswordHash { get; set; }
}