using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkleTicketing.Api.Contracts;
using SparkleTicketing.Api.Data;
using SparkleTicketing.Api.Models;

namespace SparkleTicketing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TicketsController(SparkleTicketingDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TicketListItemDto>>> GetTickets(
        [FromQuery] TicketStatus? status,
        [FromQuery] TicketPriority? priority,
        [FromQuery] int? moduleId,
        [FromQuery] int? assignedToUserId,
        [FromQuery] string? search)
    {
        var query = db.Tickets.AsQueryable();

        if (status is not null)
        {
            query = query.Where(ticket => ticket.Status == status);
        }

        if (priority is not null)
        {
            query = query.Where(ticket => ticket.Priority == priority);
        }

        if (moduleId is not null)
        {
            query = query.Where(ticket => ticket.ErpModuleId == moduleId);
        }

        if (assignedToUserId is not null)
        {
            query = query.Where(ticket => ticket.AssignedToUserId == assignedToUserId);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(ticket =>
                ticket.TicketNumber.Contains(term) ||
                ticket.Title.Contains(term) ||
                ticket.Customer.CompanyName.Contains(term));
        }

        var tickets = await ProjectTicketList(query
                .OrderByDescending(ticket => ticket.UpdatedAt))
            .ToListAsync();

        return Ok(tickets);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TicketDetailDto>> GetTicket(int id)
    {
        var ticket = await db.Tickets
            .Include(item => item.Customer)
            .Include(item => item.Branch)
            .Include(item => item.ErpModule)
            .Include(item => item.CreatedByUser)
            .Include(item => item.AssignedToUser)
            .Include(item => item.Comments).ThenInclude(comment => comment.Author)
            .Include(item => item.StatusHistory).ThenInclude(history => history.ChangedByUser)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        return Ok(ToDetailDto(ticket));
    }

    [HttpPost]
    public async Task<ActionResult<TicketDetailDto>> CreateTicket(CreateTicketRequest request)
    {
        var validationError = await ValidateCreateTicketRequest(request);
        if (validationError is not null)
        {
            return BadRequest(new { message = validationError });
        }

        var ticket = new Ticket
        {
            TicketNumber = await CreateTicketNumber(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            CustomerId = request.CustomerId,
            BranchId = request.BranchId,
            ErpModuleId = request.ErpModuleId,
            Priority = request.Priority,
            Status = TicketStatus.Open,
            CreatedByUserId = request.CreatedByUserId,
            AssignedToUserId = request.AssignedToUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Tickets.Add(ticket);
        await db.SaveChangesAsync();

        db.TicketStatusHistory.Add(new TicketStatusHistory
        {
            TicketId = ticket.Id,
            FromStatus = null,
            ToStatus = TicketStatus.Open,
            ChangedByUserId = request.CreatedByUserId,
            Note = "Ticket created",
            CreatedAt = ticket.CreatedAt
        });

        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, await LoadDetailDto(ticket.Id));
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<TicketDetailDto>> UpdateStatus(int id, UpdateTicketStatusRequest request)
    {
        var ticket = await db.Tickets.FindAsync(id);
        if (ticket is null)
        {
            return NotFound();
        }

        var userExists = await db.Users.AnyAsync(user => user.Id == request.ChangedByUserId);
        if (!userExists)
        {
            return BadRequest(new { message = "ChangedByUserId does not exist." });
        }

        var previousStatus = ticket.Status;
        ticket.Status = request.Status;
        ticket.UpdatedAt = DateTime.UtcNow;

        if (request.Status == TicketStatus.Resolved)
        {
            ticket.ResolvedAt = DateTime.UtcNow;
        }

        if (request.Status == TicketStatus.Closed)
        {
            ticket.ClosedAt = DateTime.UtcNow;
        }

        db.TicketStatusHistory.Add(new TicketStatusHistory
        {
            TicketId = ticket.Id,
            FromStatus = previousStatus,
            ToStatus = request.Status,
            ChangedByUserId = request.ChangedByUserId,
            Note = request.Note,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        return Ok(await LoadDetailDto(ticket.Id));
    }

    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<TicketDetailDto>> AddComment(int id, AddTicketCommentRequest request)
    {
        var ticket = await db.Tickets.FindAsync(id);
        if (ticket is null)
        {
            return NotFound();
        }

        var authorExists = await db.Users.AnyAsync(user => user.Id == request.AuthorUserId);
        if (!authorExists)
        {
            return BadRequest(new { message = "AuthorUserId does not exist." });
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest(new { message = "Comment body is required." });
        }

        db.TicketComments.Add(new TicketComment
        {
            TicketId = ticket.Id,
            AuthorUserId = request.AuthorUserId,
            Body = request.Body.Trim(),
            IsInternal = request.IsInternal,
            CreatedAt = DateTime.UtcNow
        });

        ticket.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return Ok(await LoadDetailDto(ticket.Id));
    }

    private async Task<string?> ValidateCreateTicketRequest(CreateTicketRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return "Title is required.";
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return "Description is required.";
        }

        if (!await db.Customers.AnyAsync(customer => customer.Id == request.CustomerId))
        {
            return "CustomerId does not exist.";
        }

        if (request.BranchId is not null)
        {
            var branchMatchesCustomer = await db.Branches.AnyAsync(branch =>
                branch.Id == request.BranchId && branch.CustomerId == request.CustomerId);

            if (!branchMatchesCustomer)
            {
                return "BranchId does not belong to the selected customer.";
            }
        }

        if (!await db.ErpModules.AnyAsync(module => module.Id == request.ErpModuleId))
        {
            return "ErpModuleId does not exist.";
        }

        if (!await db.Users.AnyAsync(user => user.Id == request.CreatedByUserId))
        {
            return "CreatedByUserId does not exist.";
        }

        if (request.AssignedToUserId is not null &&
            !await db.Users.AnyAsync(user => user.Id == request.AssignedToUserId))
        {
            return "AssignedToUserId does not exist.";
        }

        return null;
    }

    private async Task<string> CreateTicketNumber()
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        var countToday = await db.Tickets.CountAsync(ticket => ticket.TicketNumber.StartsWith($"SPK-{today}"));
        return $"SPK-{today}-{countToday + 1:0000}";
    }

    private async Task<TicketDetailDto> LoadDetailDto(int id)
    {
        var ticket = await db.Tickets
            .Include(item => item.Customer)
            .Include(item => item.Branch)
            .Include(item => item.ErpModule)
            .Include(item => item.CreatedByUser)
            .Include(item => item.AssignedToUser)
            .Include(item => item.Comments).ThenInclude(comment => comment.Author)
            .Include(item => item.StatusHistory).ThenInclude(history => history.ChangedByUser)
            .SingleAsync(item => item.Id == id);

        return ToDetailDto(ticket);
    }

    private static IQueryable<TicketListItemDto> ProjectTicketList(IQueryable<Ticket> query)
    {
        return query.Select(ticket => new TicketListItemDto(
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

    private static TicketDetailDto ToDetailDto(Ticket ticket)
    {
        return new TicketDetailDto(
            ticket.Id,
            ticket.TicketNumber,
            ticket.Title,
            ticket.Description,
            ticket.Status.ToString(),
            ticket.Priority.ToString(),
            ticket.CustomerId,
            ticket.Customer.CompanyName,
            ticket.BranchId,
            ticket.Branch?.Name,
            ticket.ErpModuleId,
            ticket.ErpModule.Name,
            ticket.CreatedByUserId,
            ticket.CreatedByUser.FullName,
            ticket.AssignedToUserId,
            ticket.AssignedToUser?.FullName,
            ticket.CreatedAt,
            ticket.UpdatedAt,
            ticket.ResolvedAt,
            ticket.ClosedAt,
            ticket.Comments
                .OrderBy(comment => comment.CreatedAt)
                .Select(comment => new TicketCommentDto(
                    comment.Id,
                    comment.Author.FullName,
                    comment.Body,
                    comment.IsInternal,
                    comment.CreatedAt))
                .ToList(),
            ticket.StatusHistory
                .OrderBy(history => history.CreatedAt)
                .Select(history => new TicketStatusHistoryDto(
                    history.Id,
                    history.FromStatus?.ToString(),
                    history.ToStatus.ToString(),
                    history.ChangedByUser?.FullName,
                    history.Note,
                    history.CreatedAt))
                .ToList());
    }
}
