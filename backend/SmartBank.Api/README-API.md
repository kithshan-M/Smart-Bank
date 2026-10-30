# API quick reference

Base URL: `http://localhost:5080`

## Login
`POST /api/auth/login`
```json
{ "email": "admin@smartbank.local", "password": "Admin@12345" }
```

## Create customer
`POST /api/customers` (Admin or BankOfficer)
```json
{ "fullName": "Nimal Perera", "nicNumber": "199012345678", "email": "nimal@example.com", "phone": "0771234567" }
```

## Open account
`POST /api/accounts` (Admin or BankOfficer)
```json
{ "customerId": 1, "accountType": "Savings" }
```

## Deposit / withdraw
`POST /api/transactions/deposit` or `/api/transactions/withdraw`
```json
{ "accountId": 1, "amount": 1500.00, "description": "Counter deposit" }
```

## Transfer
`POST /api/transactions/transfer`
```json
{ "fromAccountId": 1, "toAccountId": 2, "amount": 500.00, "description": "Own account transfer" }
```
