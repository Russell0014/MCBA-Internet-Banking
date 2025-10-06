using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
namespace AdminApi.Models;

public enum AccountType
{
    Checking = 1,
    Savings = 2
}

public class Account
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Display(Name = "Account Number")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "Must be exactly 4 digits.")]
    public int AccountNumber { get; set; }

    [Display(Name = "Type")]
    [EnumDataType(typeof(AccountType), ErrorMessage = "Invalid account type.")]
    public AccountType AccountType { get; set; }

    [Column(TypeName = "money")]
    [DataType(DataType.Currency)]
    public decimal Balance { get; set; }

}