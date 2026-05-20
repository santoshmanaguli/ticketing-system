namespace SparkleTicketing.Api.Models;

public sealed class TicketStatusHistory
{
    public int Id { get; set; }

    public int TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public TicketStatus? FromStatus { get; set; }
    public TicketStatus ToStatus { get; set; }

    public int? ChangedByUserId { get; set; }
    public AppUser? ChangedByUser { get; set; }

    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
