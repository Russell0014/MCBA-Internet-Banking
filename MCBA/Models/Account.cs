using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCBA.Models;

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

    public int CustomerId { get; set; }
    public virtual Customer Customer { get; set; }

    [Column(TypeName = "money")]
    [DataType(DataType.Currency)]
    public decimal Balance { get; set; }

    // Set ambiguous navigation property with InverseProperty annotation
    [InverseProperty("Account")] public virtual List<Transaction> Transactions { get; set; }
}