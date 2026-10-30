using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBank.Api.Data;
using SmartBank.Api.DTOs;
using SmartBank.Api.Models;

namespace SmartBank.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,BankOfficer")]
public sealed class CustomersController(SmartBankDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.Customers.Include(x => x.Accounts).OrderBy(x => x.FullName)
            .Select(x => new { x.Id, x.CustomerNumber, x.FullName, x.NicNumber, x.Email, x.Phone, AccountCount = x.Accounts.Count })
            .ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(CreateCustomerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.NicNumber))
            return BadRequest(new { message = "Full name and NIC are required." });
        var nic = request.NicNumber.Trim();
        if (await db.Customers.AnyAsync(x => x.NicNumber == nic))
            return Conflict(new { message = "A customer with this NIC already exists." });

        var customer = new Customer
        {
            CustomerNumber = $"CUST-{DateTime.UtcNow:yyMMddHHmmss}",
            FullName = request.FullName.Trim(),
            NicNumber = nic,
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = request.Phone.Trim()
        };
        db.Customers.Add(customer);
        db.AuditLogs.Add(new AuditLog { Action = "CustomerCreated", Details = $"Customer {customer.CustomerNumber} created.", ActorEmail = User.Identity?.Name ?? "unknown" });
        await db.SaveChangesAsync();
        return Created($"/api/customers/{customer.Id}", new { customer.Id, customer.CustomerNumber, customer.FullName });
    }
}
