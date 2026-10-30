using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBank.Api.Data;

namespace SmartBank.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class DashboardController(SmartBankDbContext db) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary()
    {
        var accounts = db.Accounts.AsQueryable();
        if (User.IsInRole("Customer"))
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)!.Value;
            var customer = await db.Customers.SingleOrDefaultAsync(x => x.Email == email);
            accounts = customer is null ? accounts.Where(_ => false) : accounts.Where(x => x.CustomerId == customer.Id);
        }
        var totalAccounts = await accounts.CountAsync();
        var totalBalance = await accounts.SumAsync(x => (decimal?)x.Balance) ?? 0m;
        var customerCount = User.IsInRole("Customer") ? 1 : await db.Customers.CountAsync();
        var visibleAccountIds = accounts.Select(x => x.Id);
        var recent = await db.Transactions
            .Where(x => visibleAccountIds.Contains(x.AccountId))
            .OrderByDescending(x => x.CreatedAtUtc).Take(8)
            .Select(x => new { x.Reference, x.Type, x.Amount, x.CreatedAtUtc, AccountNumber = x.Account.AccountNumber })
            .ToListAsync();
        return Ok(new { customerCount, totalAccounts, totalBalance, currency = "LKR", recentTransactions = recent });
    }
}
