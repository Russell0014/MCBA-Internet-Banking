# MCBA

A simulated banking application built with ASP.NET that demonstrates core banking functionality and administrative capabilities.

## Features

### Customer Portal

- **Account Management**: View account balances and transaction history
- **Transactions**: Simulate deposits and withdrawals and transfers
- **Transfers**: Move money between accounts
- **Scheduled Payments (BillPay)**: Set up recurring or future-dated payments
- **Profile Management**: Update personal information and password

### Admin Portal
- **Administrative Tasks**: Manage Payee details and manage billpay for pending billpay transactions
- **API-driven**: Separate API and portal architecture

## Getting Started

### Installation

Clone the repository:
```bash
git clone https://github.com/rmit-wdt-s2-2025/s4001795-s4018548-a2
```

### Updating Database Connections

Before running the application, you will need to update the database connection strings in the following files with your own credentials:

- `AdminApi/appsettings.json`
- `MCBA/appsettings.json`

This ensures the application can connect to your database.

### Running the Customer Website

1. Navigate to the MCBA directory:
```bash
cd MCBA
```

2. Run the application:
```bash
dotnet run
```

3. Open your browser and navigate to the URL shown in the terminal.

### Running the Admin Website

The admin system consists of two components that need to run simultaneously:

#### Terminal 1 - Admin API
```bash
cd AdminAPI
dotnet run
```

#### Terminal 2 - Admin Portal
```bash
cd AdminPortal
dotnet run
```

The Admin Portal will connect to the Admin API automatically. Open your browser to the URL shown in the terminal.

### Running Tests

From the project root directory:
```bash
dotnet test
```

## Made by

- [Russell Sheikh](https://github.com/Russell0014)
- [Elza Sprudzans](https://github.com/elza-sprudzans)

