using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SparkleTicketing.Api.Contracts;
using SparkleTicketing.Api.Data;
using SparkleTicketing.Api.Models;

namespace SparkleTicketing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LookupsController(SparkleTicketingDbContext db) : ControllerBase
{
    [HttpGet("erp-modules")]
    public async Task<ActionResult<IReadOnlyList<LookupDto>>> GetErpModules()
    {
        var modules = await db.ErpModules
            .Where(module => module.IsActive)
            .OrderBy(module => module.Name)
            .Select(module => new LookupDto(module.Id, module.Name))
            .ToListAsync();

        return Ok(modules);
    }

    [HttpGet("customers")]
    public async Task<ActionResult<IReadOnlyList<CustomerLookupDto>>> GetCustomers()
    {
        var customers = await db.Customers
            .Where(customer => customer.IsActive)
            .OrderBy(customer => customer.CompanyName)
            .Select(customer => new CustomerLookupDto(
                customer.Id,
                customer.CompanyName,
                customer.ContactPerson,
                customer.City,
                customer.Branches
                    .OrderBy(branch => branch.Name)
                    .Select(branch => new BranchLookupDto(branch.Id, branch.Name, branch.City))
                    .ToList()))
            .ToListAsync();

        return Ok(customers);
    }

    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<UserLookupDto>>> GetUsers()
    {
        var users = await db.Users
            .Where(user => user.IsActive)
            .OrderBy(user => user.FullName)
            .Select(user => new UserLookupDto(user.Id, user.FullName, user.Email, user.Role.ToString()))
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("ticket-statuses")]
    public ActionResult<IReadOnlyList<EnumLookupDto>> GetTicketStatuses()
    {
        return Ok(ToEnumLookup<TicketStatus>());
    }

    [HttpGet("ticket-priorities")]
    public ActionResult<IReadOnlyList<EnumLookupDto>> GetTicketPriorities()
    {
        return Ok(ToEnumLookup<TicketPriority>());
    }

    private static IReadOnlyList<EnumLookupDto> ToEnumLookup<TEnum>()
        where TEnum : struct, Enum
    {
        return Enum.GetNames<TEnum>()
            .Select(value => new EnumLookupDto(value, SplitPascalCase(value)))
            .ToList();
    }

    private static string SplitPascalCase(string value)
    {
        return string.Concat(value.Select((character, index) =>
            index > 0 && char.IsUpper(character) ? $" {character}" : character.ToString()));
    }
}
