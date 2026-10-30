using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBank.Api.Data;
using SmartBank.Api.DTOs;
using SmartBank.Api.Services;

namespace SmartBank.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(SmartBankDbContext db, TokenService tokens) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email);
        if (user is null || !PasswordService.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(new LoginResponse(tokens.CreateToken(user), user.FullName, user.Email, user.Role));
    }
}
