using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCBA.Models;

public class Customer
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int CustomerId { get; set; }

    [StringLength(50)] public string Name { get; set; }

    [StringLength(11)] public string? TFN { get; set; }

    [StringLength(50)] public string? Address { get; set; }

    [StringLength(40)] public string? City { get; set; }

    [StringLength(3)] public string? State { get; set; }

    [StringLength(4)] public string? PostCode { get; set; }

    [StringLength(12)] public string? Mobile { get; set; }

    public virtual List<Account>? Accounts { get; set; }
}