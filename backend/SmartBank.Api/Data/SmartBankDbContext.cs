using Microsoft.EntityFrameworkCore;
using SmartBank.Api.Models;

namespace SmartBank.Api.Data;

public sealed class SmartBankDbContext(DbContextOptions<SmartBankDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<BankAccount> Accounts => Set<BankAccount>();
    public DbSet<BankTransaction> Transactions => Set<BankTransaction>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Customer>().HasIndex(x => x.CustomerNumber).IsUnique();
        modelBuilder.Entity<Customer>().HasIndex(x => x.NicNumber).IsUnique();
        modelBuilder.Entity<BankAccount>().HasIndex(x => x.AccountNumber).IsUnique();
        modelBuilder.Entity<BankTransaction>().HasIndex(x => x.Reference).IsUnique();

        modelBuilder.Entity<BankAccount>().Property(x => x.Balance).HasPrecision(18, 2);
        modelBuilder.Entity<BankTransaction>().Property(x => x.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<BankTransaction>().Property(x => x.BalanceAfter).HasPrecision(18, 2);

        modelBuilder.Entity<AppUser>()
            .HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Customer>()
            .HasMany(x => x.Accounts)
            .WithOne(x => x.Customer)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BankAccount>()
            .HasMany<BankTransaction>()
            .WithOne(x => x.Account)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
