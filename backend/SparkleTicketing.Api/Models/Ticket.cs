namespace SparkleTicketing.Api.Models;

public sealed class Ticket
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public int ErpModuleId { get; set; }
    public ErpModule ErpModule { get; set; } = null!;

    public int CreatedByUserId { get; set; }
    public AppUser CreatedByUser { get; set; } = null!;

    public int? AssignedToUserId { get; set; }
    public AppUser? AssignedToUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    public ICollection<TicketStatusHistory> StatusHistory { get; set; } = new List<TicketStatusHistory>();
}
