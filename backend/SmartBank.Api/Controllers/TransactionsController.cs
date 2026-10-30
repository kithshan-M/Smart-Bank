using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBank.Api.Data;
using SmartBank.Api.DTOs;
using SmartBank.Api.Services;

namespace SmartBank.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,BankOfficer")]
public sealed class TransactionsController(SmartBankDbContext db, TransactionService service) : ControllerBase
{
    private string Actor => User.Identity?.Name ?? "unknown";

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit(MoneyRequest request)
    {
        var result = await service.DepositAsync(request.AccountId, request.Amount, request.Description, Actor);
        return result.Success ? Ok(new { result.Message, result.Data }) : BadRequest(new { result.Message });
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw(MoneyRequest request)
    {
        var result = await service.WithdrawAsync(request.AccountId, request.Amount, request.Description, Actor);
        return result.Success ? Ok(new { result.Message, result.Data }) : BadRequest(new { result.Message });
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer(TransferRequest request)
    {
        var result = await service.TransferAsync(request.FromAccountId, request.ToAccountId, request.Amount, request.Description, Actor);
        return result.Success ? Ok(new { result.Message, result.Data }) : BadRequest(new { result.Message });
    }

    [HttpGet("account/{accountId:int}")]
    public async Task<IActionResult> AccountHistory(int accountId)
    {
        var rows = await db.Transactions.Where(x => x.AccountId == accountId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new { x.Id, x.Reference, x.Type, x.Amount, x.BalanceAfter, x.Description, x.CreatedAtUtc })
            .ToListAsync();
        return Ok(rows);
    }
}
