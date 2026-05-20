using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkleTicketing.Api.Contracts;
using SparkleTicketing.Api.Data;
using SparkleTicketing.Api.Models;

namespace SparkleTicketing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DashboardController(SparkleTicketingDbContext db) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var weekStart = DateTime.UtcNow.AddDays(-7);

        var totalTickets = await db.Tickets.CountAsync();
        var openTickets = await db.Tickets.CountAsync(ticket =>
            ticket.Status != TicketStatus.Resolved && ticket.Status != TicketStatus.Closed);
        var criticalTickets = await db.Tickets.CountAsync(ticket => ticket.Priority == TicketPriority.Critical);
        var unassignedTickets = await db.Tickets.CountAsync(ticket => ticket.AssignedToUserId == null);
        var resolvedThisWeek = await db.Tickets.CountAsync(ticket =>
            ticket.ResolvedAt != null && ticket.ResolvedAt >= weekStart);

        var ticketsByStatus = await db.Tickets
            .GroupBy(ticket => ticket.Status)
            .Select(group => new DashboardBucketDto(group.Key.ToString(), group.Count()))
            .ToListAsync();

        var ticketsByPriority = await db.Tickets
            .GroupBy(ticket => ticket.Priority)
            .Select(group => new DashboardBucketDto(group.Key.ToString(), group.Count()))
            .ToListAsync();

        var recentTickets = await TicketProjection(db.Tickets
                .OrderByDescending(ticket => ticket.CreatedAt)
                .Take(6))
            .ToListAsync();

        return Ok(new DashboardSummaryDto(
            totalTickets,
            openTickets,
            criticalTickets,
            unassignedTickets,
            resolvedThisWeek,
            ticketsByStatus,
            ticketsByPriority,
            recentTickets));
    }

    private static IQueryable<TicketListItemDto> TicketProjection(IQueryable<Ticket> tickets)
    {
        return tickets.Select(ticket => new TicketListItemDto(
            ticket.Id,
            ticket.TicketNumber,
            ticket.Title,
            ticket.Status.ToString(),
            ticket.Priority.ToString(),
            ticket.Customer.CompanyName,
            ticket.Branch != null ? ticket.Branch.Name : null,
            ticket.ErpModule.Name,
            ticket.AssignedToUser != null ? ticket.AssignedToUser.FullName : null,
            ticket.CreatedByUser.FullName,
            ticket.CreatedAt,
            ticket.UpdatedAt));
    }
}
