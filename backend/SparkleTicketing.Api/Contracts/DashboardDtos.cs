namespace SparkleTicketing.Api.Contracts;

public sealed record DashboardSummaryDto(
    int TotalTickets,
    int OpenTickets,
    int CriticalTickets,
    int UnassignedTickets,
    int ResolvedThisWeek,
    IReadOnlyList<DashboardBucketDto> TicketsByStatus,
    IReadOnlyList<DashboardBucketDto> TicketsByPriority,
    IReadOnlyList<TicketListItemDto> RecentTickets);

public sealed record DashboardBucketDto(string Name, int Count);
