using Microsoft.EntityFrameworkCore;
using SmartBank.Api.Models;
using SmartBank.Api.Services;

namespace SmartBank.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(SmartBankDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        var customer = new Customer
        {
            CustomerNumber = "CUST-10001",
            FullName = "Demo Customer",
            NicNumber = "199900000000",
            Email = "customer@smartbank.local",
            Phone = "0770000000"
        };
        var account = new BankAccount
        {
            AccountNumber = "100000000001",
            AccountType = "Savings",
            Balance = 250000m,
            Customer = customer
        };
        db.Customers.Add(customer);
        db.Accounts.Add(account);
        db.Users.AddRange(
            new AppUser { FullName = "SmartBank Administrator", Email = "admin@smartbank.local", PasswordHash = PasswordService.Hash("Admin@12345"), Role = "Admin" },
            new AppUser { FullName = "Demo Bank Officer", Email = "officer@smartbank.local", PasswordHash = PasswordService.Hash("Officer@12345"), Role = "BankOfficer" },
            new AppUser { FullName = "Demo Customer", Email = "customer@smartbank.local", PasswordHash = PasswordService.Hash("Customer@12345"), Role = "Customer", Customer = customer }
        );
        await db.SaveChangesAsync();
    }
}
