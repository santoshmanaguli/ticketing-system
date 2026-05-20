namespace SparkleTicketing.Api.Models;

public sealed class Customer
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
