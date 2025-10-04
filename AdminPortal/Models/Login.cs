using System.ComponentModel.DataAnnotations;

namespace AdminPortal.Models;

public class Login
{
    [Display(Name = "Username")]
    public string UserName { get; set; }
    public string Password { get; set; }
}