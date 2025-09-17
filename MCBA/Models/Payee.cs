using System.ComponentModel.DataAnnotations;

namespace MCBA.Models;

public class Payee
{
    public int PayeeId { get; set; }

    [StringLength(50)] public string Name { get; set; }

    [StringLength(50)] public string Address { get; set; }

    [StringLength(40)] public string City { get; set; }

    [StringLength(3)] public string State { get; set; }

    [StringLength(4)] public string Postcode { get; set; }

    [StringLength(14)] public string Phone { get; set; }
}