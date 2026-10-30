namespace SmartBank.Api.DTOs;

public sealed record LoginRequest(string Email, string Password);
public sealed record LoginResponse(string Token, string FullName, string Email, string Role);
public sealed record CreateCustomerRequest(string FullName, string NicNumber, string Email, string Phone);
public sealed record CreateAccountRequest(int CustomerId, string AccountType);
public sealed record MoneyRequest(int AccountId, decimal Amount, string? Description);
public sealed record TransferRequest(int FromAccountId, int ToAccountId, decimal Amount, string? Description);
