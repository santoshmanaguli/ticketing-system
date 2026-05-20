using Microsoft.EntityFrameworkCore;
using SparkleTicketing.Api.Models;

namespace SparkleTicketing.Api.Data;

public sealed class SparkleTicketingDbContext(DbContextOptions<SparkleTicketingDbContext> options)
    : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<ErpModule> ErpModules => Set<ErpModule>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TicketStatusHistory> TicketStatusHistory => Set<TicketStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.FullName).HasMaxLength(120).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(160).IsRequired();
            entity.Property(user => user.Role).HasConversion<string>().HasMaxLength(40).IsRequired();
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.Property(customer => customer.CompanyName).HasMaxLength(180).IsRequired();
            entity.Property(customer => customer.ContactPerson).HasMaxLength(120).IsRequired();
            entity.Property(customer => customer.Phone).HasMaxLength(30).IsRequired();
            entity.Property(customer => customer.Email).HasMaxLength(160).IsRequired();
            entity.Property(customer => customer.City).HasMaxLength(80).IsRequired();
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.ToTable("Branches");
            entity.Property(branch => branch.Name).HasMaxLength(120).IsRequired();
            entity.Property(branch => branch.Address).HasMaxLength(240).IsRequired();
            entity.Property(branch => branch.City).HasMaxLength(80).IsRequired();

            entity
                .HasOne(branch => branch.Customer)
                .WithMany(customer => customer.Branches)
                .HasForeignKey(branch => branch.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ErpModule>(entity =>
        {
            entity.ToTable("ErpModules");
            entity.Property(module => module.Name).HasMaxLength(120).IsRequired();
            entity.Property(module => module.Description).HasMaxLength(300).IsRequired();
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("Tickets");
            entity.HasIndex(ticket => ticket.TicketNumber).IsUnique();
            entity.Property(ticket => ticket.TicketNumber).HasMaxLength(40).IsRequired();
            entity.Property(ticket => ticket.Title).HasMaxLength(180).IsRequired();
            entity.Property(ticket => ticket.Description).HasMaxLength(4000).IsRequired();
            entity.Property(ticket => ticket.Priority).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(ticket => ticket.Status).HasConversion<string>().HasMaxLength(40).IsRequired();

            entity
                .HasOne(ticket => ticket.Customer)
                .WithMany(customer => customer.Tickets)
                .HasForeignKey(ticket => ticket.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasOne(ticket => ticket.Branch)
                .WithMany(branch => branch.Tickets)
                .HasForeignKey(ticket => ticket.BranchId)
                .OnDelete(DeleteBehavior.SetNull);

            entity
                .HasOne(ticket => ticket.ErpModule)
                .WithMany(module => module.Tickets)
                .HasForeignKey(ticket => ticket.ErpModuleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasOne(ticket => ticket.CreatedByUser)
                .WithMany(user => user.CreatedTickets)
                .HasForeignKey(ticket => ticket.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasOne(ticket => ticket.AssignedToUser)
                .WithMany(user => user.AssignedTickets)
                .HasForeignKey(ticket => ticket.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TicketComment>(entity =>
        {
            entity.ToTable("TicketComments");
            entity.Property(comment => comment.Body).HasMaxLength(4000).IsRequired();

            entity
                .HasOne(comment => comment.Ticket)
                .WithMany(ticket => ticket.Comments)
                .HasForeignKey(comment => comment.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(comment => comment.Author)
                .WithMany(user => user.Comments)
                .HasForeignKey(comment => comment.AuthorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TicketStatusHistory>(entity =>
        {
            entity.ToTable("TicketStatusHistory");
            entity.Property(history => history.FromStatus).HasConversion<string>().HasMaxLength(40);
            entity.Property(history => history.ToStatus).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(history => history.Note).HasMaxLength(800);

            entity
                .HasOne(history => history.Ticket)
                .WithMany(ticket => ticket.StatusHistory)
                .HasForeignKey(history => history.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(history => history.ChangedByUser)
                .WithMany()
                .HasForeignKey(history => history.ChangedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
