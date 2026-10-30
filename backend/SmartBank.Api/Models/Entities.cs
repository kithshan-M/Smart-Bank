namespace SmartBank.Api.Models;

public sealed class AppUser
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Customer";
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
}

public sealed class Customer
{
    public int Id { get; set; }
    public string CustomerNumber { get; set; } = "";
    public string FullName { get; set; } = "";
    public string NicNumber { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<BankAccount> Accounts { get; set; } = new List<BankAccount>();
}

public sealed class BankAccount
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = "";
    public string AccountType { get; set; } = "Savings";
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "LKR";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
}

public sealed class BankTransaction
{
    public int Id { get; set; }
    public string Reference { get; set; } = "";
    public string Type { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public int AccountId { get; set; }
    public BankAccount Account { get; set; } = null!;
}

public sealed class AuditLog
{
    public int Id { get; set; }
    public string Action { get; set; } = "";
    public string Details { get; set; } = "";
    public string ActorEmail { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
