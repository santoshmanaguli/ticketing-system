namespace SparkleTicketing.Api.Models;

public sealed class Branch
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
