# SmartBank – Banking Management System

A portfolio/demo banking application built with ASP.NET Core 10 Web API, Entity Framework Core 10, SQL Server, React, and Vite.

> **Important:** SmartBank is an educational demonstration. It uses simulated data and is not suitable for real banking, real customer data, or real-money transactions.

## Features

- JWT authentication with Admin, BankOfficer, and Customer roles
- Customer and bank-account management
- Savings and Current account types
- Deposits, withdrawals, and account-to-account transfers
- Transaction history and account summaries
- Audit log records for key banking operations
- Swagger/OpenAPI API documentation
- React dashboard with account, customer, and transaction screens
- SQL Server persistence and EF Core migrations

## Requirements

- Windows 10
- .NET 10 SDK
- Node.js 20.19+ or 22.12+
- SQL Server (Developer or Express) and SSMS
- Visual Studio Code

## 1. Configure SQL Server

Create a database named `SmartBankDb` in SSMS, or let EF Core create it when migrations are applied.

Update `backend/SmartBank.Api/appsettings.Development.json` if your SQL Server instance uses a different name:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=SmartBankDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

Common server names:
- Default instance: `localhost`
- SQL Express: `localhost\\SQLEXPRESS`
- LocalDB: `(localdb)\\MSSQLLocalDB` (if installed)

## 2. Run the backend

Open a terminal in `backend/SmartBank.Api`:

```powershell
dotnet restore
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

The API defaults to `http://localhost:5080` and `https://localhost:7080`. Swagger is available at `/swagger` in Development.

If `dotnet-ef` is already installed, update it with:

```powershell
dotnet tool update --global dotnet-ef
```

## 3. Run the frontend

Open another terminal in `frontend`:

```powershell
npm install
npm run dev
```

Open the Vite URL shown in the terminal (normally `http://localhost:5173`).

## Demo accounts

The backend seeds these demo accounts on startup:

| Role | Email | Password |
|---|---|---|
| Admin | admin@smartbank.local | Admin@12345 |
| Bank Officer | officer@smartbank.local | Officer@12345 |
| Customer | customer@smartbank.local | Customer@12345 |

Change these credentials before sharing a deployed demo. The seed accounts and secret key are for local development only.

## Main API endpoints

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/login` | Login and receive JWT |
| GET | `/api/dashboard/summary` | Dashboard totals |
| GET | `/api/customers` | List customers (Admin/BankOfficer) |
| POST | `/api/customers` | Create customer (Admin/BankOfficer) |
| GET | `/api/accounts` | List accounts |
| POST | `/api/accounts` | Open account (Admin/BankOfficer) |
| POST | `/api/transactions/deposit` | Deposit funds (Admin/BankOfficer) |
| POST | `/api/transactions/withdraw` | Withdraw funds (Admin/BankOfficer) |
| POST | `/api/transactions/transfer` | Transfer between accounts |
| GET | `/api/transactions/account/{accountId}` | Account transaction history |

All amounts are simulated LKR amounts. API requests other than login require:

`Authorization: Bearer <token>`

## Project structure

```text
SmartBank/
├── backend/SmartBank.Api/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Models/
│   ├── Services/
│   └── Program.cs
└── frontend/
    └── src/
```

## Notes and next improvements

This is a functional starter project, not a production banking platform. Before production use, add professional security review, secret management, MFA, KYC/AML workflows, idempotency keys, stronger audit controls, monitoring, backups, rate limiting, and independent penetration testing. Use decimal values and database transactions for monetary operations; never use floating-point types for money.
