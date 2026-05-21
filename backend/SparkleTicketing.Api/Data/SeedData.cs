using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SparkleTicketing.Api.Models;
using SparkleTicketing.Api.Services;

namespace SparkleTicketing.Api.Data;

public static class SeedData
{
    public const string DefaultSeedPassword = "Sparkle@123";

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SparkleTicketingDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher<AppUser>>();

        await db.Database.MigrateAsync();
        await EnsurePasswordHashesAsync(db, passwordHasher);

        if (!await db.ErpModules.AnyAsync())
        {
            db.ErpModules.AddRange(
                new ErpModule { Name = "POS Billing", Description = "Retail billing, GST invoice, and payment flows" },
                new ErpModule { Name = "Inventory", Description = "Stock, item tags, barcodes, and transfers" },
                new ErpModule { Name = "Gold Rate", Description = "Daily gold rate, making charges, and pricing rules" },
                new ErpModule { Name = "Diamond Stock", Description = "Diamond attributes, certificates, and valuation" },
                new ErpModule { Name = "Purchase", Description = "Vendor purchase, inward entries, and bills" },
                new ErpModule { Name = "Repair", Description = "Repair orders, delivery tracking, and charges" },
                new ErpModule { Name = "Karigar", Description = "Manufacturing, karigar issue/receipt, and wastage" },
                new ErpModule { Name = "Reports", Description = "Sales, stock, GST, and management reports" });
        }

        if (!await db.Users.AnyAsync())
        {
            db.Users.AddRange(
                CreateSeedUser("Aarav Mehta", "aarav@sparkleerp.local", UserRole.SupportManager, passwordHasher),
                CreateSeedUser("Nisha Shah", "nisha@sparkleerp.local", UserRole.SupportAgent, passwordHasher),
                CreateSeedUser("Rohan Iyer", "rohan@sparkleerp.local", UserRole.Developer, passwordHasher),
                CreateSeedUser("Priya Nair", "priya@sparkleerp.local", UserRole.Admin, passwordHasher));
        }

        if (!await db.Customers.AnyAsync())
        {
            db.Customers.AddRange(
                new Customer
                {
                    CompanyName = "Kundan Jewels",
                    ContactPerson = "Vikram Jain",
                    Phone = "+91 98765 10001",
                    Email = "vikram@kundanjewels.local",
                    City = "Mumbai",
                    Branches =
                    [
                        new Branch { Name = "Zaveri Bazaar", Address = "Main showroom, Zaveri Bazaar", City = "Mumbai" },
                        new Branch { Name = "Borivali", Address = "Borivali West branch", City = "Mumbai" }
                    ]
                },
                new Customer
                {
                    CompanyName = "Ratna Gold House",
                    ContactPerson = "Mehul Soni",
                    Phone = "+91 98765 10002",
                    Email = "mehul@ratnagold.local",
                    City = "Ahmedabad",
                    Branches =
                    [
                        new Branch { Name = "CG Road", Address = "CG Road showroom", City = "Ahmedabad" }
                    ]
                });
        }

        await db.SaveChangesAsync();

        if (!await db.Tickets.AnyAsync())
        {
            var customers = await db.Customers.Include(customer => customer.Branches).ToListAsync();
            var modules = await db.ErpModules.ToListAsync();
            var users = await db.Users.ToListAsync();

            var supportAgent = users.Single(user => user.Email == "nisha@sparkleerp.local");
            var developer = users.Single(user => user.Email == "rohan@sparkleerp.local");
            var manager = users.Single(user => user.Email == "aarav@sparkleerp.local");
            var kundan = customers.Single(customer => customer.CompanyName == "Kundan Jewels");
            var ratna = customers.Single(customer => customer.CompanyName == "Ratna Gold House");

            var ticketOne = new Ticket
            {
                TicketNumber = "SPK-20260520-0001",
                Title = "GST invoice total differs after gold rate update",
                Description = "Customer reports that the POS bill total changes after updating today's gold rate and reopening the invoice.",
                Customer = kundan,
                Branch = kundan.Branches.First(),
                ErpModule = modules.Single(module => module.Name == "POS Billing"),
                Priority = TicketPriority.High,
                Status = TicketStatus.InProgress,
                CreatedByUser = manager,
                AssignedToUser = supportAgent,
                CreatedAt = DateTime.UtcNow.AddHours(-8),
                UpdatedAt = DateTime.UtcNow.AddHours(-2)
            };

            var ticketTwo = new Ticket
            {
                TicketNumber = "SPK-20260520-0002",
                Title = "Diamond certificate number not visible in stock report",
                Description = "The branch can see certificate numbers on item detail but not in the diamond stock summary report.",
                Customer = ratna,
                Branch = ratna.Branches.First(),
                ErpModule = modules.Single(module => module.Name == "Diamond Stock"),
                Priority = TicketPriority.Medium,
                Status = TicketStatus.Open,
                CreatedByUser = supportAgent,
                AssignedToUser = developer,
                CreatedAt = DateTime.UtcNow.AddHours(-3),
                UpdatedAt = DateTime.UtcNow.AddHours(-3)
            };

            db.Tickets.AddRange(ticketOne, ticketTwo);
            await db.SaveChangesAsync();

            db.TicketComments.AddRange(
                new TicketComment
                {
                    TicketId = ticketOne.Id,
                    AuthorUserId = supportAgent.Id,
                    Body = "Reproduced on test branch. Checking rate recalculation sequence before invoice save.",
                    IsInternal = true,
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                },
                new TicketComment
                {
                    TicketId = ticketTwo.Id,
                    AuthorUserId = developer.Id,
                    Body = "Need report column mapping and sample exported report from customer.",
                    IsInternal = true,
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                });

            db.TicketStatusHistory.AddRange(
                new TicketStatusHistory
                {
                    TicketId = ticketOne.Id,
                    FromStatus = null,
                    ToStatus = TicketStatus.Open,
                    ChangedByUserId = manager.Id,
                    Note = "Ticket created",
                    CreatedAt = ticketOne.CreatedAt
                },
                new TicketStatusHistory
                {
                    TicketId = ticketOne.Id,
                    FromStatus = TicketStatus.Open,
                    ToStatus = TicketStatus.InProgress,
                    ChangedByUserId = supportAgent.Id,
                    Note = "Assigned to support for reproduction",
                    CreatedAt = ticketOne.UpdatedAt
                },
                new TicketStatusHistory
                {
                    TicketId = ticketTwo.Id,
                    FromStatus = null,
                    ToStatus = TicketStatus.Open,
                    ChangedByUserId = supportAgent.Id,
                    Note = "Ticket created",
                    CreatedAt = ticketTwo.CreatedAt
                });

            await db.SaveChangesAsync();
        }
    }

    private static AppUser CreateSeedUser(
        string fullName,
        string email,
        UserRole role,
        PasswordHasher<AppUser> passwordHasher)
    {
        var user = new AppUser
        {
            FullName = fullName,
            Email = email,
            Role = role,
            IsActive = true
        };

        user.PasswordHash = AuthService.HashPassword(passwordHasher, user, DefaultSeedPassword);
        return user;
    }

    private static async Task EnsurePasswordHashesAsync(
        SparkleTicketingDbContext db,
        PasswordHasher<AppUser> passwordHasher)
    {
        var usersMissingHash = await db.Users
            .Where(user => user.PasswordHash == "")
            .ToListAsync();

        if (usersMissingHash.Count == 0)
        {
            return;
        }

        foreach (var user in usersMissingHash)
        {
            user.PasswordHash = AuthService.HashPassword(passwordHasher, user, DefaultSeedPassword);
        }

        await db.SaveChangesAsync();
    }
}
