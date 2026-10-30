using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBank.Api.Data;
using SmartBank.Api.DTOs;
using SmartBank.Api.Models;

namespace SmartBank.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AccountsController(SmartBankDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = db.Accounts.Include(x => x.Customer).AsQueryable();
        if (User.IsInRole("Customer"))
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)!.Value;
            var customer = await db.Customers.SingleOrDefaultAsync(x => x.Email == email);
            if (customer is null) return Ok(Array.Empty<object>());
            query = query.Where(x => x.CustomerId == customer.Id);
        }
        return Ok(await query.OrderBy(x => x.AccountNumber)
            .Select(x => new { x.Id, x.AccountNumber, x.AccountType, x.Balance, x.Currency, x.IsActive, CustomerName = x.Customer.FullName, x.CustomerId })
            .ToListAsync());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,BankOfficer")]
    public async Task<IActionResult> Create(CreateAccountRequest request)
    {
        if (request.AccountType is not ("Savings" or "Current"))
            return BadRequest(new { message = "Account type must be Savings or Current." });
        var customer = await db.Customers.FindAsync(request.CustomerId);
        if (customer is null) return NotFound(new { message = "Customer not found." });

        var account = new BankAccount
        {
            AccountNumber = $"10{DateTime.UtcNow:yyMMddHHmmssfff}",
            AccountType = request.AccountType,
            CustomerId = customer.Id
        };
        db.Accounts.Add(account);
        db.AuditLogs.Add(new AuditLog { Action = "AccountOpened", Details = $"Account {account.AccountNumber} opened.", ActorEmail = User.Identity?.Name ?? "unknown" });
        await db.SaveChangesAsync();
        return Created($"/api/accounts/{account.Id}", new { account.Id, account.AccountNumber, account.AccountType, account.Balance });
    }
}
