using Microsoft.EntityFrameworkCore;
using SmartBank.Api.Data;
using SmartBank.Api.Models;

namespace SmartBank.Api.Services;

public sealed class TransactionService(SmartBankDbContext db)
{
    public async Task<(bool Success, string Message, object? Data)> DepositAsync(int accountId, decimal amount, string? description, string actor)
    {
        if (amount <= 0) return (false, "Amount must be greater than zero.", null);
        await using var tx = await db.Database.BeginTransactionAsync();
        var account = await db.Accounts.SingleOrDefaultAsync(x => x.Id == accountId);
        if (account is null || !account.IsActive) return (false, "Active account not found.", null);
        account.Balance += amount;
        var record = NewTransaction(account, "Deposit", amount, description);
        db.Transactions.Add(record);
        db.AuditLogs.Add(new AuditLog { Action = "Deposit", Details = $"{amount:0.00} deposited to {account.AccountNumber}.", ActorEmail = actor });
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return (true, "Deposit completed.", new { record.Reference, account.Balance });
    }

    public async Task<(bool Success, string Message, object? Data)> WithdrawAsync(int accountId, decimal amount, string? description, string actor)
    {
        if (amount <= 0) return (false, "Amount must be greater than zero.", null);
        await using var tx = await db.Database.BeginTransactionAsync();
        var account = await db.Accounts.SingleOrDefaultAsync(x => x.Id == accountId);
        if (account is null || !account.IsActive) return (false, "Active account not found.", null);
        if (account.Balance < amount) return (false, "Insufficient funds.", null);
        account.Balance -= amount;
        var record = NewTransaction(account, "Withdrawal", -amount, description);
        db.Transactions.Add(record);
        db.AuditLogs.Add(new AuditLog { Action = "Withdrawal", Details = $"{amount:0.00} withdrawn from {account.AccountNumber}.", ActorEmail = actor });
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return (true, "Withdrawal completed.", new { record.Reference, account.Balance });
    }

    public async Task<(bool Success, string Message, object? Data)> TransferAsync(int fromId, int toId, decimal amount, string? description, string actor)
    {
        if (amount <= 0) return (false, "Amount must be greater than zero.", null);
        if (fromId == toId) return (false, "Source and destination accounts must be different.", null);
        await using var tx = await db.Database.BeginTransactionAsync();
        var ids = new[] { fromId, toId };
        var accounts = await db.Accounts.Where(x => ids.Contains(x.Id)).OrderBy(x => x.Id).ToListAsync();
        if (accounts.Count != 2 || accounts.Any(x => !x.IsActive)) return (false, "Both accounts must exist and be active.", null);
        var source = accounts.Single(x => x.Id == fromId);
        var destination = accounts.Single(x => x.Id == toId);
        if (source.Currency != destination.Currency) return (false, "Accounts must use the same currency.", null);
        if (source.Balance < amount) return (false, "Insufficient funds.", null);

        source.Balance -= amount;
        destination.Balance += amount;
        var debit = NewTransaction(source, "TransferOut", -amount, description ?? $"Transfer to {destination.AccountNumber}");
        var credit = NewTransaction(destination, "TransferIn", amount, description ?? $"Transfer from {source.AccountNumber}");
        db.Transactions.AddRange(debit, credit);
        db.AuditLogs.Add(new AuditLog { Action = "Transfer", Details = $"{amount:0.00} transferred from {source.AccountNumber} to {destination.AccountNumber}.", ActorEmail = actor });
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return (true, "Transfer completed.", new { debit.Reference, CreditReference = credit.Reference, SourceBalance = source.Balance, DestinationBalance = destination.Balance });
    }

    private static BankTransaction NewTransaction(BankAccount account, string type, decimal amount, string? description) => new()
    {
        Reference = $"TXN-{Guid.NewGuid():N}"[..16].ToUpperInvariant(),
        Type = type,
        Amount = amount,
        BalanceAfter = account.Balance,
        Description = description,
        AccountId = account.Id
    };
}
