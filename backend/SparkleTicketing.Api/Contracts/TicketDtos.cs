using SparkleTicketing.Api.Models;

namespace SparkleTicketing.Api.Contracts;

public sealed record TicketListItemDto(
    int Id,
    string TicketNumber,
    string Title,
    string Status,
    string Priority,
    string CustomerName,
    string? BranchName,
    string ModuleName,
    string? AssignedTo,
    string CreatedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record TicketDetailDto(
    int Id,
    string TicketNumber,
    string Title,
    string Description,
    string Status,
    string Priority,
    int CustomerId,
    string CustomerName,
    int? BranchId,
    string? BranchName,
    int ErpModuleId,
    string ModuleName,
    int CreatedByUserId,
    string CreatedBy,
    int? AssignedToUserId,
    string? AssignedTo,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? ResolvedAt,
    DateTime? ClosedAt,
    IReadOnlyList<TicketCommentDto> Comments,
    IReadOnlyList<TicketStatusHistoryDto> StatusHistory);

public sealed record TicketCommentDto(
    int Id,
    string Author,
    string Body,
    bool IsInternal,
    DateTime CreatedAt);

public sealed record TicketStatusHistoryDto(
    int Id,
    string? FromStatus,
    string ToStatus,
    string? ChangedBy,
    string? Note,
    DateTime CreatedAt);

public sealed record CreateTicketRequest(
    string Title,
    string Description,
    int CustomerId,
    int? BranchId,
    int ErpModuleId,
    TicketPriority Priority,
    int CreatedByUserId,
    int? AssignedToUserId);

public sealed record UpdateTicketStatusRequest(
    TicketStatus Status,
    int ChangedByUserId,
    string? Note);

public sealed record AddTicketCommentRequest(
    int AuthorUserId,
    string Body,
    bool IsInternal);
